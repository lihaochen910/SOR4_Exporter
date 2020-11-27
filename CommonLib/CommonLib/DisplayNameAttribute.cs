using System;

namespace CommonLib
{
	public class DisplayNameAttribute : Attribute
	{
		public readonly string name;

		public DisplayNameAttribute(string name)
		{
			this.name = name;
		}
	}
}
