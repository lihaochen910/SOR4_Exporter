using System;

namespace CommonLib
{
	public struct AssetId_s : IEquatable<AssetId_s>
	{
		public string assetPath;

		public Type type;

		public AssetId_s(string assetPath, Type type)
		{
			this.assetPath = assetPath;
			this.type = type;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AssetId_s))
			{
				return false;
			}
			AssetId_s assetId_s = (AssetId_s)obj;
			if (assetPath == assetId_s.assetPath)
			{
				return type == assetId_s.type;
			}
			return false;
		}

		public bool Equals(AssetId_s other)
		{
			if (assetPath == other.assetPath)
			{
				return type == other.type;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = -1425977799;
			num = num * -1521134295 + assetPath.GetHashCode();
			return num * -1521134295 + type.GetHashCode();
		}

		public static bool operator ==(AssetId_s value1, AssetId_s value2)
		{
			if (value1.assetPath == value2.assetPath)
			{
				return value1.type == value2.type;
			}
			return false;
		}

		public static bool operator !=(AssetId_s value1, AssetId_s value2)
		{
			if (!(value1.assetPath != value2.assetPath))
			{
				return value1.type != value2.type;
			}
			return true;
		}

		public override string ToString()
		{
			return assetPath + ": " + type.Name;
		}
	}
}
