using System;

namespace CommonLib
{
	public class EnumAttribute : Attribute
	{
		public readonly bool sort;

		public EnumAttribute(bool sort = false)
		{
			this.sort = sort;
		}
	}
}
