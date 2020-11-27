using System;

namespace CommonLib
{
	public class InputStringAttribute : Attribute
	{
		public readonly bool isPassword;

		public InputStringAttribute(bool isPassword = false)
		{
			this.isPassword = isPassword;
		}
	}
}
