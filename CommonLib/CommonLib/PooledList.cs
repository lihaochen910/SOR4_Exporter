using System.Collections.Generic;

namespace CommonLib
{
	public class PooledList<T> where T : class, IPooledObject, new()
	{
		private List<T> mainList = new List<T>();

		private List<T> reserveList = new List<T>();

		public int Count => mainList.Count;

		public T this[int index]
		{
			get
			{
				return mainList[index];
			}
			set
			{
				mainList[index] = value;
			}
		}

		public PooledList(int capacity)
		{
			mainList.Capacity = capacity;
			reserveList.Capacity = capacity;
		}

		public T add()
		{
			int count = reserveList.Count;
			T val;
			if (count > 0)
			{
				count--;
				val = reserveList[count];
				reserveList.RemoveAt(count);
			}
			else
			{
				val = new T();
			}
			mainList.Add(val);
			return val;
		}

		public void remove_at(int index)
		{
			T val = mainList[index];
			mainList.RemoveAt(index);
			val.clear();
			reserveList.Add(val);
		}
	}
}
