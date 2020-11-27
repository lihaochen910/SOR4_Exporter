using System;

namespace CommonLib
{
	public class EditableAssetAttribute : Attribute
	{
		public readonly string displayName;

		public readonly string prefix;

		public readonly string defaultFolder;

		public EditableAssetAttribute(string displayName, string prefix, string defaultFolder)
		{
			this.displayName = displayName;
			this.prefix = prefix;
			this.defaultFolder = defaultFolder;
		}
	}
}
