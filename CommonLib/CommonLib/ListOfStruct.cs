using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonLib
{
	public class ListOfStruct<T> : IEnumerable<T>, IEnumerable where T : struct
	{
		public struct Enumerator
		{
			private ListOfStruct<T> list;

			private int index;

			public T Current => list._array.array[index];

			internal Enumerator(ListOfStruct<T> list)
			{
				this.list = list;
				index = -1;
			}

			public bool MoveNext()
			{
				index++;
				return (uint)index < (uint)list.Count;
			}
		}

		public struct IteratorStruct
		{
			private ListOfStruct<T> list;

			private int index;

			private int endCount;

			public bool MoveNext
			{
				get
				{
					index++;
					return (uint)index < (uint)endCount;
				}
			}

			public int Index => index;

			internal IteratorStruct(ListOfStruct<T> list)
			{
				this.list = list;
				index = -1;
				endCount = list.Count;
			}

			public void remove()
			{
				list._array.remove_at(index);
				index--;
				endCount--;
			}

			public void add(T item)
			{
				list.add(item);
			}

			public void add_and_iterate(T item)
			{
				list.add(item);
				endCount++;
			}
		}

		private DynamicArray_s<T> _array;

		public T[] Array => _array.array;

		public int Count => _array.count;

		public int Capacity
		{
			get
			{
				return _array.array.Length;
			}
			set
			{
				_array.set_capacity(value);
			}
		}

		public bool IsEmpty => _array.count == 0;

		public int LastIndex => Count - 1;

		public IteratorStruct Iterator => new IteratorStruct(this);

		public ListOfStruct()
		{
			_array.init();
		}

		public ListOfStruct(int capacity)
		{
			_array.init(capacity);
		}

		public void set_count_without_clearing_memory(int count)
		{
			_array.ensure_capacity(count);
			_array.count = count;
		}

		public void clear()
		{
			_array.clear();
		}

		private void Add(T item)
		{
			_array.add(item);
		}

		public void add(T item)
		{
			_array.add(ref item);
		}

		public void add(ref T item)
		{
			_array.add(ref item);
		}

		public void unordered_remove_at(int index)
		{
			_array.unordered_remove_at(index);
		}

		private Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
