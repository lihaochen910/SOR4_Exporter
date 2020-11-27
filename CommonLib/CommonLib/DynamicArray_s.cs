using System;

namespace CommonLib
{
	internal struct DynamicArray_s<T>
	{
		private static readonly T[] emptyArray = new T[0];

		public T[] array;

		public int count;

		public void init()
		{
			array = emptyArray;
		}

		public void init(int capacity)
		{
			if (capacity == 0)
			{
				array = emptyArray;
			}
			else
			{
				array = new T[capacity];
			}
		}

		public void set_capacity(int capacity)
		{
			if (capacity == array.Length)
			{
				return;
			}
			if (capacity > 0)
			{
				T[] destinationArray = new T[capacity];
				if (count > 0)
				{
					Array.Copy(array, 0, destinationArray, 0, count);
				}
				array = destinationArray;
			}
			else
			{
				array = emptyArray;
			}
		}

		public void clear()
		{
			if (count > 0)
			{
				Array.Clear(array, 0, count);
				count = 0;
			}
		}

		public static int calculate_new_ensure_capacity(int current, int min)
		{
			int num = ((current != 0) ? (current * 2) : 4);
			if ((uint)num > 2146435071u)
			{
				num = 2146435071;
			}
			if (num < min)
			{
				num = min;
			}
			return num;
		}

		public void ensure_capacity(int min)
		{
			if (array.Length < min)
			{
				int capacity = calculate_new_ensure_capacity(array.Length, min);
				set_capacity(capacity);
			}
		}

		public T get_value(int index)
		{
			return array[index];
		}

		public void set_value(int index, T value)
		{
			array[index] = value;
		}

		public void add(T item)
		{
			if (count == array.Length)
			{
				ensure_capacity(count + 1);
			}
			array[count++] = item;
		}

		public void add(ref T item)
		{
			if (count == array.Length)
			{
				ensure_capacity(count + 1);
			}
			array[count++] = item;
		}

		public T get_top()
		{
			return array[count - 1];
		}

		public T pop()
		{
			T result = array[--count];
			array[count] = default(T);
			return result;
		}

		public void remove_at(int index)
		{
			count--;
			if (index < count)
			{
				Array.Copy(array, index + 1, array, index, count - index);
			}
			array[count] = default(T);
		}

		public void unordered_remove_at(int index)
		{
			count--;
			if (index < count)
			{
				array[index] = array[count];
			}
			array[count] = default(T);
		}
	}
}
