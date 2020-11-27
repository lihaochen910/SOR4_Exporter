using System;

namespace CommonLib
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class CustomGUIAttribute : Attribute
	{
		private readonly string _name;

		public string Name => _name;

		public CustomGUIAttribute(string name)
		{
			_name = name;
		}
	}
}
