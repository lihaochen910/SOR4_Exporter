using System;

namespace CommonLib
{
	public class CircularBuffer<T>
	{
		private readonly T[] _buffer;

		private int _start;

		private int _end;

		private int _size;

		public int StartIndex => _start;

		public int EndIndex => _end;

		public int Capacity => _buffer.Length;

		public bool IsFull => Size == Capacity;

		public bool IsEmpty => Size == 0;

		public int Size => _size;

		public T this[int index]
		{
			get
			{
				return _buffer[index];
			}
			set
			{
				_buffer[index] = value;
			}
		}

		public CircularBuffer(int capacity)
			: this(capacity, new T[0])
		{
		}

		public CircularBuffer(int capacity, T[] items)
		{
			if (capacity < 1)
			{
				throw new ArgumentException("Circular buffer cannot have negative or zero capacity.", "capacity");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			if (items.Length > capacity)
			{
				throw new ArgumentException("Too many items to fit circular buffer", "items");
			}
			_buffer = new T[capacity];
			Array.Copy(items, _buffer, items.Length);
			_size = items.Length;
			_start = 0;
			_end = ((_size != capacity) ? _size : 0);
		}

		public T Front()
		{
			ThrowIfEmpty();
			return _buffer[_start];
		}

		public T Back()
		{
			ThrowIfEmpty();
			return _buffer[((_end != 0) ? _end : Capacity) - 1];
		}

		public void PushBack(T item)
		{
			if (IsFull)
			{
				_buffer[_end] = item;
				Increment(ref _end);
				_start = _end;
			}
			else
			{
				_buffer[_end] = item;
				Increment(ref _end);
				_size++;
			}
		}

		public void PushFront(T item)
		{
			if (IsFull)
			{
				Decrement(ref _start);
				_end = _start;
				_buffer[_start] = item;
			}
			else
			{
				Decrement(ref _start);
				_buffer[_start] = item;
				_size++;
			}
		}

		public void PopBack()
		{
			ThrowIfEmpty("Cannot take elements from an empty buffer.");
			Decrement(ref _end);
			_buffer[_end] = default(T);
			_size--;
		}

		public void PopFront()
		{
			ThrowIfEmpty("Cannot take elements from an empty buffer.");
			_buffer[_start] = default(T);
			Increment(ref _start);
			_size--;
		}

		private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(message);
			}
		}

		private void Increment(ref int index)
		{
			if (++index == Capacity)
			{
				index = 0;
			}
		}

		private void Decrement(ref int index)
		{
			if (index == 0)
			{
				index = Capacity;
			}
			index--;
		}
	}
}
