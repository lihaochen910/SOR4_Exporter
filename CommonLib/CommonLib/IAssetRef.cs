using System;

namespace CommonLib
{
	public interface IAssetRef
	{
		string Path
		{
			get;
			set;
		}

		Type Type
		{
			get;
		}
	}
}
