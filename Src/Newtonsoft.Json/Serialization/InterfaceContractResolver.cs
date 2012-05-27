using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;


namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Used by <see cref="JsonSerializer"/> to resolves a <see cref="JsonContract"/> using a given <see cref="Interface"/> to specify the members to serialize.
	/// </summary>
	public class InterfaceContractResolver : DefaultContractResolver
	{
		private Type _serializableInterface;
		private Type _baseObjectType;
		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceContractResolver"/> class.
		/// </summary>
		/// <param name="baseObjectType">The type of the object to be serialized</param>
		/// <param name="serializableInterface">The type of the interface to use as a contract</param>
		public InterfaceContractResolver(Type baseObjectType, Type serializableInterface)
			: base(false)
		{
			if (baseObjectType.GetInterface(serializableInterface.FullName) == null)
			{
				throw new ArgumentException("baseObjectType does not implement serializableInterface");
			}

			_serializableInterface = serializableInterface;
			_baseObjectType = baseObjectType;
		}

		/// <summary>
		/// Gets the serializable members for the type.
		/// </summary>
		/// <param name="objectType">The type to get serializable members for.</param>
		/// <returns>The serializable members for the type.</returns>
		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			List<MemberInfo> baseSerializableMembers = base.GetSerializableMembers(objectType);

			//This allows the interface to only be used against the original object type,
			//In all other cases it will fallback to the default serializer
			if (objectType != _baseObjectType)
			{
				return baseSerializableMembers;
			}

			List<MemberInfo> interfaceMembers = GetInterfaceProperties(_serializableInterface).Cast<MemberInfo>().Where(m => !ReflectionUtils.IsIndexedProperty(m)).ToList();

			List<MemberInfo> serializableMembers = baseSerializableMembers.Where(x => interfaceMembers.Contains(x, new MemberNameComparer())).ToList();

			return serializableMembers;
		}

		/// <summary>
		/// Compares members based on their name
		/// </summary>
		protected class MemberNameComparer : IEqualityComparer<MemberInfo>
		{

			public bool Equals(MemberInfo x, MemberInfo y)
			{
				if (x == null || y == null)
				{
					return false;
				}
				return x.Name == y.Name;
			}

			public int GetHashCode(MemberInfo obj)
			{
				return obj.Name.GetHashCode();
			}

		}

		///<summary>
		///Gets all properties on an interface including those from chained interfaces
		///</summary>
		//From http://stackoverflow.com/a/2444090/300996
		private static PropertyInfo[] GetInterfaceProperties(Type type)
		{
			if (!type.IsInterface)
			{
				throw new ArgumentException("type is not an interface");
			}

			var propertyInfos = new List<PropertyInfo>();

			var considered = new List<Type>();
			var queue = new Queue<Type>();
			considered.Add(type);
			queue.Enqueue(type);
			while (queue.Count > 0)
			{
				var subType = queue.Dequeue();
				foreach (var subInterface in subType.GetInterfaces())
				{
					if (considered.Contains(subInterface)) continue;

					considered.Add(subInterface);
					queue.Enqueue(subInterface);
				}

				var typeProperties = subType.GetProperties(
					BindingFlags.FlattenHierarchy
					| BindingFlags.Public
					| BindingFlags.Instance);

				var newPropertyInfos = typeProperties
					.Where(x => !propertyInfos.Contains(x));

				propertyInfos.InsertRange(0, newPropertyInfos);
			}

			return propertyInfos.ToArray();

		}
	}
}
