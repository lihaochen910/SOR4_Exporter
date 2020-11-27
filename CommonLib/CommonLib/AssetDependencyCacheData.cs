using System.Collections.Generic;

namespace CommonLib
{
	[ProtoRoot(false)]
	public class AssetDependencyCacheData
	{
		public struct StringAssetId_s
		{
			[Tag(1, null, true)]
			public string assetPath;

			[Tag(2, null, true)]
			public string typeName;
		}

		public struct Dependencies_s
		{
			[Tag(1, null, true)]
			public StringAssetId_s assetId;

			[Tag(2, null, true)]
			public StringAssetId_s[] dependencyArray;

			public AssetId_s[] get_id_array()
			{
				AssetId_s[] array = new AssetId_s[dependencyArray.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i].assetPath = dependencyArray[i].assetPath;
					array[i].type = reflection.get_type_by_full_name_without_namespace(dependencyArray[i].typeName);
				}
				return array;
			}
		}

		[Tag(1, null, true)]
		public List<Dependencies_s> dependencyList = new List<Dependencies_s>();

		[Tag(2, null, true)]
		public List<Dependencies_s> dependencyIgnoreNoDependencyAttributeList = new List<Dependencies_s>();

		public void set_in_cache()
		{
			foreach (Dependencies_s dependency in dependencyList)
			{
				asset_cache.set_dependency_array(dependency.assetId.assetPath, reflection.get_type_by_full_name_without_namespace(dependency.assetId.typeName), dependency.get_id_array());
			}
			foreach (Dependencies_s dependencyIgnoreNoDependencyAttribute in dependencyIgnoreNoDependencyAttributeList)
			{
				asset_cache.set_dependency_array_ignore_no_dependency_attribute(dependencyIgnoreNoDependencyAttribute.assetId.assetPath, reflection.get_type_by_full_name_without_namespace(dependencyIgnoreNoDependencyAttribute.assetId.typeName), dependencyIgnoreNoDependencyAttribute.get_id_array());
			}
		}
	}
}
