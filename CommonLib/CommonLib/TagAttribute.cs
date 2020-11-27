using System;

namespace CommonLib
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class TagAttribute : Attribute
	{
		public readonly int tag;

		public readonly string displayName;

		public readonly bool yaml = true;

		public TagAttribute(int tag, string displayName = null, bool yaml = true)
		{
			this.tag = tag;
			this.displayName = displayName;
			this.yaml = yaml;
		}
	}
}
