using System;

namespace CommonLib
{
	public struct AssetRefOrInline_s<T> : IYamlSurrogate where T : class
	{
		[Tag(1, null, true)]
		public AssetRef_s<T> assetRef;

		[Tag(2, null, true)]
		[CanBeNull]
		public T asset;

		public Type Type => typeof(T);

		public T Value
		{
			get
			{
				if (asset != null)
				{
					return asset;
				}
				return assetRef.Value;
			}
		}

		public T ValueOrNull
		{
			get
			{
				if (IsNull)
				{
					return null;
				}
				return Value;
			}
		}

		public string Path
		{
			get
			{
				return assetRef.Path;
			}
			set
			{
				assetRef.Path = value;
			}
		}

		public bool IsNull
		{
			get
			{
				if (asset == null)
				{
					return assetRef.IsNull;
				}
				return false;
			}
		}

		public bool IsNotNull => !IsNull;

		public object YamlSurrogateObject
		{
			get
			{
				if (asset != null)
				{
					return asset;
				}
				return assetRef.Path;
			}
			set
			{
				if (value is string)
				{
					assetRef.Path = (string)value;
				}
				else
				{
					asset = (T)value;
				}
			}
		}

		public static implicit operator AssetRefOrInline_s<T>(T v)
		{
			AssetRefOrInline_s<T> result = default(AssetRefOrInline_s<T>);
			result.asset = v;
			return result;
		}
	}
}
