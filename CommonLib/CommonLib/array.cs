using System;

namespace CommonLib
{
	public static class array
	{
		public static int add<T>(ref T[] array)
		{
			int num = ((array == null) ? 1 : (array.Length + 1));
			Array.Resize(ref array, num);
			return num - 1;
		}

		public static int add<T>(ref T[] array, T value)
		{
			int num = add(ref array);
			array[num] = value;
			return num;
		}

		public static int add<T>(ref T[] array, ref T value)
		{
			int num = add(ref array);
			array[num] = value;
			return num;
		}

		public static void add<T>(ref T[] array, T[] otherArray)
		{
			if (!otherArray.is_null_or_empty())
			{
				int num = 0;
				if (array != null)
				{
					num = array.Length;
				}
				Array.Resize(ref array, num + otherArray.Length);
				Array.Copy(otherArray, 0, array, num, otherArray.Length);
			}
		}

		public static void insert<T>(ref T[] array, int index, T value)
		{
			Array.Resize(ref array, array.Length + 1);
			for (int num = array.Length - 1; num > index; num--)
			{
				array[num] = array[num - 1];
			}
			array[index] = value;
		}

		public static void remove_at<T>(ref T[] array, int index)
		{
			for (int i = index; i < array.Length - 1; i++)
			{
				array[i] = array[i + 1];
			}
			Array.Resize(ref array, array.Length - 1);
		}

		public static void remove_at(ref Array array, int index)
		{
			for (int i = index; i < array.Length - 1; i++)
			{
				array.SetValue(array.GetValue(i + 1), i);
			}
			resize(ref array, array.Length - 1);
		}

		public static Type get_element_type(this Array array)
		{
			return array.GetType().GetElementType();
		}

		public static void resize(ref Array array, int newSize)
		{
			Array array2 = Array.CreateInstance(array.get_element_type(), newSize);
			Array.Copy(array, array2, newSize);
			array = array2;
		}

		public static void ensure_capacity<T>(ref T[] array, int min)
		{
			if (array == null || array.Length < min)
			{
				Array.Resize(ref array, min);
			}
		}

		public static void resize<T>(ref T[] array, int newSize)
		{
			Array.Resize(ref array, newSize);
		}

		public static void resize<T>(ref T[,] original, int newCoNum, int newRoNum)
		{
			T[,] array = new T[newCoNum, newRoNum];
			int length = original.GetLength(1);
			int upperBound = original.GetUpperBound(0);
			for (int i = 0; i <= upperBound; i++)
			{
				Array.Copy(original, i * length, array, i * newRoNum, length);
			}
			original = array;
		}

		public static void clear<T>(this T[] array)
		{
			if (!array.is_null_or_empty())
			{
				Array.Clear(array, 0, array.Length);
			}
		}

		public static void shift<T>(ref T[] array, int value)
		{
			if (value < 0)
			{
				Array.Copy(array, -value, array, 0, array.Length + value);
			}
			else if (value > 0)
			{
				Array.Copy(array, 0, array, value, array.Length - value);
			}
		}

		public static bool equals<T>(T[] a, T[] otherA)
		{
			if (!a.length_equals(otherA))
			{
				return false;
			}
			if (a != null)
			{
				for (int i = 0; i < a.Length; i++)
				{
					if (!a[i].Equals(otherA[i]))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
