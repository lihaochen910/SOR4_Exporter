using System;

namespace CommonLib
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class YamlPreviousNameAttribute : Attribute
	{
		private readonly string[] _previousNameArray;

		public string[] PreviousNameArray => _previousNameArray;

		public YamlPreviousNameAttribute(params string[] previousNameArray)
		{
			_previousNameArray = previousNameArray;
		}
	}
}
