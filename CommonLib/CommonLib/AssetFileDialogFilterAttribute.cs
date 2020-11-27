using System;

namespace CommonLib
{
	public class AssetFileDialogFilterAttribute : Attribute
	{
		public readonly string filter;

		public AssetFileDialogFilterAttribute(string filter)
		{
			this.filter = filter;
		}
	}
}
