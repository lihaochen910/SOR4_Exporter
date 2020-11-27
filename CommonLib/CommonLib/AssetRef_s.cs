using System;

namespace CommonLib
{
	public struct AssetRef_s<T> : IYamlCustomSerialization, IAssetRef, INamed where T : class
	{
		private string path;

		private T strongRef;

		private uint version;

		[Tag(1, null, true)]
		public string Path
		{
			get
			{
				return path ?? "";
			}
			set
			{
				if (!(value == path))
				{
					strongRef = null;
					path = value;
				}
			}
		}

		public bool IsNull => path.is_null_or_empty();

		public bool IsNotNull => path.is_not_null_or_empty();

		public T Value
		{
			get
			{
				load();
				return strongRef;
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

		public Type Type => typeof(T);

		public string Name
		{
			get
			{
				if (path.is_null_or_empty())
				{
					return "none";
				}
				return path.split_and_get_last('/');
			}
			set
			{
			}
		}

		public AssetId_s AssetId => new AssetId_s(Path, typeof(T));

		public AssetRef_s(string path)
		{
			this = default(AssetRef_s<T>);
			Path = path;
		}

		public void load()
		{
			if (version != asset_cache.assetByIdVersion)
			{
				version = asset_cache.assetByIdVersion;
				strongRef = null;
			}
			if (strongRef == null)
			{
				strongRef = asset_cache.get<T>(path);
			}
		}

		public void release()
		{
			strongRef = null;
		}

		public string yaml_serialize()
		{
			return path;
		}

		public void yaml_deserialize(string src)
		{
			path = src;
		}

		public void nullify()
		{
			path = null;
			strongRef = null;
		}

		public override string ToString()
		{
			return Path;
		}
	}
}
