using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace CommonLib
{
	public static class reflection
	{
		private struct Type_s
		{
			public Member_s[] memberArray;

			public Member_s[] yamlMemberArray;

			public Dictionary<string, Member_s> memberByName;

			public Dictionary<int, Member_s> memberByTag;
		}

		public struct Member_s
		{
			private MemberInfo memberInfo;

			private object defaultValue;

			public ICustomAttributeProvider AttributeProvider => memberInfo;

			public Type Type
			{
				get
				{
					if (memberInfo is PropertyInfo)
					{
						PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
						return propertyInfo.PropertyType;
					}
					FieldInfo fieldInfo = (FieldInfo)memberInfo;
					return fieldInfo.FieldType;
				}
			}

			public string Name => memberInfo.Name;

			public int Tag
			{
				get
				{
					TagAttribute tagAttribute = memberInfo.get_custom_attribute<TagAttribute>();
					return tagAttribute.tag;
				}
			}

			public string DisplayName
			{
				get
				{
					string text = memberInfo.get_custom_attribute<TagAttribute>()?.displayName;
					if (text.is_not_null_or_empty())
					{
						return text;
					}
					return Name;
				}
			}

			public bool IsNull => memberInfo == null;

			public bool IsList => is_list(Type);

			public string CustomGUIName => memberInfo.get_custom_attribute<CustomGUIAttribute>(inherit: true)?.Name;

			public object DefaultValue => defaultValue;

			internal Member_s(MemberInfo memberInfo, object defaultValue)
			{
				this.memberInfo = memberInfo;
				this.defaultValue = defaultValue;
			}

			public bool is_default_value(object value)
			{
				if (value == null && defaultValue == null)
				{
					return true;
				}
				if (Type.can_be_cast_to(typeof(IAssetRef)))
				{
					string path = ((IAssetRef)defaultValue).Path;
					string path2 = ((IAssetRef)value).Path;
					if ((path == null || path == "") && (path2 == null || path2 == ""))
					{
						return true;
					}
					return false;
				}
				if (value != null && value.Equals(defaultValue))
				{
					return true;
				}
				if (Type == typeof(string) && (defaultValue == null || (string)defaultValue == "") && (value == null || (string)value == ""))
				{
					return true;
				}
				return false;
			}

			public object get_value(object obj)
			{
				if (memberInfo is PropertyInfo)
				{
					PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
					return propertyInfo.GetValue(obj, null);
				}
				FieldInfo fieldInfo = (FieldInfo)memberInfo;
				return fieldInfo.GetValue(obj);
			}

			public T get_value<T>(object obj)
			{
				return (T)get_value(obj);
			}

			public void set_value(object obj, object value)
			{
				if (memberInfo is PropertyInfo)
				{
					PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
					propertyInfo.SetValue(obj, value, null);
					return;
				}
				FieldInfo fieldInfo = (FieldInfo)memberInfo;
				if (fieldInfo.FieldType.IsArray && value != null && value.GetType() != fieldInfo.FieldType)
				{
					Array array = Array.CreateInstance(fieldInfo.FieldType.GetElementType(), ((Array)value).Length);
					Array.Copy((Array)value, array, array.Length);
					fieldInfo.SetValue(obj, array);
				}
				else
				{
					fieldInfo.SetValue(obj, value);
				}
			}

			public T[] get_attribute_array<T>() where T : Attribute
			{
				return (T[])memberInfo.GetCustomAttributes<T>();
			}
		}

		public struct DerivedClass_s
		{
			public int tag;

			public Type type;
		}

		private static ConcurrentDictionary<string, Type> typeByFullName = new ConcurrentDictionary<string, Type>();

		private static ConcurrentDictionary<string, Type> typeByFullNameWithoutNamespace = new ConcurrentDictionary<string, Type>();

		private static Type[] allTypesArray;

		private static Type[] allClassesArray;

		private static Dictionary<Type, Type_s> cachedTypeByType = new Dictionary<Type, Type_s>();

		private static Dictionary<Type, DerivedClass_s[]> instantiableDerivedClassArrayByType = new Dictionary<Type, DerivedClass_s[]>();

		private static Dictionary<Type, Type> topmostBaseTypeByType = new Dictionary<Type, Type>();

		private static Member_s[] emptyMemberArray = new Member_s[0];

		[ThreadStatic]
		private static List<Member_s> tempMemberList;

		[ThreadStatic]
		private static List<Member_s> tempYamlMemberList;

		[ThreadStatic]
		private static List<DerivedClass_s> tempSubClassList;

		[ThreadStatic]
		private static List<object> tempObjectList;

		private static Dictionary<Type, string> nameByType = new Dictionary<Type, string>();

		public static Action<Stream, object> delegate_serialize;

		public static Func<Stream, object, Type, Action<string>, object> delegate_deserialize;

		public static Func<object, object> delegate_deep_clone;

		private static Action<string> deserializeSilentExceptionDelegate = deserialize_silent_exception;

		[ThreadStatic]
		private static bool contextIsDeserializing;

		public static bool ContextIsDeserializing => contextIsDeserializing;

		private static void get_cached_type(Type type, out Type_s cachedType)
		{
			if (cachedTypeByType.TryGetValue(type, out cachedType))
			{
				return;
			}
			utils.initialize_thread_static_list(ref tempMemberList);
			utils.initialize_thread_static_list(ref tempYamlMemberList);
			object obj = create_instance(type);
			MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MemberInfo[] array = members;
			foreach (MemberInfo memberInfo in array)
			{
				if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo))
				{
					continue;
				}
				TagAttribute tagAttribute = memberInfo.get_custom_attribute<TagAttribute>();
				if (tagAttribute != null)
				{
					object value;
					if (memberInfo is FieldInfo)
					{
						FieldInfo fieldInfo = (FieldInfo)memberInfo;
						value = fieldInfo.GetValue(obj);
					}
					else
					{
						PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
						value = propertyInfo.GetValue(obj);
					}
					if (tagAttribute.yaml)
					{
						tempYamlMemberList.Add(new Member_s(memberInfo, value));
					}
					tempMemberList.Add(new Member_s(memberInfo, value));
				}
			}
			cachedType.memberArray = tempMemberList.ToArray();
			cachedType.yamlMemberArray = tempYamlMemberList.ToArray();
			cachedType.memberByName = new Dictionary<string, Member_s>(cachedType.memberArray.Length);
			cachedType.memberByTag = new Dictionary<int, Member_s>(cachedType.memberArray.Length);
			for (int j = 0; j < cachedType.memberArray.Length; j++)
			{
				Member_s value2 = cachedType.memberArray[j];
				cachedType.memberByName[value2.Name] = value2;
				cachedType.memberByTag[value2.Tag] = value2;
			}
			lock (cachedTypeByType)
			{
				cachedTypeByType[type] = cachedType;
			}
		}

		public static Member_s[] get_member_array(object obj, bool yaml = false)
		{
			if (obj == null)
			{
				return emptyMemberArray;
			}
			return get_member_array(obj.GetType(), yaml);
		}

		public static Member_s[] get_member_array(Type type, bool yaml = false)
		{
			get_cached_type(type, out var cachedType);
			if (yaml)
			{
				return cachedType.yamlMemberArray;
			}
			return cachedType.memberArray;
		}

		public static Member_s get_member_by_tag(object obj, int tag)
		{
			return get_member_by_tag(obj.GetType(), tag);
		}

		public static Member_s get_member_by_name(object obj, string name)
		{
			return get_member_by_name(obj.GetType(), name);
		}

		public static Member_s get_member_by_name(Type type, string name)
		{
			get_cached_type(type, out var cachedType);
			return cachedType.memberByName.get_or_default(name);
		}

		public static Member_s get_member_by_tag(Type type, int tag)
		{
			get_cached_type(type, out var cachedType);
			return cachedType.memberByTag.get_or_default(tag);
		}

		public static object create_instance(Type type, object sourceObject = null)
		{
			if (type == typeof(string))
			{
				return null;
			}
			if (type.IsAbstract)
			{
				type = get_instantiable_derived_class_array(type)[0].type;
			}
			object obj = Activator.CreateInstance(type);
			if (sourceObject != null)
			{
				Member_s[] array = get_member_array(sourceObject);
				for (int i = 0; i < array.Length; i++)
				{
					Member_s member_s = array[i];
					Member_s member_s2 = get_member_by_name(type, member_s.Name);
					if (!member_s2.IsNull && member_s2.Type == member_s.Type)
					{
						member_s2.set_value(obj, member_s.get_value(sourceObject));
					}
				}
			}
			return obj;
		}

		public static bool is_list(Type type)
		{
			return typeof(IList).IsAssignableFrom(type);
		}

		public static bool can_be_cast_to(this Type source, Type dest)
		{
			if (dest == null)
			{
				return false;
			}
			return dest.IsAssignableFrom(source);
		}

		public static bool is_list(object obj)
		{
			return is_list(obj.GetType());
		}

		public static Type get_list_element_type(Type listType)
		{
			if (listType.IsArray)
			{
				return listType.GetElementType();
			}
			return listType.GetGenericArguments()[0];
		}

		public static DerivedClass_s[] get_instantiable_derived_class_array(Type type)
		{
			if (instantiableDerivedClassArrayByType.TryGetValue(type, out var value))
			{
				return value;
			}
			utils.initialize_thread_static_list(ref tempSubClassList);
			Type[] all_classes_array = get_all_classes_array();
			foreach (Type type2 in all_classes_array)
			{
				if (!type2.IsAbstract && type2.can_be_cast_to(type))
				{
					tempSubClassList.Add(new DerivedClass_s
					{
						tag = 0,
						type = type2
					});
				}
			}
			DerivedClass_s[] array = tempSubClassList.ToArray();
			lock (instantiableDerivedClassArrayByType)
			{
				instantiableDerivedClassArrayByType[type] = array;
				return array;
			}
		}

		public static IList insert_element(IList list, Type listType, int index, object value)
		{
			int num = 0;
			if (list != null)
			{
				num = list.Count;
			}
			if (index < 0)
			{
				index = num + index + 1;
			}
			if (listType.IsArray)
			{
				num++;
				IList list2 = list;
				int num2 = 0;
				list = Array.CreateInstance(get_list_element_type(listType), num);
				for (int i = 0; i < num; i++)
				{
					if (i == index)
					{
						list[i] = value;
						i++;
						if (i == num)
						{
							break;
						}
					}
					list[i] = list2[num2];
					num2++;
				}
			}
			else
			{
				if (list == null)
				{
					list = (IList)Activator.CreateInstance(listType);
				}
				list.Insert(index, value);
			}
			return list;
		}

		public static IList remove_element(IList list, Type listType, int index)
		{
			int count = list.Count;
			if (index < 0)
			{
				index = count + index;
			}
			if (listType.IsArray)
			{
				count--;
				IList list2 = list;
				list = Array.CreateInstance(get_list_element_type(listType), count);
				int num = 0;
				int num2 = 0;
				while (num < count)
				{
					if (num == index)
					{
						num2++;
					}
					list[num] = list2[num2];
					num++;
					num2++;
				}
			}
			else
			{
				list.RemoveAt(index);
			}
			return list;
		}

		private static void get_children_add_to_list(object obj, Type type, List<object> objectList)
		{
			if (obj == null)
			{
				return;
			}
			if (type.IsAssignableFrom(obj.GetType()))
			{
				objectList.Add(obj);
			}
			if (is_list(obj))
			{
				IList list = (IList)obj;
				foreach (object item in list)
				{
					get_children_add_to_list(item, type, objectList);
				}
			}
			else
			{
				Member_s[] array = get_member_array(obj);
				foreach (Member_s member_s in array)
				{
					get_children_add_to_list(member_s.get_value(obj), type, objectList);
				}
			}
		}

		public static T[] get_child_array<T>(object obj)
		{
			utils.initialize_thread_static_list(ref tempObjectList);
			get_children_add_to_list(obj, typeof(T), tempObjectList);
			T[] array = new T[tempObjectList.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (T)tempObjectList[i];
			}
			return array;
		}

		public static Type[] get_all_types_array()
		{
			if (allTypesArray == null)
			{
				List<Type> list = new List<Type>();
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					if (assembly.GetCustomAttribute<AssemblyProtoReflectionAttribute>() == null)
					{
						continue;
					}
					foreach (TypeInfo definedType in assembly.DefinedTypes)
					{
						list.Add(definedType);
					}
				}
				allTypesArray = list.ToArray();
			}
			return allTypesArray;
		}

		public static Type[] get_all_classes_array()
		{
			if (allClassesArray == null)
			{
				List<Type> list = new List<Type>();
				Type[] all_types_array = get_all_types_array();
				foreach (Type type in all_types_array)
				{
					if (type.IsClass)
					{
						list.Add(type);
					}
				}
				allClassesArray = list.ToArray();
			}
			return allClassesArray;
		}

		private static Type type_by_full_name_factory(string fullName)
		{
			Type[] all_types_array = get_all_types_array();
			foreach (Type type in all_types_array)
			{
				if (type.FullName == fullName)
				{
					return type;
				}
			}
			return null;
		}

		public static Type get_type_by_full_name(string fullName)
		{
			return typeByFullName.GetOrAdd(fullName, type_by_full_name_factory);
		}

		public static string get_full_name_without_namespace(Type type)
		{
			string @namespace = type.Namespace;
			if (@namespace == null)
			{
				return type.FullName;
			}
			return type.FullName.Substring(@namespace.Length + 1);
		}

		private static Type type_by_full_name_without_namespace_factory(string name)
		{
			Type[] all_types_array = get_all_types_array();
			foreach (Type type in all_types_array)
			{
				if (get_full_name_without_namespace(type) == name)
				{
					return type;
				}
			}
			return name switch
			{
				"Texture2D" => typeof(Texture2D), 
				"SpriteFont" => typeof(SpriteFont), 
				"Effect" => typeof(Effect), 
				_ => throw new Exception("Can not find type " + name), 
			};
		}

		public static Type get_type_by_full_name_without_namespace(string name)
		{
			return typeByFullNameWithoutNamespace.GetOrAdd(name, type_by_full_name_without_namespace_factory);
		}

		public static bool try_get_attribute<T>(this ICustomAttributeProvider provider, out T attribute, bool inherit = false, T defaultValue = null) where T : Attribute
		{
			if (provider == null)
			{
				attribute = defaultValue;
				return false;
			}
			object[] customAttributes = provider.GetCustomAttributes(typeof(T), inherit);
			if (customAttributes.Length == 0)
			{
				attribute = defaultValue;
				return false;
			}
			attribute = (T)customAttributes[0];
			return true;
		}

		public static bool has_attribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
		{
			if (provider == null)
			{
				return false;
			}
			object[] customAttributes = provider.GetCustomAttributes(typeof(T), inherit);
			return customAttributes.Length != 0;
		}

		public static Type get_topmost_base_type(Type type)
		{
			if (topmostBaseTypeByType.TryGetValue(type, out var value))
			{
				return value;
			}
			value = type.BaseType;
			if (get_all_classes_array().Contains(value) || value.IsConstructedGenericType)
			{
				return get_topmost_base_type(value);
			}
			return type;
		}

		public static object get_value_at_tag(object obj, int tag)
		{
			if (obj == null)
			{
				return null;
			}
			if (obj is IList)
			{
				IList list = (IList)obj;
				if (tag < 0 || tag >= list.Count)
				{
					return null;
				}
				return list[tag];
			}
			Member_s member_s = get_member_by_tag(obj, tag);
			if (member_s.IsNull)
			{
				return null;
			}
			return member_s.get_value(obj);
		}

		public static object get_value_at_tag_enumerable<T>(object obj, T tagEnumerable) where T : IEnumerable<int>
		{
			if (tagEnumerable != null)
			{
				foreach (int item in tagEnumerable)
				{
					obj = get_value_at_tag(obj, item);
				}
				return obj;
			}
			return obj;
		}

		public static string get_path_from_tag_enumerable<T>(Type type, T tagEnumerable) where T : IEnumerable<int>
		{
			string text = "";
			if (tagEnumerable != null)
			{
				foreach (int item in tagEnumerable)
				{
					Member_s member_s = get_member_by_tag(type, item);
					if (text != "")
					{
						text += member_s.Name;
					}
					type = member_s.Type;
				}
				return text;
			}
			return text;
		}

		public static object set_value_at_tag(object obj, int tag, object value)
		{
			if (obj == null)
			{
				return null;
			}
			if (obj is IList)
			{
				IList list = (IList)obj;
				if (tag < 0 || tag >= list.Count)
				{
					return null;
				}
				list[tag] = value;
			}
			else
			{
				Member_s member_s = get_member_by_tag(obj, tag);
				if (member_s.IsNull)
				{
					return null;
				}
				member_s.set_value(obj, value);
			}
			return obj;
		}

		public static object set_value<T>(object obj, T tagList, object value) where T : IList<int>
		{
			if (tagList != null)
			{
				List<object> list = new List<object>();
				list.Add(obj);
				for (int i = 0; i < tagList.Count - 1; i++)
				{
					int tag = tagList[i];
					list.Add(get_value_at_tag(list.get_last(), tag));
				}
				for (int num = tagList.Count - 1; num >= 0; num--)
				{
					int tag2 = tagList[num];
					set_value_at_tag(list[num], tag2, value);
					value = list[num];
				}
			}
			return obj;
		}

		private static object get_value_at_id(object obj, string valueId)
		{
			if (valueId.is_null_or_empty())
			{
				return obj;
			}
			if (obj == null)
			{
				return null;
			}
			if (obj is IList)
			{
				IList list = (IList)obj;
				if (list.Count == 0)
				{
					return null;
				}
				int i = Convert.ToInt32(valueId);
				i = i.clamp(0, list.Count - 1);
				return list[i];
			}
			Member_s member_s = get_member_by_name(obj, valueId);
			if (member_s.IsNull)
			{
				return null;
			}
			return member_s.get_value(obj);
		}

		private static void get_object_and_type_at_id(ref object obj, ref Type type, string valueId)
		{
			if (obj != null)
			{
				type = obj.GetType();
			}
			if (valueId.is_null_or_empty() || type == null)
			{
				return;
			}
			if (obj != null)
			{
				obj = get_value_at_id(obj, valueId);
				if (obj != null)
				{
					type = obj.GetType();
					return;
				}
			}
			if (is_list(type))
			{
				type = get_list_element_type(type);
				return;
			}
			Member_s member_s = get_member_by_name(type, valueId);
			if (member_s.IsNull)
			{
				type = null;
			}
			else
			{
				type = member_s.Type;
			}
		}

		public static object get_value_at_path(object obj, string valuePath)
		{
			if (valuePath.is_null_or_empty())
			{
				return obj;
			}
			string[] array = valuePath.Split('.');
			string[] array2 = array;
			foreach (string valueId in array2)
			{
				obj = get_value_at_id(obj, valueId);
			}
			return obj;
		}

		public static Type get_type_at_path(object root, Type rootType, string valuePath)
		{
			if (valuePath.is_null_or_empty())
			{
				return rootType;
			}
			string[] array = valuePath.Split('.');
			string[] array2 = array;
			foreach (string valueId in array2)
			{
				get_object_and_type_at_id(ref root, ref rootType, valueId);
			}
			return rootType;
		}

		private static object set_value_at_id(object obj, string id, object value)
		{
			if (obj == null)
			{
				return null;
			}
			if (obj is IList)
			{
				IList list = (IList)obj;
				int i = Convert.ToInt32(id);
				i = i.clamp(0, list.Count - 1);
				list[i] = value;
			}
			else
			{
				Member_s member_s = get_member_by_name(obj, id);
				if (member_s.IsNull)
				{
					return null;
				}
				member_s.set_value(obj, value);
			}
			return obj;
		}

		public static object set_value_at_path(object obj, string path, object value)
		{
			if (path.is_null_or_empty())
			{
				return value;
			}
			string[] array = path.Split('.');
			List<object> list = new List<object>();
			list.Add(obj);
			for (int i = 0; i < array.Length - 1; i++)
			{
				string valueId = array[i];
				list.Add(get_value_at_id(list.get_last(), valueId));
			}
			for (int num = array.Length - 1; num >= 0; num--)
			{
				string id = array[num];
				set_value_at_id(list[num], id, value);
				value = list[num];
			}
			return obj;
		}

		public static object deep_clone(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Type type = obj.GetType();
			if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
			{
				return obj;
			}
			if (type.IsArray)
			{
				Array array = (Array)obj;
				int length = array.Length;
				Array array2 = Array.CreateInstance(type.GetElementType(), length);
				for (int i = 0; i < length; i++)
				{
					array2.SetValue(deep_clone(array.GetValue(i)), i);
				}
				return array2;
			}
			if (is_list(type))
			{
				IList list = (IList)obj;
				IList list2 = (IList)Activator.CreateInstance(type);
				{
					foreach (object item in list)
					{
						list2.Add(deep_clone(item));
					}
					return list2;
				}
			}
			object obj2 = Activator.CreateInstance(type);
			Member_s[] array3 = get_member_array(type);
			for (int j = 0; j < array3.Length; j++)
			{
				Member_s member_s = array3[j];
				object obj3 = member_s.get_value(obj);
				obj3 = deep_clone(obj3);
				member_s.set_value(obj2, obj3);
			}
			return obj2;
		}

		public static T get_custom_attribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : class
		{
			object[] customAttributes = provider.GetCustomAttributes(typeof(T), inherit);
			if (customAttributes.Length != 0)
			{
				return customAttributes[0] as T;
			}
			return null;
		}

		public static string find_object_and_get_value_path(object root, object objToFind, string valuePath = "")
		{
			if (objToFind == root)
			{
				return valuePath;
			}
			if (root == null)
			{
				return null;
			}
			if (root is IList)
			{
				IList list = (IList)root;
				for (int i = 0; i < list.Count; i++)
				{
					string text = find_object_and_get_value_path(list[i], objToFind, valuePath);
					if (text != null)
					{
						return append_value_path(i.to_string(), text);
					}
				}
			}
			else
			{
				Member_s[] array = get_member_array(root);
				for (int j = 0; j < array.Length; j++)
				{
					Member_s member_s = array[j];
					string text2 = find_object_and_get_value_path(member_s.get_value(root), objToFind, valuePath);
					if (text2 != null)
					{
						return append_value_path(member_s.Name, text2);
					}
				}
			}
			return null;
		}

		public static string append_value_path(string part1, string part2)
		{
			if (part1.is_null_or_empty())
			{
				return part2 ?? "";
			}
			if (part2.is_null_or_empty())
			{
				return part1 ?? "";
			}
			return part1 + "." + part2;
		}

		public static string get_display_name(this Type type)
		{
			if (type == null)
			{
				return "null";
			}
			if (nameByType.TryGetValue(type, out var value))
			{
				return value;
			}
			value = type.get_custom_attribute<DisplayNameAttribute>()?.name;
			if (value != null)
			{
				nameByType.Add(type, value);
				return value;
			}
			value = type.get_custom_attribute<TagAttribute>()?.displayName;
			if (value != null)
			{
				nameByType.Add(type, value);
				return value;
			}
			value = type.Name;
			nameByType.Add(type, value);
			return value;
		}

		public static string get_display_name(this object obj)
		{
			if (obj == null)
			{
				return "null";
			}
			if (obj is INamed)
			{
				return ((INamed)obj).Name;
			}
			return obj.GetType().get_display_name();
		}

		public static string get_value_path(List<string> valueIdList, int lastIndexDif = 0)
		{
			if (valueIdList.is_null_or_empty())
			{
				return "";
			}
			string text = valueIdList[0];
			for (int i = 1; i < valueIdList.Count + lastIndexDif; i++)
			{
				text = text + "." + valueIdList[i];
			}
			return text;
		}

		public static string get_value_path(List<IdValuePair_s> valueIdList, int lastIndexDif = 0)
		{
			if (valueIdList.is_null_or_empty() || valueIdList.Count + lastIndexDif <= 0)
			{
				return "";
			}
			string text = valueIdList[0].id;
			int num = (valueIdList.Count + lastIndexDif) % (valueIdList.Count + 1);
			for (int i = 1; i < num; i++)
			{
				text = text + "." + valueIdList[i].id;
			}
			return text;
		}

		public static IdValuePair_s[] get_id_value_pair_array(object root, string valuePath)
		{
			if (valuePath.is_null_or_empty() || root == null)
			{
				return new IdValuePair_s[0];
			}
			string[] array = valuePath.Split('.');
			IdValuePair_s[] array2 = new IdValuePair_s[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				root = get_value_at_id(root, text);
				array2[i].id = text;
				array2[i].value = root;
			}
			return array2;
		}

		public static bool deep_equals(object a, object b, bool yaml = false)
		{
			if (a == null)
			{
				return b == null;
			}
			if (b == null)
			{
				return a == null;
			}
			if (a is IList || b is IList)
			{
				if (!(a is IList) || !(b is IList))
				{
					return false;
				}
				IList list = (IList)a;
				IList list2 = (IList)b;
				if (list.Count != list2.Count)
				{
					return false;
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (!deep_equals(list[i], list2[i], yaml))
					{
						return false;
					}
				}
				return true;
			}
			if (a.GetType() != b.GetType())
			{
				return false;
			}
			if (a.GetType().IsPrimitive || a.GetType() == typeof(string) || a.GetType().IsEnum)
			{
				return a.Equals(b);
			}
			Member_s[] array = get_member_array(a, yaml);
			for (int j = 0; j < array.Length; j++)
			{
				Member_s member_s = array[j];
				if (!deep_equals(member_s.get_value(a), member_s.get_value(b)))
				{
					return false;
				}
			}
			return true;
		}

		private static object get_deep_clone(object source)
		{
			if (source == null)
			{
				return null;
			}
			Type type = source.GetType();
			if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
			{
				return source;
			}
			if (type.IsArray)
			{
				Array array = (Array)source;
				Array array2 = Array.CreateInstance(type.GetElementType(), array.Length);
				for (int i = 0; i < array2.Length; i++)
				{
					array2.SetValue(get_deep_clone(array.GetValue(i)), i);
				}
				return array2;
			}
			if (source is IList)
			{
				IList list = (IList)source;
				IList list2 = (IList)Activator.CreateInstance(type);
				for (int j = 0; j < list.Count; j++)
				{
					list2.Add(get_deep_clone(list[j]));
				}
				return list2;
			}
			object obj = Activator.CreateInstance(type);
			Member_s[] array3 = get_member_array(type);
			for (int k = 0; k < array3.Length; k++)
			{
				Member_s member_s = array3[k];
				member_s.set_value(obj, get_deep_clone(member_s.get_value(source)));
			}
			return obj;
		}

		public static void deep_copy<T>(T source, T dest) where T : class
		{
			Member_s[] array = get_member_array(typeof(T));
			for (int i = 0; i < array.Length; i++)
			{
				Member_s member_s = array[i];
				member_s.set_value(dest, get_deep_clone(member_s.get_value(source)));
			}
		}

		private static void deserialize_silent_exception(string message)
		{
			utils.log_write_line(message, LogImportanceEnum.error);
			try
			{
				string path = utils.get_file_path_with_current_date(platform.get_save_file_path("save_game_load_error"), ".txt");
				File.WriteAllText(path, message);
			}
			catch
			{
			}
		}

		public static void bin_serialize_to_file(string filePath, object obj)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			FileStream fileStream = ((!File.Exists(filePath)) ? new FileStream(filePath, FileMode.Create) : new FileStream(filePath, FileMode.Truncate));
			bin_serialize(fileStream, obj);
			fileStream.Close();
		}

		public static void bin_serialize(Stream stream, object obj)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			delegate_serialize(stream, obj);
		}

		public static byte[] bin_serialize_to_memory(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			bin_serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		public static object bin_deserialize(Stream source, object value, Type type, Action<string> noExceptionModeErrorDelegate = null)
		{
			contextIsDeserializing = true;
			object result = delegate_deserialize(source, value, type, noExceptionModeErrorDelegate);
			contextIsDeserializing = false;
			return result;
		}

		public static object bin_deserialize(byte[] byteArray, Type type)
		{
			MemoryStream source = new MemoryStream(byteArray);
			contextIsDeserializing = true;
			object result = bin_deserialize(source, null, type);
			contextIsDeserializing = false;
			return result;
		}

		public static object bin_deserialize_from_file(string filePath, Type type)
		{
			using FileStream source = File.OpenRead(filePath);
			contextIsDeserializing = true;
			object result = bin_deserialize(source, null, type);
			contextIsDeserializing = false;
			return result;
		}

		public static string bin_deserialize_from_file_to_object_no_throw_deserialize_error(string filePath, object obj)
		{
			FileStream fileStream = File.OpenRead(filePath);
			try
			{
				contextIsDeserializing = true;
				bin_deserialize(fileStream, obj, obj.GetType());
				contextIsDeserializing = false;
			}
			catch (Exception ex)
			{
				return $"{ex.GetType()} {ex.Message}";
			}
			finally
			{
				fileStream.Close();
			}
			return null;
		}

		public static void bin_deserialize_save_game_from_file_to_object(string filePath, object obj)
		{
			FileStream fileStream = File.OpenRead(filePath);
			try
			{
				contextIsDeserializing = true;
				bin_deserialize(fileStream, obj, obj.GetType(), deserializeSilentExceptionDelegate);
				contextIsDeserializing = false;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				fileStream.Close();
			}
		}

		public static T bin_deserialize<T>(Stream source)
		{
			contextIsDeserializing = true;
			T result = (T)bin_deserialize(source, null, typeof(T));
			contextIsDeserializing = false;
			return result;
		}

		public static T protobuf_deep_clone<T>(T obj)
		{
			contextIsDeserializing = true;
			T result = (T)delegate_deep_clone(obj);
			contextIsDeserializing = false;
			return result;
		}

		public static T bin_instantiate<T>(string uid, bool keepInCache = false)
		{
			throw new NotImplementedException();
		}
	}
}
