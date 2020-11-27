namespace CommonLib
{
	public struct ArrayDictionary<T> where T : class
	{
		public struct Entry
		{
			[Tag(1, null, true)]
			public T value;
		}

		[Tag(1, null, true)]
		public Entry[] _array;

		public T this[int index]
		{
			get
			{
				return _array[index].value;
			}
			set
			{
				_array[index].value = value;
			}
		}

		public int Count => _array.Length;

		public ArrayDictionary(int length)
		{
			_array = new Entry[length];
		}

		public bool sequence_equal(ArrayDictionary<T> other)
		{
			for (int i = 0; i < _array.Length; i++)
			{
				if (_array[i].value != other._array[i].value)
				{
					return false;
				}
			}
			return true;
		}

		public void copy(ArrayDictionary<T> destination)
		{
			for (int i = 0; i < _array.Length; i++)
			{
				destination._array[i].value = _array[i].value;
			}
		}
	}
}
