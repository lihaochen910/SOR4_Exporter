using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace CommonLib
{
	public static class simple_serializer
	{
		private static StreamWriter writer;

		private static StreamReader reader;

		private static void rec_serialize(object obj, string name)
		{
			Type type = obj.GetType();
			if (type == typeof(int))
			{
				writer.WriteLine("{0}: {1}", name, obj);
			}
			else if (type == typeof(bool))
			{
				writer.WriteLine("{0}: {1}", name, obj);
			}
			else if (type == typeof(float))
			{
				writer.WriteLine("{0}: {1}", name, obj);
			}
			else if (type == typeof(string))
			{
				writer.WriteLine("{0}: {1}", name, obj);
			}
			else if (type.IsEnum)
			{
				writer.WriteLine("{0}: {1}", name, (int)obj);
			}
			else if (type.IsArray)
			{
				IList list = (IList)obj;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						rec_serialize(list[i], $"{name}[{i}]");
					}
				}
			}
			else if (type.IsClass || type.IsValueType)
			{
				if (name.is_not_null_or_empty() && !name.EndsWith("]"))
				{
					name += ".";
				}
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					rec_serialize(fieldInfo.GetValue(obj), name + fieldInfo.Name);
				}
			}
		}

		public static void serialize(object obj, string path, bool canThrow)
		{
			try
			{
				writer = new StreamWriter(path);
			}
			catch (Exception ex)
			{
				if (canThrow)
				{
					throw ex;
				}
				Console.WriteLine("Could not write to file: " + path + ". " + ex.Message);
				return;
			}
			rec_serialize(obj, "");
			writer.Close();
		}

		private static void rec_deserialize(object obj, string line)
		{
			if (obj == null)
			{
				return;
			}
			string[] array = line.Split('[', ']');
			string[] array2 = line.Split('.');
			if (array.Length == 1 && array2.Length == 1)
			{
				array = line.Split(':');
				if (array.Length != 2)
				{
					return;
				}
				FieldInfo field = obj.GetType().GetField(array[0]);
				if (field == null)
				{
					return;
				}
				int result4;
				if (field.FieldType == typeof(int))
				{
					if (int.TryParse(array[1], out var result))
					{
						field.SetValue(obj, result);
					}
				}
				else if (field.FieldType == typeof(bool))
				{
					if (bool.TryParse(array[1], out var result2))
					{
						field.SetValue(obj, result2);
					}
				}
				else if (field.FieldType == typeof(float))
				{
					if (float.TryParse(array[1], out var result3))
					{
						field.SetValue(obj, result3);
					}
				}
				else if (field.FieldType == typeof(string))
				{
					field.SetValue(obj, array[1]);
				}
				else if (field.FieldType.IsEnum && int.TryParse(array[1], out result4))
				{
					field.SetValue(obj, result4);
				}
			}
			else if (array.Length == 3)
			{
				FieldInfo field2 = obj.GetType().GetField(array[0]);
				if (!(field2 == null) && field2.FieldType.IsArray && int.TryParse(array[1], out var result5))
				{
					IList list = (IList)field2.GetValue(obj);
					if (list != null && result5 < list.Count && result5 >= 0)
					{
						rec_deserialize(list[result5], array[2]);
					}
				}
			}
			else
			{
				if (array2.Length != 2)
				{
					return;
				}
				FieldInfo field3 = obj.GetType().GetField(array2[0]);
				if (!(field3 == null) && field3.FieldType.IsClass)
				{
					obj = field3.GetValue(obj);
					if (obj != null)
					{
						rec_deserialize(obj, array2[1]);
					}
				}
			}
		}

		public static void deserialize(object obj, string path)
		{
			if (!File.Exists(path))
			{
				Console.WriteLine("File does not exist: " + path + ".");
				return;
			}
			try
			{
				reader = new StreamReader(path);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not read file: " + path + ". " + ex.Message);
				return;
			}
			string text;
			while ((text = reader.ReadLine()) != null)
			{
				rec_deserialize(obj, text.Replace(" ", ""));
			}
			reader.Close();
		}
	}
}
