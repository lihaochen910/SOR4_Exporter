using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CommonLib
{
	public static class asset_cache
	{
		public enum AssetLoadingStateEnum
		{
			none = 0,
			textureDiskToMemory = 1,
			textureWaitInMemory = 2,
			textureMemoryToGpu = 3,
			diskToMemory = 4,
			loaded = 5
		}

		public class AssetLoadingState
		{
			public AssetLoadingStateEnum state;

			public TextureStream textureStream;
		}

		public class TextureStream : MemoryStream
		{
			public TextureStream()
				: base(4194304)
			{
			}

			protected override void Dispose(bool disposing)
			{
			}
		}

		public struct LengthAndPos_s
		{
			public long position;

			public int length;
		}

		public class TextureBigFile
		{
			public Dictionary<string, LengthAndPos_s> textureLengthAndPosByAssetPath = new Dictionary<string, LengthAndPos_s>();

			public FileStream textureBigFileStream;
		}

		private struct TextureToDecompress_s
		{
			public string assetPath;

			public byte[] data;
		}

		private struct TextureToPushOnGpu_s
		{
			public string assetPath;

			public MemoryStream data;
		}

		public static ConcurrentDictionary<AssetId_s, object> assetById = new ConcurrentDictionary<AssetId_s, object>();

		private static MethodInfo genericGetMethodInfo = typeof(asset_cache).GetMethod("get");

		private static Dictionary<Type, MethodInfo> getMethodByType = new Dictionary<Type, MethodInfo>();

		private static ConcurrentDictionary<Type, object> defaultAssetByType = new ConcurrentDictionary<Type, object>();

		private static volatile int totalTextureMemory;

		private static Dictionary<string, string> fontFullPathByRelativePath = new Dictionary<string, string>();

		public static volatile uint assetByIdVersion = 1u;

		private const int textureStreamCapacity = 4194304;

		public static List<AssetId_s> priorityGraphicsAssetLoadingList = new List<AssetId_s>();

		public static List<string> texturePathLoadingList = new List<string>();

		private static volatile int textureLoadingUsedMemory;

		private const int textureLoadingMemoryBudget = 8388608;

		public static TextureBigFile[] textureBigFileArray;

		private static List<TextureToDecompress_s> textureToDecompressList = new List<TextureToDecompress_s>();

		private static volatile bool textureReadFinished;

		private static List<TextureToPushOnGpu_s> textureToPushOnGpuList = new List<TextureToPushOnGpu_s>();

		public static volatile Thread loadingThread;

		private static Dictionary<string, AssetId_s[]> dependencyListById = new Dictionary<string, AssetId_s[]>();

		private static Dictionary<string, AssetId_s[]> dependencyListByIdIgnoreNoDependencyAttribute = new Dictionary<string, AssetId_s[]>();

		private static ConcurrentDictionary<AssetId_s, AssetLoadingState> assetLoadingStateById = new ConcurrentDictionary<AssetId_s, AssetLoadingState>();

		public static volatile HashSet<AssetId_s> assetNotInUseSet = new HashSet<AssetId_s>();

		public static bool unloadAssetsNotInUse = true;

		private static volatile bool abordBackgroundLoading;

		public static bool isLoadingTextureOnMainThreadInUpdate;

		public static bool canLoadMoreThanOneTexturePerFrame;

		private static Stopwatch stopwatch = new Stopwatch();

		private static AssetRef_s<Texture2D> loadingTextureRef = new AssetRef_s<Texture2D>("LoadingTexture");

		private static HashSet<Type> xnaTypeHashSet = new HashSet<Type>
		{
			typeof(Texture2D),
			typeof(Texture3D),
			typeof(SpriteFont),
			typeof(Model),
			typeof(Effect),
			typeof(Song),
			typeof(SoundEffect)
		};

		public static int TotalTextureMemory => totalTextureMemory;

		private static bool IsOnLoadingThread => Thread.CurrentThread.Name == "asset loading thread";

		private static int TextureLoadingUsedMo => textureLoadingUsedMemory / 1048576;

		public static int IntermediateTextureStreamsMemory => 0;

		public static bool IsLoading
		{
			get
			{
				if (loadingThread != null)
				{
					return true;
				}
				lock (textureToPushOnGpuList)
				{
					if (textureToPushOnGpuList.Count > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static void init()
		{
			using (Stream input = platform.get_decompressor(File.OpenRead(Path.Combine(platform.get_data_folder(), "bigfile"))))
			{
				BinaryReader binaryReader = new BinaryReader(input, Encoding.Unicode);
				for (int num = binaryReader.ReadInt32(); num > 0; num--)
				{
					string name = binaryReader.ReadString();
					Type type = reflection.get_type_by_full_name_without_namespace(name);
					string assetPath = binaryReader.ReadString();
					int count = binaryReader.ReadInt32();
					byte[] byteArray = binaryReader.ReadBytes(count);
					object obj = reflection.bin_deserialize(byteArray, type);
					if (obj is IAssetEvents)
					{
						((IAssetEvents)obj).on_asset_modified(assetPath);
					}
					add_asset_object(assetPath, type, obj);
				}
			}
			AssetDependencyCacheData assetDependencyCacheData = get<AssetDependencyCacheData>("dependencies");
			assetDependencyCacheData.set_in_cache();
			assetDependencyCacheData = null;
			assetById.TryRemove(new AssetId_s("dependencies", typeof(AssetDependencyCacheData)), out var _);
			GC.Collect();
			textureBigFileArray = new TextureBigFile[2];
			textureBigFileArray[0] = open_texture_bigfile_and_table("texture_table", "textures");
			textureBigFileArray[1] = open_texture_bigfile_and_table("texture_table02", "textures02");
		}

		private static TextureBigFile open_texture_bigfile_and_table(string tableName, string bigFileName)
		{
			TextureBigFile textureBigFile = new TextureBigFile();
			using (BinaryReader binaryReader = new BinaryReader(new FileStream(Path.Combine(platform.get_data_folder(), tableName), FileMode.Open), Encoding.Unicode))
			{
				LengthAndPos_s value = default(LengthAndPos_s);
				while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
				{
					string key = binaryReader.ReadString();
					value.position = binaryReader.ReadInt64();
					value.length = binaryReader.ReadInt32();
					textureBigFile.textureLengthAndPosByAssetPath.Add(key, value);
				}
			}
			textureBigFile.textureBigFileStream = new FileStream(Path.Combine(platform.get_data_folder(), bigFileName), FileMode.Open);
			return textureBigFile;
		}

		private static void get_texture_bigfile_info(string assetPath, out FileStream bigfileStream, out LengthAndPos_s lp)
		{
			TextureBigFile[] array = textureBigFileArray;
			foreach (TextureBigFile textureBigFile in array)
			{
				if (textureBigFile.textureLengthAndPosByAssetPath.TryGetValue(assetPath, out lp))
				{
					bigfileStream = textureBigFile.textureBigFileStream;
					return;
				}
			}
			throw new Exception("Could not find texture " + assetPath + " in any textures bigfile.");
		}

		private static string get_binary_file_name(string assetPath, Type type)
		{
			if (type != typeof(Texture2D) && type != typeof(SpriteFont) && type != typeof(Effect))
			{
				throw new Exception();
			}
			return assetPath + ".xnb";
		}

		private static string find_binary_file_path(string assetPath, Type type, out string rootDirectory)
		{
			string path = get_binary_file_name(assetPath, type);
			rootDirectory = platform.get_data_folder();
			string text = Path.Combine(rootDirectory, path);
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}

		public static string get_font_full_path_from_relative_path(string relativePath)
		{
			if (!fontFullPathByRelativePath.TryGetValue(relativePath, out var value))
			{
				value = Path.GetFullPath(Path.Combine(platform.get_data_folder(), Path.GetFileName(relativePath)));
				fontFullPathByRelativePath[relativePath] = value;
			}
			return value;
		}

		public static bool is_compressed(Type type)
		{
			if (type == typeof(Effect) || type == typeof(Song))
			{
				return false;
			}
			return true;
		}

		private static Stream get_asset_stream(string assetPath, Type type, out string rootDirectory)
		{
			if (type == typeof(Texture2D))
			{
				get_texture_bigfile_info(assetPath, out var bigfileStream, out var lp);
				lock (bigfileStream)
				{
					byte[] buffer = new byte[lp.length];
					bigfileStream.Position = lp.position;
					bigfileStream.Read(buffer, 0, lp.length);
					rootDirectory = null;
					return platform.get_decompressor(new MemoryStream(buffer));
				}
			}
			string text = find_binary_file_path(assetPath, type, out rootDirectory);
			if (text == null)
			{
				throw new Exception("Asset Stream not found " + assetPath + " of type " + type.Name);
			}
			if (is_compressed(type))
			{
				return platform.get_decompressor(File.OpenRead(text));
			}
			return new FileStream(text, FileMode.Open, FileAccess.Read);
		}

		public static void shutdown()
		{
			utils.WorkerThreadsShouldTerminate = true;
			TextureBigFile[] array = textureBigFileArray;
			foreach (TextureBigFile textureBigFile in array)
			{
				textureBigFile.textureBigFileStream.Close();
			}
		}

		public static void clear_dependency_cache(string assetPath)
		{
		}

		public static bool is_protobuf_asset(Type type)
		{
			return !xnaTypeHashSet.Contains(type);
		}

		public static bool contains_asset(AssetId_s id)
		{
			return assetById.ContainsKey(id);
		}

		public static bool contains_asset(string assetPath, Type type)
		{
			return assetById.ContainsKey(new AssetId_s(assetPath, type));
		}

		public static void add_asset_object(AssetId_s id, object asset)
		{
			if (!assetById.TryAdd(id, asset))
			{
				new InvalidOperationException("Trying to add an asset that is already in cache: " + id.ToString());
			}
			assetByIdVersion++;
		}

		private static void add_asset_object(string assetPath, Type type, object asset)
		{
			add_asset_object(new AssetId_s(assetPath, type), asset);
		}

		private static bool try_get_asset_object(AssetId_s id, out object obj)
		{
			return assetById.TryGetValue(id, out obj);
		}

		private static bool try_get_asset_object(string assetPath, Type type, out object obj)
		{
			return try_get_asset_object(new AssetId_s(assetPath, type), out obj);
		}

		public static void remove_asset_object(AssetId_s id)
		{
			AssetLoadingState assetLoadingState = get_loading_state(id);
			lock (assetLoadingState)
			{
				assetLoadingState.state = AssetLoadingStateEnum.none;
				if (!assetById.TryRemove(id, out var _))
				{
					new InvalidOperationException("Trying to remove an asset that is not in cache: " + id.ToString());
				}
				assetByIdVersion++;
			}
		}

		public static void remove_asset_object(string assetPath, Type type)
		{
			remove_asset_object(new AssetId_s(assetPath, type));
		}

		private static void replace_asset_object(AssetId_s id, object newObj)
		{
			if (!try_get_asset_object(id, out var obj))
			{
				new InvalidOperationException("Trying to replace an asset that is not in cache: " + id.ToString());
			}
			if (!assetById.TryUpdate(id, newObj, obj))
			{
				new Exception("Should not get there in replace_asset_object: " + id.ToString());
			}
			assetByIdVersion++;
		}

		private static void replace_asset_object(string assetPath, Type type, object newObj)
		{
			replace_asset_object(new AssetId_s(assetPath, type), newObj);
		}

		private static void load_asset(AssetId_s assetId, Stream customStream = null)
		{
			if (assetId.type != typeof(Wwise.SoundBankDummyAsset) && assetId.type != typeof(Texture2D) && assetId.type != typeof(SpriteFont) && assetId.type != typeof(Effect))
			{
				if (assetId.assetPath.is_null_or_empty())
				{
					throw new Exception("Trying to access null asset of type " + assetId.ToString());
				}
				throw new Exception("Should not try to load " + assetId.ToString());
			}
			if (assetId.type == typeof(Wwise.SoundBankDummyAsset))
			{
				// Wwise.load_bank(assetId.assetPath);
				return;
			}
			AssetLoadingState assetLoadingState = get_loading_state(assetId);
			bool flag = false;
			bool lockTaken = false;
			Monitor.Enter(assetLoadingState, ref lockTaken);
			if (contains_asset(assetId))
			{
				return;
			}
			if (utils.is_main_thread() && isLoadingTextureOnMainThreadInUpdate)
			{
				if (lockTaken)
				{
					Monitor.Exit(assetLoadingState);
				}
			}
			else
			{
				if (assetLoadingState.state != 0)
				{
					flag = true;
				}
				else
				{
					assetLoadingState.state = AssetLoadingStateEnum.diskToMemory;
				}
				if (lockTaken)
				{
					Monitor.Exit(assetLoadingState);
				}
				if (flag)
				{
					while (true)
					{
						lock (assetLoadingState)
						{
							if (assetLoadingState.state == AssetLoadingStateEnum.loaded)
							{
								return;
							}
						}
						if (utils.is_main_thread())
						{
							update_on_main_thread();
						}
						Thread.Sleep(10);
						Thread.Yield();
					}
				}
			}
			object obj = null;
			if (xnaTypeHashSet.Contains(assetId.type))
			{
				obj = xna_load_asset(assetId, customStream);
			}
			else
			{
				try
				{
					obj = reflection.bin_deserialize(get_asset_stream(assetId.assetPath, assetId.type, out var _), null, assetId.type);
				}
				catch (Exception ex)
				{
					throw new Exception("Error while deserializing asset " + assetId.ToString() + ": \n" + ex.Message);
				}
				if (obj is IAssetEvents)
				{
					((IAssetEvents)obj).on_asset_modified(assetId.assetPath);
				}
			}
			lock (assetLoadingState)
			{
				add_asset_object(assetId, obj);
				assetLoadingState.state = AssetLoadingStateEnum.loaded;
			}
		}

		public static string get_binary_file_path_from_asset_path(string binAssetFolder, string assetPath, Type type)
		{
			return Path.Combine(binAssetFolder, get_binary_file_name(assetPath, type));
		}

		public static bool bin_asset_exists(string assetPath, Type type)
		{
			if (assetPath.is_null_or_empty())
			{
				return false;
			}
			string rootDirectory;
			return find_binary_file_path(assetPath, type, out rootDirectory) != null;
		}

		private static bool asset_exists(string assetPath, Type type)
		{
			if (bin_asset_exists(assetPath, type))
			{
				return true;
			}
			return false;
		}

		[Conditional("EDITOR")]
		public static void check_path(ref string assetPath)
		{
			if (assetPath != null)
			{
				if (assetPath == "\n\0")
				{
					assetPath = "";
				}
				else
				{
					assetPath = assetPath.Replace('\\', '/');
				}
			}
		}

		private static object get_default_factory<T>(Type t) where T : class
		{
			string assetPath = "Default/" + t.Name;
			if (asset_exists(assetPath, t))
			{
				return get<T>(assetPath);
			}
			object obj = Activator.CreateInstance(reflection.get_instantiable_derived_class_array(t)[0].type);
			if (obj is IAssetEvents)
			{
				((IAssetEvents)obj).on_asset_modified(assetPath);
			}
			return obj;
		}

		public static T get_default<T>() where T : class
		{
			return (T)defaultAssetByType.GetOrAdd(typeof(T), get_default_factory<T>);
		}

		private static T try_get_asset_in_cache<T>(string assetPath) where T : class
		{
			if (assetPath.is_null_or_empty())
			{
				return null;
			}
			if (try_get_asset_object(assetPath, typeof(T), out var obj))
			{
				if (xnaTypeHashSet.Contains(typeof(T)))
				{
					return (T)xna_get_asset_from_stored_asset<T>(obj);
				}
				return (T)obj;
			}
			return null;
		}

		public static T get<T>(string assetPath) where T : class
		{
			T val = null;
			val = try_get_asset_in_cache<T>(assetPath);
			if (val != null)
			{
				return val;
			}
			AssetId_s assetId = new AssetId_s(assetPath, typeof(T));
			load_asset(assetId);
			return try_get_asset_in_cache<T>(assetPath);
		}

		public static void unload(string assetPath, Type type)
		{
			if (type == typeof(Wwise.SoundBankDummyAsset))
			{
				Wwise.unloadbank(assetPath);
				return;
			}
			if (try_get_asset_object(assetPath, type, out var obj))
			{
				if (obj is IDisposable)
				{
					if (obj is Texture2D)
					{
						Texture2D texture2D = (Texture2D)obj;
						totalTextureMemory -= texture2D.Width * texture2D.Height * 4;
					}
					IDisposable disposable = (IDisposable)obj;
					disposable.Dispose();
				}
				remove_asset_object(assetPath, type);
			}
			clear_dependency_cache(assetPath);
		}

		[Obsolete("Does not work for now.")]
		public static string[] get_all_asset_path_array(Type type)
		{
			List<string> list = new List<string>();
			return list.ToArray();
		}

		public static object get_with_reflection(Type type, string assetPath)
		{
			if (getMethodByType.TryGetValue(type, out var value))
			{
				return value.Invoke(null, new object[1]
				{
					assetPath
				});
			}
			value = genericGetMethodInfo.MakeGenericMethod(type);
			lock (getMethodByType)
			{
				getMethodByType[type] = value;
			}
			return value.Invoke(null, new object[1]
			{
				assetPath
			});
		}

		public static string convert_asset_path_to_standalone(string assetPath)
		{
			assetPath = assetPath.to_lower();
			assetPath = assetPath.Replace('\ufffd', 'e');
			assetPath = assetPath.Replace('\ufffd', 'e');
			assetPath = assetPath.Replace('\ufffd', 'e');
			assetPath = assetPath.Replace('\ufffd', 'a');
			assetPath = assetPath.Replace('\ufffd', 'c');
			assetPath = assetPath.Replace(' ', '_');
			return assetPath;
		}

		public static void convert_all_asset_refs_to_standalone_path(object obj)
		{
			if (obj == null)
			{
				return;
			}
			Type type = obj.GetType();
			if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
			{
				return;
			}
			if (obj is IList)
			{
				IList list = (IList)obj;
				for (int i = 0; i < list.Count; i++)
				{
					object obj2 = list[i];
					convert_all_asset_refs_to_standalone_path(obj2);
					list[i] = obj2;
				}
				return;
			}
			if (type.can_be_cast_to(typeof(IAssetRef)))
			{
				IAssetRef assetRef = (IAssetRef)obj;
				assetRef.Path = convert_asset_path_to_standalone(assetRef.Path);
				return;
			}
			reflection.Member_s[] array = reflection.get_member_array(obj);
			for (int j = 0; j < array.Length; j++)
			{
				reflection.Member_s member_s = array[j];
				object obj3 = member_s.get_value(obj);
				convert_all_asset_refs_to_standalone_path(obj3);
				member_s.set_value(obj, obj3);
			}
		}

		private static void yield_if_not_enough_texture_loading_memory_and_add(int requiredMemory)
		{
			int num = 0;
			while (textureLoadingUsedMemory + requiredMemory > 8388608)
			{
				Thread.Sleep(10);
				Thread.Yield();
				if (++num >= 500)
				{
					break;
				}
			}
			textureLoadingUsedMemory += requiredMemory;
			if (num >= 500)
			{
				Console.WriteLine($"TEXTURE BUDGET IS LOCKED UP: {textureLoadingUsedMemory}");
			}
		}

		private static AssetLoadingState asset_loading_state_factory(AssetId_s id)
		{
			return new AssetLoadingState();
		}

		public static AssetLoadingState get_loading_state(AssetId_s id)
		{
			return assetLoadingStateById.GetOrAdd(id, asset_loading_state_factory);
		}

		public static AssetId_s[] get_dependency_array(string assetPath, Type type)
		{
			if (xnaTypeHashSet.Contains(type))
			{
				return null;
			}
			if (dependencyListById.TryGetValue(assetPath, out var value))
			{
				return value;
			}
			return null;
		}

		public static void set_dependency_array(string assetPath, Type type, AssetId_s[] array)
		{
			dependencyListById[assetPath] = array;
		}

		public static AssetId_s[] get_dependency_array_ignore_no_dependency_attribute(string assetPath, Type type)
		{
			if (xnaTypeHashSet.Contains(type))
			{
				return null;
			}
			if (dependencyListByIdIgnoreNoDependencyAttribute.TryGetValue(assetPath, out var value))
			{
				return value;
			}
			return null;
		}

		public static void set_dependency_array_ignore_no_dependency_attribute(string assetPath, Type type, AssetId_s[] array)
		{
			dependencyListByIdIgnoreNoDependencyAttribute[assetPath] = array;
		}

		public static void rec_load_dependencies(HashSet<AssetId_s> addedDependencies, string assetPath, Type type, bool allProtobufMode, List<string> missingAssetList = null)
		{
			if (!utils.is_main_thread() && (abordBackgroundLoading || utils.WorkerThreadsShouldTerminate))
			{
				return;
			}
			AssetId_s assetId_s = new AssetId_s(assetPath, type);
			if (addedDependencies.Contains(assetId_s))
			{
				return;
			}
			addedDependencies.Add(assetId_s);
			if (type == typeof(Wwise.SoundBankDummyAsset))
			{
				if (!allProtobufMode)
				{
					load_asset(assetId_s);
				}
				return;
			}
			if (xnaTypeHashSet.Contains(type))
			{
				if (allProtobufMode || contains_asset(assetId_s))
				{
					return;
				}
				if (utils.is_main_thread())
				{
					load_asset(assetId_s);
					return;
				}
				if (type == typeof(Effect) || type == typeof(SpriteFont))
				{
					lock (priorityGraphicsAssetLoadingList)
					{
						if (!priorityGraphicsAssetLoadingList.Contains(assetId_s))
						{
							priorityGraphicsAssetLoadingList.Add(assetId_s);
						}
					}
					return;
				}
				if (type == typeof(Texture2D))
				{
					lock (texturePathLoadingList)
					{
						if (!texturePathLoadingList.Contains(assetPath))
						{
							texturePathLoadingList.Add(assetPath);
						}
					}
					return;
				}
				throw new Exception();
			}
			AssetId_s[] array = ((!allProtobufMode) ? get_dependency_array(assetPath, type) : get_dependency_array_ignore_no_dependency_attribute(assetPath, type));
			if (array != null)
			{
				AssetId_s[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					AssetId_s assetId_s2 = array2[i];
					rec_load_dependencies(addedDependencies, assetId_s2.assetPath, assetId_s2.type, allProtobufMode, missingAssetList);
				}
			}
		}

		public static void unload_assets_not_in_use()
		{
			if (assetNotInUseSet.Count == 0)
			{
				return;
			}
			foreach (AssetId_s item in assetNotInUseSet)
			{
				unload(item.assetPath, item.type);
			}
			assetNotInUseSet.Clear();
			platform.full_garbage_collection();
			assetByIdVersion++;
		}

		private static void get_bigfile_info_for_compare(string assetPath, out int bigfileId, out long pos)
		{
			for (int i = 0; i < textureBigFileArray.Length; i++)
			{
				TextureBigFile textureBigFile = textureBigFileArray[i];
				if (textureBigFile.textureLengthAndPosByAssetPath.TryGetValue(assetPath, out var value))
				{
					bigfileId = i;
					pos = value.position;
					return;
				}
			}
			throw new Exception("Could not find texture " + assetPath + " in any textures bigfile.");
		}

		private static int texture_path_compare(string a, string b)
		{
			get_bigfile_info_for_compare(a, out var bigfileId, out var pos);
			get_bigfile_info_for_compare(b, out var bigfileId2, out var pos2);
			if (bigfileId != bigfileId2)
			{
				return bigfileId.CompareTo(bigfileId2);
			}
			return pos.CompareTo(pos2);
		}

		private static void decompress_thread_method()
		{
			while (true)
			{
				TextureToDecompress_s textureToDecompress_s = default(TextureToDecompress_s);
				lock (textureToDecompressList)
				{
					if (textureToDecompressList.Count > 0)
					{
						textureToDecompress_s = textureToDecompressList[0];
						textureToDecompressList.RemoveAt(0);
					}
					else if (textureReadFinished)
					{
						return;
					}
				}
				if (textureToDecompress_s.assetPath == null)
				{
					Thread.Sleep(10);
					Thread.Yield();
					continue;
				}
				AssetLoadingState assetLoadingState = get_loading_state(new AssetId_s(textureToDecompress_s.assetPath, typeof(Texture2D)));
				MemoryStream memoryStream = new MemoryStream();
				Stream stream = platform.get_decompressor(new MemoryStream(textureToDecompress_s.data));
				stream.CopyTo(memoryStream);
				stream.Close();
				textureLoadingUsedMemory -= textureToDecompress_s.data.Length;
				platform.gc_free(ref textureToDecompress_s.data);
				memoryStream.Position = 0L;
				textureLoadingUsedMemory += memoryStream.GetBuffer().Length;
				lock (textureToPushOnGpuList)
				{
					textureToPushOnGpuList.Add(new TextureToPushOnGpu_s
					{
						assetPath = textureToDecompress_s.assetPath,
						data = memoryStream
					});
				}
				lock (assetLoadingState)
				{
					assetLoadingState.state = AssetLoadingStateEnum.textureWaitInMemory;
				}
			}
		}

		public static void start_background_loading(AssetId_s[] assetIdArray, bool loadAllProtobufAssets)
		{
			active_wait_interrupt_background_loading();
			textureLoadingUsedMemory = 0;
			loadingThread = utils.start_worker_thread(delegate
			{
				HashSet<AssetId_s> hashSet = new HashSet<AssetId_s>();
				AssetId_s[] array = assetIdArray;
				for (int i = 0; i < array.Length; i++)
				{
					AssetId_s assetId_s = array[i];
					rec_load_dependencies(hashSet, assetId_s.assetPath, assetId_s.type, allProtobufMode: false);
				}
				HashSet<AssetId_s> hashSet2 = new HashSet<AssetId_s>();
				foreach (AssetId_s key in assetById.Keys)
				{
					if (key.type == typeof(Texture2D) || key.type == typeof(Wwise.SoundBankDummyAsset))
					{
						hashSet2.Add(key);
					}
				}
				foreach (AssetId_s item in hashSet)
				{
					hashSet2.Remove(item);
				}
				assetNotInUseSet = hashSet2;
				if (unloadAssetsNotInUse)
				{
					unload_assets_not_in_use();
				}
				textureReadFinished = false;
				Thread[] array2 = new Thread[2];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = utils.start_worker_thread(decompress_thread_method, "texture decompression thread " + j.to_string());
				}
				lock (texturePathLoadingList)
				{
					texturePathLoadingList.Sort(texture_path_compare);
					foreach (string texturePathLoading in texturePathLoadingList)
					{
						if (abordBackgroundLoading)
						{
							break;
						}
						AssetLoadingState assetLoadingState = get_loading_state(new AssetId_s(texturePathLoading, typeof(Texture2D)));
						lock (assetLoadingState)
						{
							if (assetLoadingState.state == AssetLoadingStateEnum.none)
							{
								assetLoadingState.state = AssetLoadingStateEnum.textureDiskToMemory;
								goto IL_01de;
							}
						}
						continue;
						IL_01de:
						get_texture_bigfile_info(texturePathLoading, out var bigfileStream, out var lp);
						yield_if_not_enough_texture_loading_memory_and_add(lp.length);
						lock (bigfileStream)
						{
							byte[] array3 = new byte[lp.length];
							bigfileStream.Position = lp.position;
							bigfileStream.Read(array3, 0, lp.length);
							lock (textureToDecompressList)
							{
								textureToDecompressList.Add(new TextureToDecompress_s
								{
									assetPath = texturePathLoading,
									data = array3
								});
							}
						}
					}
					texturePathLoadingList.Clear();
					textureReadFinished = true;
				}
				while (true)
				{
					bool flag = false;
					for (int k = 0; k < array2.Length; k++)
					{
						if (array2[k].IsAlive)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						break;
					}
					Thread.Sleep(10);
					Thread.Yield();
				}
			}, "main assets loading");
		}

		public static void active_wait_interrupt_background_loading()
		{
			abordBackgroundLoading = true;
			while (!background_loading_finished())
			{
				update_on_main_thread(canCreateNewThreads: false);
				lock (texturePathLoadingList)
				{
					texturePathLoadingList.Clear();
				}
			}
			abordBackgroundLoading = false;
		}

		public static bool background_loading_finished()
		{
			if (loadingThread != null && loadingThread.IsAlive)
			{
				return false;
			}
			lock (textureToDecompressList)
			{
				if (textureToDecompressList.Count > 0)
				{
					return false;
				}
			}
			lock (textureToPushOnGpuList)
			{
				if (textureToPushOnGpuList.Count > 0)
				{
					return false;
				}
			}
			lock (priorityGraphicsAssetLoadingList)
			{
				lock (texturePathLoadingList)
				{
					foreach (AssetId_s priorityGraphicsAssetLoading in priorityGraphicsAssetLoadingList)
					{
						load_asset(priorityGraphicsAssetLoading);
					}
					priorityGraphicsAssetLoadingList.Clear();
					if (texturePathLoadingList.Count == 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static string format_memory(long bytes)
		{
			bytes /= 1000000;
			return $"{bytes:#,0}";
		}

		public static string get_memory_trace()
		{
			long process_memory = platform.get_process_memory();
			long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
			long num = IntermediateTextureStreamsMemory;
			long num2 = TotalTextureMemory;
			long num3 = Wwise.native_wwise_get_total_memory();
			string text = "Total: " + format_memory(process_memory) + "\n";
			text = text + "Texture Loading Streams: " + format_memory(num) + "\n";
			text = text + "Managed (no texture streams): " + format_memory(totalMemory - num) + "\n";
			text = text + "Textures: " + format_memory(num2) + "\n";
			text = text + "Wwise: " + format_memory(num3) + "\n";
			text = text + "Untracked: " + format_memory(process_memory - totalMemory - num2 - num3) + "\n";
			return text + "Untracked+Managed (no texture streams): " + format_memory(process_memory - totalMemory + num - num2 - num3) + "\n";
		}

		public static bool xna_asset_can_be_loaded_on_worker_thread(Type type)
		{
			if (type != typeof(Texture2D) && type != typeof(SpriteFont))
			{
				return type != typeof(Effect);
			}
			return false;
		}

		public static void update_on_main_thread(bool canCreateNewThreads = true)
		{
			lock (priorityGraphicsAssetLoadingList)
			{
				foreach (AssetId_s priorityGraphicsAssetLoading in priorityGraphicsAssetLoadingList)
				{
					load_asset(priorityGraphicsAssetLoading);
				}
				priorityGraphicsAssetLoadingList.Clear();
			}
			if (loadingThread != null && !loadingThread.IsAlive)
			{
				loadingThread = null;
			}
			stopwatch.Restart();
			while (stopwatch.ElapsedMilliseconds < 33)
			{
				TextureToPushOnGpu_s textureToPushOnGpu_s = default(TextureToPushOnGpu_s);
				lock (textureToPushOnGpuList)
				{
					if (textureToPushOnGpuList.Count <= 0)
					{
						return;
					}
					textureToPushOnGpu_s = textureToPushOnGpuList[0];
					textureToPushOnGpuList.RemoveAt(0);
				}
				if (textureToPushOnGpu_s.assetPath != null)
				{
					AssetLoadingState assetLoadingState = get_loading_state(new AssetId_s(textureToPushOnGpu_s.assetPath, typeof(Texture2D)));
					lock (assetLoadingState)
					{
						assetLoadingState.state = AssetLoadingStateEnum.textureMemoryToGpu;
					}
					isLoadingTextureOnMainThreadInUpdate = true;
					byte[] buffer = textureToPushOnGpu_s.data.GetBuffer();
					load_asset(new AssetId_s(textureToPushOnGpu_s.assetPath, typeof(Texture2D)), textureToPushOnGpu_s.data);
					textureLoadingUsedMemory -= buffer.Length;
					platform.gc_free(ref buffer);
					isLoadingTextureOnMainThreadInUpdate = false;
				}
				if (!canLoadMoreThanOneTexturePerFrame)
				{
					break;
				}
			}
		}

		[Conditional("DEBUG")]
		private static void xna_assert_asset_can_be_loaded(Type type)
		{
		}

		private static object xna_load_asset(AssetId_s assetId, Stream customStream)
		{
			if (customStream == null)
			{
				customStream = get_asset_stream(assetId.assetPath, assetId.type, out var _);
			}
			if (assetId.type == typeof(Texture2D))
			{
				Texture2D texture2D;
				lock (customStream)
				{
					texture2D = graphics.read_asset<Texture2D>(assetId.assetPath, customStream);
				}
				totalTextureMemory += texture2D.Width * texture2D.Height * 4;
				return texture2D;
			}
			if (assetId.type == typeof(Effect))
			{
				return graphics.read_asset<Effect>(assetId.assetPath, customStream);
			}
			if (assetId.type == typeof(SpriteFont))
			{
				return graphics.read_asset<SpriteFont>(assetId.assetPath, customStream);
			}
			throw new Exception();
		}

		private static object xna_get_asset_from_stored_asset<T>(object storedAsset) where T : class
		{
			if (typeof(T) == typeof(Effect) || typeof(T) == typeof(SpriteFont))
			{
				return (T)storedAsset;
			}
			if (typeof(T) == typeof(Texture2D))
			{
				return (T)storedAsset;
			}
			throw new Exception();
		}

		private static object xna_get_temporary_loading_asset(AssetId_s assetId)
		{
			return null;
		}
	}
}
