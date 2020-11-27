using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace CommonLib
{
	public static class utils
	{
		public struct ListIterator_s<T>
		{
			public static ListIterator_s<T> empty = new ListIterator_s<T>(new List<T>());

			private List<T> list;

			private int index;

			public bool MoveNext
			{
				get
				{
					index++;
					return (uint)index < (uint)list.Count;
				}
			}

			public int Index => index;

			public T Value
			{
				get
				{
					return list[index];
				}
				set
				{
					list[index] = value;
				}
			}

			internal ListIterator_s(List<T> list)
			{
				this.list = list;
				index = -1;
			}

			public void remove()
			{
				list.RemoveAt(index);
				index--;
			}
		}

		public struct ArrayIterator_s<T>
		{
			public static ArrayIterator_s<T> empty = new ArrayIterator_s<T>(new T[0]);

			private T[] thisArray;

			private int index;

			public bool MoveNext
			{
				get
				{
					index++;
					return (uint)index < (uint)thisArray.Length;
				}
			}

			public int Index => index;

			public T Value
			{
				get
				{
					return thisArray[index];
				}
				set
				{
					thisArray[index] = value;
				}
			}

			public T[] Array => thisArray;

			internal ArrayIterator_s(T[] a)
			{
				thisArray = a;
				index = -1;
			}

			public void remove(ref T[] a)
			{
				array.remove_at(ref a, index);
				thisArray = a;
				index--;
			}
		}

		private static Action<string, string, LogImportanceEnum> logListenerDelegate;

		public static Action<Exception> showErrorReportDelegate;

		public static FpsCounter fpsCounter = new FpsCounter();

		public static RandomGenerator random = new RandomGenerator();

		public static string branchName = "release05";

		private static string _svnRevision;

		[ThreadStatic]
		private static StringBuilder stringBuilder;

		public static Dictionary<string, int> lineNumberByFile = new Dictionary<string, int>();

		public static List<string> logLineList = new List<string>();

		private static List<Thread> workerThreadList = new List<Thread>();

		private static volatile bool workerThreadsShouldTerminate;

		[ThreadStatic]
		private static bool isMainThread;

		public static bool WorkerThreadsShouldTerminate
		{
			get
			{
				return workerThreadsShouldTerminate;
			}
			set
			{
				if (!value)
				{
					throw new InvalidOperationException();
				}
				workerThreadsShouldTerminate = true;
			}
		}

		public static void rec_line_number([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			lineNumberByFile[sourceFilePath] = sourceLineNumber;
		}

		public static float minutes(this float minutes)
		{
			return minutes * 60f;
		}

		public static float minutes(this int minutes)
		{
			return minutes * 60;
		}

		public static uint minutes(this uint minutes)
		{
			return minutes * 60;
		}

		public static float frames(this int frames)
		{
			return (float)frames / 60f;
		}

		public static T cast<T>(this object obj)
		{
			return (T)obj;
		}

		public static void initialize_thread_static_list<T>(ref List<T> list, int capacity = 0)
		{
			if (list == null)
			{
				list = new List<T>(capacity);
			}
			list.Clear();
		}

		public static void initialize_thread_static_list<T>(ref ListOfStruct<T> list, int capacity = 0) where T : struct
		{
			if (list == null)
			{
				list = new ListOfStruct<T>(capacity);
			}
			list.clear();
		}

		public static void initialize_thread_static_array<T>(ref T[] array, int length)
		{
			if (array == null)
			{
				array = new T[length];
			}
			else
			{
				array.clear();
			}
		}

		[Conditional("DEBUG")]
		public static void assert(bool mustBeTrue, string message = null)
		{
			if (!mustBeTrue)
			{
				throw new InvalidOperationException("Assert failed: " + message);
			}
		}

		[Conditional("DEBUG")]
		public static void assert_null(object mustBeNotNull, string message = null)
		{
			if (mustBeNotNull == null)
			{
				throw new InvalidOperationException("Assert failed: " + message);
			}
		}

		public static void log_add_listener(Action<string, string, LogImportanceEnum> listenerDelegate)
		{
			if (logListenerDelegate == null)
			{
				logListenerDelegate = listenerDelegate;
				return;
			}
			lock (logListenerDelegate)
			{
				logListenerDelegate = (Action<string, string, LogImportanceEnum>)Delegate.Combine(logListenerDelegate, listenerDelegate);
			}
		}

		public static void log_write_line(string message, LogImportanceEnum importance = LogImportanceEnum.info)
		{
			log_write_line("", message, importance);
		}

		public static void log_write_line(string category, string message, LogImportanceEnum importance = LogImportanceEnum.info)
		{
			if (logListenerDelegate != null)
			{
				logListenerDelegate(category, message, importance);
				lock (logLineList)
				{
					logLineList.Add("[" + category + "] " + message);
				}
				return;
			}
			switch (importance)
			{
			case LogImportanceEnum.fatal:
				Console.ForegroundColor = ConsoleColor.Magenta;
				break;
			case LogImportanceEnum.error:
				Console.ForegroundColor = ConsoleColor.Red;
				break;
			case LogImportanceEnum.warning:
				Console.ForegroundColor = ConsoleColor.Yellow;
				break;
			}
			string text = "[" + category + "] " + message;
			Console.WriteLine(text);
			Console.ResetColor();
			lock (logLineList)
			{
				logLineList.Add(text);
			}
		}

		public static void wait_until(Func<bool> condition)
		{
			if (!condition())
			{
				while (!condition())
				{
					Thread.Sleep(1);
				}
			}
		}

		public static TValue get_or_create<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			value = new TValue();
			dictionary.Add(key, value);
			return value;
		}

		public static TValue get_or_create_thread_safe<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			value = new TValue();
			lock (dictionary)
			{
				dictionary.Add(key, value);
				return value;
			}
		}

		public static TValue get_or_default<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			return default(TValue);
		}

		public static bool compare(Fix a, Fix b, ComparisonEnum comparison)
		{
			return comparison switch
			{
				ComparisonEnum.none => false, 
				ComparisonEnum.greater => a > b, 
				ComparisonEnum.lesser => a < b, 
				ComparisonEnum.greaterOrEqual => a >= b, 
				ComparisonEnum.lesserOrEqual => a <= b, 
				ComparisonEnum.equal => a == b, 
				_ => throw new Exception(), 
			};
		}

		public static bool compare(int a, int b, ComparisonEnum comparison)
		{
			return comparison switch
			{
				ComparisonEnum.none => false, 
				ComparisonEnum.greater => a > b, 
				ComparisonEnum.lesser => a < b, 
				ComparisonEnum.greaterOrEqual => a >= b, 
				ComparisonEnum.lesserOrEqual => a <= b, 
				ComparisonEnum.equal => a == b, 
				_ => throw new Exception(), 
			};
		}

		public static string get_build_svn_revision()
		{
			if (_svnRevision == null)
			{
				_svnRevision = "";
				Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonLib.rev.txt");
				if (manifestResourceStream == null)
				{
					Console.WriteLine("Could not find CommonLib.rev.txt in assembly ressources.");
				}
				else
				{
					using StreamReader streamReader = new StreamReader(manifestResourceStream);
					_svnRevision = streamReader.ReadToEnd();
				}
			}
			return _svnRevision;
		}

		public static string get_version_identifier(string separator = " ", string overridePlatform = null)
		{
			string text = "Dev";
			if (branchName.starts_with("release"))
			{
				text = branchName.Substring(7);
			}
			text = ((overridePlatform == null) ? (text + "-s") : (text + overridePlatform));
			return "v" + text + separator + "r" + get_build_svn_revision();
		}

		public static bool type_equals(object a, object b)
		{
			if (a == null)
			{
				return b == null;
			}
			if (b == null)
			{
				return false;
			}
			return a.GetType() == b.GetType();
		}

		public static bool is_equal_to<T>(T a, T b) where T : IOptimizedIsEqualTo<T>
		{
			if (a == null)
			{
				return b == null;
			}
			if (b == null)
			{
				return false;
			}
			return a.is_equal_to(b);
		}

		public static void ensure_capacity<T>(this List<T> list, int min)
		{
			if (list.Capacity < min)
			{
				list.Capacity = DynamicArray_s<T>.calculate_new_ensure_capacity(list.Capacity, min);
			}
		}

		public static void to_array<TSource, TDest>(this List<TSource> list, out TDest[] array) where TDest : TSource
		{
			if (list == null)
			{
				array = new TDest[0];
				return;
			}
			array = new TDest[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = (TDest)(object)list[i];
			}
		}

		public static void to_array<TSource, TDest>(this ICollection<TSource> coll, out TDest[] array) where TDest : TSource
		{
			if (coll == null)
			{
				array = new TDest[0];
				return;
			}
			array = new TDest[coll.Count];
			int num = 0;
			foreach (TSource item in coll)
			{
				array[num] = (TDest)(object)item;
				num++;
			}
		}

		public static bool is_null_or_empty<T>(this List<T> list)
		{
			if (list != null)
			{
				return list.Count == 0;
			}
			return true;
		}

		public static bool is_null_or_empty<T>(this T[] array)
		{
			if (array != null)
			{
				return array.Length == 0;
			}
			return true;
		}

		public static int get_count<T>(this T[] array)
		{
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}

		public static T get_last<T>(this List<T> list)
		{
			return list[list.Count - 1];
		}

		public static T get_from_end<T>(this List<T> list, int idFromEnd)
		{
			return list[list.Count - idFromEnd];
		}

		public static T get_last<T>(this T[] array)
		{
			return array[array.Length - 1];
		}

		public static T get_from_end<T>(this T[] array, int idFromEnd)
		{
			return array[array.Length - idFromEnd];
		}

		public static T try_get_last<T>(this List<T> list)
		{
			if (list.is_null_or_empty())
			{
				return default(T);
			}
			return list[list.Count - 1];
		}

		public static T try_get_last<T>(this T[] array)
		{
			if (array.is_null_or_empty())
			{
				return default(T);
			}
			return array[array.Length - 1];
		}

		public static void remove_last<T>(this List<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		public static void try_remove_last<T>(this List<T> list)
		{
			if (!list.is_null_or_empty())
			{
				list.RemoveAt(list.Count - 1);
			}
		}

		public static T try_get<T>(this List<T> list, int index)
		{
			if (list.is_null_or_empty())
			{
				return default(T);
			}
			if (index < 0)
			{
				index = 0;
			}
			else if (index >= list.Count)
			{
				index = list.Count - 1;
			}
			return list[index];
		}

		public static T try_get<T>(this T[] array, int index)
		{
			if (array.is_null_or_empty())
			{
				return default(T);
			}
			if (index < 0)
			{
				index = 0;
			}
			else if (index >= array.Length)
			{
				index = array.Length - 1;
			}
			return array[index];
		}

		public static T try_get_or_default<T>(this T[] array, int index)
		{
			if (array.is_null_or_empty())
			{
				return default(T);
			}
			if (index < 0)
			{
				return default(T);
			}
			if (index >= array.Length)
			{
				return default(T);
			}
			return array[index];
		}

		public static void set_last<T>(this T[] array, T value)
		{
			array[array.Length - 1] = value;
		}

		public static T[] create_clone<T>(this T[] array)
		{
			T[] array2 = new T[array.Length];
			Array.Copy(array, array2, array.Length);
			return array2;
		}

		public static int index_of<T>(this T[] array, T value)
		{
			return Array.IndexOf(array, value);
		}

		public static int index_of<T>(this ICollection<T> collection, T value)
		{
			int num = 0;
			foreach (T item in collection)
			{
				if (item.Equals(value))
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public static bool contains<T>(this T[] array, T value)
		{
			if (array.is_null_or_empty())
			{
				return false;
			}
			return Array.IndexOf(array, value) != -1;
		}

		public static bool contains<T>(this AssetRef_s<T>[] array, string assetPath) where T : class
		{
			if (array.is_null_or_empty())
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Path == assetPath)
				{
					return true;
				}
			}
			return false;
		}

		public static void add_unique<T>(this List<T> list, T value)
		{
			if (!list.Contains(value))
			{
				list.Add(value);
			}
		}

		public static T find_element_of_type<T>(this List<T> list, Type type)
		{
			if (list.is_null_or_empty())
			{
				return default(T);
			}
			foreach (T item in list)
			{
				if (item.GetType() == type)
				{
					return item;
				}
			}
			return default(T);
		}

		public static T find_element_of_type<T, TParent>(this List<TParent> list) where T : TParent
		{
			if (list.is_null_or_empty())
			{
				return default(T);
			}
			foreach (TParent item in list)
			{
				if (item is T)
				{
					return (T)(object)item;
				}
			}
			return default(T);
		}

		public static void find_element_of_type<T, TParent>(this TParent[] array, out T element) where T : TParent
		{
			if (array.is_null_or_empty())
			{
				element = default(T);
				return;
			}
			foreach (TParent val in array)
			{
				if (val is T)
				{
					element = (T)(object)val;
					return;
				}
			}
			element = default(T);
		}

		public static void stable_sort<T>(this List<T> list, Comparison<T> comparison)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (comparison == null)
			{
				throw new ArgumentNullException("comparison");
			}
			int count = list.Count;
			for (int i = 1; i < count; i++)
			{
				T val = list[i];
				int num = i - 1;
				while (num >= 0 && comparison(list[num], val) > 0)
				{
					list[num + 1] = list[num];
					num--;
				}
				list[num + 1] = val;
			}
		}

		public static void clamp<T>(this List<T> list, int max)
		{
			if (max < list.Count)
			{
				list.RemoveRange(max, list.Count - max);
			}
		}

		public static void add_range<T>(this List<T> list, List<T> otherList)
		{
			list.ensure_capacity(list.Count + otherList.Count);
			for (int i = 0; i < otherList.Count; i++)
			{
				list.Add(otherList[i]);
			}
		}

		public static bool is_equal_to<T>(this T[] a, T[] otherA) where T : IOptimizedIsEqualTo<T>
		{
			if (!a.length_equals(otherA))
			{
				return true;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (!a[i].is_equal_to(otherA[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static bool length_equals<T>(this T[] a, T[] otherA)
		{
			if (a.is_null_or_empty())
			{
				return otherA.is_null_or_empty();
			}
			if (otherA.is_null_or_empty())
			{
				return false;
			}
			if (a.Length != otherA.Length)
			{
				return false;
			}
			return true;
		}

		public static int get_file_number(string fn)
		{
			fn = Path.GetFileNameWithoutExtension(fn);
			int num = fn.Length;
			while (--num > 0 && fn[num] >= '0' && fn[num] <= '9')
			{
			}
			if (int.TryParse(fn.Substring(num + 1), out var result))
			{
				return result;
			}
			return -1;
		}

		public static int[] file_array_to_image_id_array(string[] fileNames)
		{
			if (fileNames.is_null_or_empty())
			{
				return new int[0];
			}
			int[] array = new int[fileNames.Length];
			for (int i = 0; i < fileNames.Length; i++)
			{
				array[i] = get_file_number(fileNames[i]);
			}
			return array;
		}

		public static void copy_whole_directory(string sourcePath, string destinationPath, string[] excludedExtensionArray = null)
		{
			sourcePath = Path.GetFullPath(sourcePath);
			destinationPath = Path.GetFullPath(destinationPath);
			Directory.CreateDirectory(destinationPath);
			string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
			foreach (string text in directories)
			{
				Directory.CreateDirectory(text.Replace(sourcePath, destinationPath));
			}
			string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
			foreach (string text2 in files)
			{
				bool flag = false;
				if (excludedExtensionArray != null)
				{
					foreach (string value in excludedExtensionArray)
					{
						if (text2.EndsWith(value))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					File.Copy(text2, text2.Replace(sourcePath, destinationPath), overwrite: true);
				}
			}
		}

		public static void delete_whole_directory(string dirPath)
		{
			if (!Directory.Exists(dirPath))
			{
				return;
			}
			bool flag = false;
			try
			{
				Directory.Delete(dirPath, recursive: true);
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				Thread.Sleep(100);
				if (Directory.Exists(dirPath))
				{
					Directory.Delete(dirPath, recursive: true);
				}
			}
		}

		public static string get_file_path_with_current_date(string basePath, string extension)
		{
			try
			{
				basePath = platform.get_save_file_path(basePath);
				string text = basePath + "_" + string.Format("{0:yyyy_MMMdd}_{0:HH}h{0:mm}m{0:ss}s", DateTime.Now) + extension;
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				return text;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed get_file_path_with_current_date(" + basePath + ", " + extension + ").\n" + ex.Message);
			}
		}

		public static void file_copy(string sourceFileName, string destFileName, bool overwrite = true)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
			File.Copy(sourceFileName, destFileName, overwrite);
		}

		public static void delete_file(string path, int attempts)
		{
			string text = "";
			for (int i = 0; i < attempts; i++)
			{
				try
				{
					if (File.Exists(path))
					{
						text += " try deleting ";
						File.Delete(path);
					}
				}
				catch (Exception ex)
				{
					log_write_line("Could not delete file " + path + ". " + ex.Message, LogImportanceEnum.error);
					text = text + ex.Message + "\n";
					Thread.Sleep(1000);
					platform.full_garbage_collection();
				}
			}
			throw new Exception("Could not delete file " + path + " after " + attempts + " attempts.\n" + text);
		}

		public static FileStream create_file(string path, int attempts)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			string text = "";
			for (int i = 0; i < attempts; i++)
			{
				try
				{
					if (File.Exists(path))
					{
						text += " try deleting ";
						File.Delete(path);
						text += " deleted, try creating ";
					}
					return File.Create(path);
				}
				catch (Exception ex)
				{
					log_write_line("Could not create file " + path + ". " + ex.Message, LogImportanceEnum.error);
					text = text + ex.Message + "\n";
					Thread.Sleep(1000);
					platform.full_garbage_collection();
				}
			}
			throw new Exception("Could not create file " + path + " after " + attempts + " attempts.\n" + text);
		}

		public static ListIterator_s<T> iterator<T>(this List<T> list)
		{
			if (list == null)
			{
				return ListIterator_s<T>.empty;
			}
			return new ListIterator_s<T>(list);
		}

		public static ArrayIterator_s<T> iterator<T>(this T[] a)
		{
			if (a == null)
			{
				return ArrayIterator_s<T>.empty;
			}
			return new ArrayIterator_s<T>(a);
		}

		public static string to_string(this int obj)
		{
			return obj.ToString(CultureInfo.InvariantCulture);
		}

		public static string to_string(this float obj)
		{
			return obj.ToString(CultureInfo.InvariantCulture);
		}

		public static string to_string(this double obj)
		{
			return obj.ToString(CultureInfo.InvariantCulture);
		}

		public static string to_string(this byte obj)
		{
			return obj.ToString(CultureInfo.InvariantCulture);
		}

		public static string to_string(this Fix obj)
		{
			return obj.ToString();
		}

		public static string to_string(this Vector3[] array)
		{
			string text = "[";
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector = array[i];
				text += $"({vector.X},{vector.Y},{vector.Z})";
			}
			return text + "]";
		}

		public static string to_string(this Vector4[] array)
		{
			string text = "[";
			for (int i = 0; i < array.Length; i++)
			{
				Vector4 vector = array[i];
				text += $"({vector.X},{vector.Y},{vector.Z},{vector.W})";
			}
			return text + "]";
		}

		public static string to_string(this Color_s[] array)
		{
			string text = "[";
			foreach (Color_s color_s in array)
			{
				text = text + ((uint)color_s).ToString("x") + ",";
			}
			return text + "]";
		}

		public static string to_string<T>(this T[] array)
		{
			string text = "[";
			for (int i = 0; i < array.Length; i++)
			{
				T val = array[i];
				text = text + val.ToString() + ",";
			}
			return text + "]";
		}

		public static bool is_null_or_empty(this string str)
		{
			if (str == null)
			{
				return true;
			}
			return str == "";
		}

		public static bool is_not_null_or_empty(this string str)
		{
			return !str.is_null_or_empty();
		}

		public static string split_and_get(this string str, char separator, int index)
		{
			string[] array = str.Split(separator);
			if (array.Length == 1)
			{
				return array[0];
			}
			return array[(array.Length + index) % array.Length];
		}

		public static string split_and_get_last(this string str, char separator)
		{
			return str.split_and_get(separator, -1);
		}

		public static string split_and_remove_last(this string str, char separator)
		{
			string[] array = str.Split(separator);
			if (array.Length == 1)
			{
				return "";
			}
			return str.Substring(0, str.Length - array[array.Length - 1].Length);
		}

		public static string to_upper(this string s)
		{
			StringBuilder stringBuilder = new StringBuilder(s);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				if (stringBuilder[i] >= 'a' && stringBuilder[i] <= 'z')
				{
					stringBuilder[i] = (char)(stringBuilder[i] + 65 - 97);
				}
			}
			return stringBuilder.ToString();
		}

		public static string to_lower(this string s)
		{
			StringBuilder stringBuilder = new StringBuilder(s);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				if (stringBuilder[i] >= 'A' && stringBuilder[i] <= 'Z')
				{
					stringBuilder[i] = (char)(stringBuilder[i] + 97 - 65);
				}
			}
			return stringBuilder.ToString();
		}

		public static string to_title_case(this string s)
		{
			return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.to_lower());
		}

		public static string to_sentence_case(this string s)
		{
			char[] array = s.to_lower().ToCharArray();
			array[0] = array[0].ToString().to_upper()[0];
			return new string(array);
		}

		public static string remove_extension(this string str)
		{
			string extension = Path.GetExtension(str);
			return str.Remove(str.Length - extension.Length);
		}

		public static bool starts_with(this string str, string text)
		{
			if (text == null || str == null || str.Length < text.Length)
			{
				return false;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (str[i] != text[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string get_first_line(this string text)
		{
			int num = text.IndexOf('\n');
			if (num >= 0)
			{
				return text.Substring(0, num);
			}
			return text;
		}

		public static string get_from_line(this string text, int lineId)
		{
			int num = 0;
			for (int i = 0; i < lineId; i++)
			{
				num = text.IndexOf('\n', num + 1);
			}
			return text.Substring(num + 1);
		}

		public static string shorten_from_beginning(this string text, int maxSize)
		{
			if (text.Length <= maxSize)
			{
				return text;
			}
			return text.Remove(0, text.Length - maxSize);
		}

		public static string set_char(this string str, int index, char value)
		{
			char[] array = str.ToCharArray();
			array[index] = value;
			return new string(array);
		}

		public static bool contains(this string s, char c)
		{
			return s.IndexOf(c) >= 0;
		}

		public static string pack_to_string(float a, float b, float c, float d)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(c.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(d.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out float a, out float b, out float c, out float d)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = float.Parse(array[0], CultureInfo.InvariantCulture);
			b = float.Parse(array[1], CultureInfo.InvariantCulture);
			c = float.Parse(array[2], CultureInfo.InvariantCulture);
			d = float.Parse(array[3], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(float a, float b, float c)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(c.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out float a, out float b, out float c)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = float.Parse(array[0], CultureInfo.InvariantCulture);
			b = float.Parse(array[1], CultureInfo.InvariantCulture);
			c = float.Parse(array[2], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(float a, float b)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out float a, out float b)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = float.Parse(array[0], CultureInfo.InvariantCulture);
			if (array.Length < 2)
			{
				b = a;
			}
			else
			{
				b = float.Parse(array[1], CultureInfo.InvariantCulture);
			}
		}

		public static string pack_to_string(byte a, byte b, byte c, byte d)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(c.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(d.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out byte a, out byte b, out byte c, out byte d)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = byte.Parse(array[0], CultureInfo.InvariantCulture);
			b = byte.Parse(array[1], CultureInfo.InvariantCulture);
			c = byte.Parse(array[2], CultureInfo.InvariantCulture);
			d = byte.Parse(array[3], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(int a, int b)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out int a, out int b)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = int.Parse(array[0], CultureInfo.InvariantCulture);
			b = int.Parse(array[1], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(int a, int b, int c)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(c.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out int a, out int b, out int c)
		{
			string[] array = src.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			a = int.Parse(array[0], CultureInfo.InvariantCulture);
			b = int.Parse(array[1], CultureInfo.InvariantCulture);
			c = int.Parse(array[2], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(int a, int b, int c, int d)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(b.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(c.to_string());
			stringBuilder.Append(" ");
			stringBuilder.Append(d.to_string());
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out int a, out int b, out int c, out int d)
		{
			string[] array = src.Split(',', ' ');
			a = int.Parse(array[0], CultureInfo.InvariantCulture);
			b = int.Parse(array[1], CultureInfo.InvariantCulture);
			c = int.Parse(array[2], CultureInfo.InvariantCulture);
			d = int.Parse(array[3], CultureInfo.InvariantCulture);
		}

		public static string pack_to_string(string a, string b)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Clear();
			stringBuilder.Append(a);
			if (b != null)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(b);
			}
			return stringBuilder.ToString();
		}

		public static void unpack_from_string(string src, out string a, out string b)
		{
			string[] array = src.Split(',');
			a = array[0];
			if (array.Length > 1)
			{
				b = array[1];
			}
			else
			{
				b = null;
			}
		}

		public static void set_as_main_thread()
		{
			isMainThread = true;
			Thread.CurrentThread.Name = "main";
		}

		public static bool is_main_thread()
		{
			return isMainThread;
		}

		public static Thread start_worker_thread(ThreadStart start, string name)
		{
			ThreadStart start2 = delegate
			{
				try
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
					start();
				}
				catch (Exception obj)
				{
					showErrorReportDelegate(obj);
					Environment.Exit(0);
				}
			};
			Thread thread = new Thread(start2);
			if (name != null)
			{
				thread.Name = name;
			}
			thread.Start();
			lock (workerThreadList)
			{
				workerThreadList.Add(thread);
				return thread;
			}
		}

		public static void remove_worker_thread(Thread thread)
		{
			if (thread != null)
			{
				lock (workerThreadList)
				{
					workerThreadList.Remove(thread);
				}
			}
		}

		public static void shutdown_worker_threads(Action action = null)
		{
			workerThreadsShouldTerminate = true;
			while (true)
			{
				Thread[] array;
				lock (workerThreadList)
				{
					array = workerThreadList.ToArray();
				}
				string text = "BLOCKED - Terminating Worker Threads";
				bool flag = false;
				Thread[] array2 = array;
				foreach (Thread thread in array2)
				{
					if (thread != null && thread.IsAlive)
					{
						flag = true;
						text = text + " - " + thread.Name;
					}
				}
				if (!flag)
				{
					break;
				}
				action?.Invoke();
			}
		}
	}
}
