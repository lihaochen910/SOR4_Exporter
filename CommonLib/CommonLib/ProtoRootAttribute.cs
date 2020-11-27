using System;

namespace CommonLib
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public class ProtoRootAttribute : Attribute
	{
		public readonly bool reference;

		public ProtoRootAttribute(bool reference = false)
		{
			this.reference = reference;
		}
	}
}
