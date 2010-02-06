#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Used by <see cref="JsonSerializer"/> to resolves a <see cref="JsonContract"/> for a given <see cref="Type"/>.
  /// </summary>
  public class DefaultContractResolver : IContractResolver
  {
    internal static readonly IContractResolver Instance = new DefaultContractResolver();
    private static readonly IList<JsonConverter> BuiltInConverters = new List<JsonConverter>
      {
#if !PocketPC && !SILVERLIGHT && !NET20
        new EntityKeyMemberConverter(),
#endif
        new BinaryConverter(),
#if !SILVERLIGHT
        new DataSetConverter(),
        new DataTableConverter()
#endif
      };

    private readonly ThreadSafeStore<Type, JsonContract> _typeContractCache;

    /// <summary>
    /// Gets or sets the default members search flags.
    /// </summary>
    /// <value>The default members search flags.</value>
    public BindingFlags DefaultMembersSearchFlags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultContractResolver"/> class.
    /// </summary>
    public DefaultContractResolver()
    {
      DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.Instance;

      _typeContractCache = new ThreadSafeStore<Type, JsonContract>(CreateContract);
    }

    /// <summary>
    /// Resolves the contract for a given type.
    /// </summary>
    /// <param name="type">The type to resolve a contract for.</param>
    /// <returns>The contract for a given type.</returns>
    public virtual JsonContract ResolveContract(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      return _typeContractCache.Get(type);
    }

    /// <summary>
    /// Gets the serializable members for the type.
    /// </summary>
    /// <param name="objectType">The type to get serializable members for.</param>
    /// <returns>The serializable members for the type.</returns>
    protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
    {
#if !PocketPC && !NET20
      DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
#endif

      List<MemberInfo> defaultMembers = ReflectionUtils.GetFieldsAndProperties(objectType, DefaultMembersSearchFlags)
        .Where(m => !ReflectionUtils.IsIndexedProperty(m)).ToList();
      List<MemberInfo> allMembers = ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        .Where(m => !ReflectionUtils.IsIndexedProperty(m)).ToList();

      List<MemberInfo> serializableMembers = new List<MemberInfo>();
      foreach (MemberInfo member in allMembers)
      {
        if (defaultMembers.Contains(member))
        {
          // add all members that are found by default member search
          serializableMembers.Add(member);
        }
        else
        {
          // add members that are explicitly marked with JsonProperty/DataMember attribute
          if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member) != null)
            serializableMembers.Add(member);
#if !PocketPC && !NET20
          else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(member) != null)
            serializableMembers.Add(member);
#endif
        }
      }

#if !PocketPC && !SILVERLIGHT && !NET20
      Type match;
      // don't include EntityKey on entities objects... this is a bit hacky
      if (objectType.AssignableToTypeName("System.Data.Objects.DataClasses.EntityObject", out match))
        serializableMembers = serializableMembers.Where(ShouldSerializeEntityMember).ToList();
#endif

      return serializableMembers;
    }

#if !PocketPC && !SILVERLIGHT && !NET20
    private bool ShouldSerializeEntityMember(MemberInfo memberInfo)
    {
      PropertyInfo propertyInfo = memberInfo as PropertyInfo;
      if (propertyInfo != null)
      {
        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().FullName == "System.Data.Objects.DataClasses.EntityReference`1")
          return false;
      }

      return true;
    }
#endif

    /// <summary>
    /// Creates a <see cref="JsonObjectContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonObjectContract"/> for the given type.</returns>
    protected virtual JsonObjectContract CreateObjectContract(Type objectType)
    {
      JsonObjectContract contract = new JsonObjectContract(objectType);
      InitializeContract(contract);

      contract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType);
      contract.Properties.AddRange(CreateProperties(contract));
      if (contract.DefaultCreator == null || contract.DefaultCreatorNonPublic)
        contract.ParametrizedConstructor = GetParametrizedConstructor(objectType);

      return contract;
    }

    private ConstructorInfo GetParametrizedConstructor(Type objectType)
    {
      ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

      if (constructors.Length == 1)
        return constructors[0];
      else
        return null;
    }

    /// <summary>
    /// Resolves the default <see cref="JsonConverter" /> for the contract.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns></returns>
    protected virtual JsonConverter ResolveContractConverter(Type objectType)
    {
      return JsonTypeReflector.GetJsonConverter(objectType, objectType);
    }

    private void InitializeContract(JsonContract contract)
    {
      JsonContainerAttribute containerAttribute = JsonTypeReflector.GetJsonContainerAttribute(contract.UnderlyingType);
      if (containerAttribute != null)
      {
        contract.IsReference = containerAttribute._isReference;
      }
#if !PocketPC && !NET20
      else
      {
        DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.UnderlyingType);
        // doesn't have a null value
        if (dataContractAttribute != null && dataContractAttribute.IsReference)
          contract.IsReference = true;
      }
#endif

      contract.Converter = ResolveContractConverter(contract.UnderlyingType);

      // then see whether object is compadible with any of the built in converters
      JsonConverter internalConverter;
      if (JsonSerializer.HasMatchingConverter(BuiltInConverters, contract.UnderlyingType, out internalConverter))
        contract.InternalConverter = internalConverter;

      if (ReflectionUtils.HasDefaultConstructor(contract.CreatedType, true)
        || contract.CreatedType.IsValueType)
      {
#if !PocketPC && !SILVERLIGHT
        contract.DefaultCreator = LateBoundDelegateFactory.CreateDefaultConstructor(contract.CreatedType);
#else
        ConstructorInfo constructorInfo = ReflectionUtils.GetDefaultConstructor(contract.CreatedType, true);

        contract.DefaultCreator = () =>
                                    {
                                      if (contract.CreatedType.IsValueType)
                                        return Activator.CreateInstance(contract.CreatedType);

                                      return constructorInfo.Invoke(null);
                                    };
#endif
        contract.DefaultCreatorNonPublic = (!contract.CreatedType.IsValueType &&
                                            ReflectionUtils.GetDefaultConstructor(contract.CreatedType) == null);
      }

      foreach (MethodInfo method in contract.UnderlyingType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
      {
        // compact framework errors when getting parameters for a generic method
        // lame, but generic methods should not be callbacks anyway
        if (method.ContainsGenericParameters)
          continue;

        Type prevAttributeType = null;
        ParameterInfo[] parameters = method.GetParameters();

#if !PocketPC && !NET20
        if (IsValidCallback(method, parameters, typeof(OnSerializingAttribute), contract.OnSerializing, ref prevAttributeType))
        {
          contract.OnSerializing = method;
        }
        if (IsValidCallback(method, parameters, typeof(OnSerializedAttribute), contract.OnSerialized, ref prevAttributeType))
        {
          contract.OnSerialized = method;
        }
        if (IsValidCallback(method, parameters, typeof(OnDeserializingAttribute), contract.OnDeserializing, ref prevAttributeType))
        {
          contract.OnDeserializing = method;
        }
        if (IsValidCallback(method, parameters, typeof(OnDeserializedAttribute), contract.OnDeserialized, ref prevAttributeType))
        {
          contract.OnDeserialized = method;
        }
#endif
        if (IsValidCallback(method, parameters, typeof(OnErrorAttribute), contract.OnError, ref prevAttributeType))
        {
          contract.OnError = method;
        }
      }
    }

    /// <summary>
    /// Creates a <see cref="JsonDictionaryContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonDictionaryContract"/> for the given type.</returns>
    protected virtual JsonDictionaryContract CreateDictionaryContract(Type objectType)
    {
      JsonDictionaryContract contract = new JsonDictionaryContract(objectType);
      InitializeContract(contract);

      return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonArrayContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonArrayContract"/> for the given type.</returns>
    protected virtual JsonArrayContract CreateArrayContract(Type objectType)
    {
      JsonArrayContract contract = new JsonArrayContract(objectType);
      InitializeContract(contract);

      return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonPrimitiveContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonPrimitiveContract"/> for the given type.</returns>
    protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type objectType)
    {
      JsonPrimitiveContract contract = new JsonPrimitiveContract(objectType);
      InitializeContract(contract);
      
      return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonLinqContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonLinqContract"/> for the given type.</returns>
    protected virtual JsonLinqContract CreateLinqContract(Type objectType)
    {
      JsonLinqContract contract = new JsonLinqContract(objectType);
      InitializeContract(contract);

      return contract;
    }

#if !SILVERLIGHT && !PocketPC
    /// <summary>
    /// Creates a <see cref="JsonISerializableContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonISerializableContract"/> for the given type.</returns>
    protected virtual JsonISerializableContract CreateISerializableContract(Type objectType)
    {
      JsonISerializableContract contract = new JsonISerializableContract(objectType);
      InitializeContract(contract);

      ConstructorInfo constructorInfo = objectType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new [] {typeof (SerializationInfo), typeof (StreamingContext)}, null);
      if (constructorInfo != null)
      {
        MethodCaller<object> methodCaller = LateBoundDelegateFactory.CreateMethodCall(constructorInfo);

        contract.ISerializableCreator = (args => methodCaller(null, args));
      }

      return contract;
    }
#endif

    /// <summary>
    /// Creates a <see cref="JsonStringContract"/> for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonStringContract"/> for the given type.</returns>
    protected virtual JsonStringContract CreateStringContract(Type objectType)
    {
      JsonStringContract contract = new JsonStringContract(objectType);
      InitializeContract(contract);

      return contract;
    }

    /// <summary>
    /// Determines which contract type is created for the given type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>A <see cref="JsonContract"/> for the given type.</returns>
    protected virtual JsonContract CreateContract(Type objectType)
    {
      if (JsonConvert.IsJsonPrimitiveType(objectType))
        return CreatePrimitiveContract(objectType);

      if (JsonTypeReflector.GetJsonObjectAttribute(objectType) != null)
        return CreateObjectContract(objectType);

      if (JsonTypeReflector.GetJsonArrayAttribute(objectType) != null)
        return CreateArrayContract(objectType);

      if (objectType.IsSubclassOf(typeof(JToken)))
        return CreateLinqContract(objectType);

      bool canConvertToString = CanConvertToString(objectType);

#if !SILVERLIGHT && !PocketPC
      if (typeof(ISerializable).IsAssignableFrom(objectType) && !canConvertToString)
        return CreateISerializableContract(objectType);
#endif

      if (CollectionUtils.IsDictionaryType(objectType))
        return CreateDictionaryContract(objectType);

      if (typeof(IEnumerable).IsAssignableFrom(objectType))
        return CreateArrayContract(objectType);

      if (canConvertToString)
        return CreateStringContract(objectType);

      return CreateObjectContract(objectType);
    }

    internal static bool CanConvertToString(Type type)
    {
#if !PocketPC
      TypeConverter converter = ConvertUtils.GetConverter(type);

      // use the objectType's TypeConverter if it has one and can convert to a string
      if (converter != null
#if !SILVERLIGHT
 && !(converter is ComponentConverter)
 && !(converter is ReferenceConverter)
#endif
 && converter.GetType() != typeof(TypeConverter))
      {
        if (converter.CanConvertTo(typeof(string)))
          return true;
      }
#endif

      if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
        return true;

#if SILVERLIGHT || PocketPC
      if (type == typeof(Guid) || type == typeof(Uri) || type == typeof(TimeSpan))
        return true;
#endif

      return false;
    }

    private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo currentCallback, ref Type prevAttributeType)
    {
      if (!method.IsDefined(attributeType, false))
        return false;

      if (currentCallback != null)
        throw new Exception("Invalid attribute. Both '{0}' and '{1}' in type '{2}' have '{3}'.".FormatWith(CultureInfo.InvariantCulture, method, currentCallback, GetClrTypeFullName(method.DeclaringType), attributeType));

      if (prevAttributeType != null)
        throw new Exception("Invalid Callback. Method '{3}' in type '{2}' has both '{0}' and '{1}'.".FormatWith(CultureInfo.InvariantCulture, prevAttributeType, attributeType, GetClrTypeFullName(method.DeclaringType), method));

      if (method.IsVirtual)
        throw new Exception("Virtual Method '{0}' of type '{1}' cannot be marked with '{2}' attribute.".FormatWith(CultureInfo.InvariantCulture, method, GetClrTypeFullName(method.DeclaringType), attributeType));

      if (method.ReturnType != typeof(void))
        throw new Exception("Serialization Callback '{1}' in type '{0}' must return void.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method));

      if (attributeType == typeof(OnErrorAttribute))
      {
        if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
          throw new Exception("Serialization Error Callback '{1}' in type '{0}' must have two parameters of type '{2}' and '{3}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof (StreamingContext), typeof(ErrorContext)));
      }
      else
      {
        if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
          throw new Exception("Serialization Callback '{1}' in type '{0}' must have a single parameter of type '{2}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext)));
      }

      prevAttributeType = attributeType;

      return true;
    }

    internal static string GetClrTypeFullName(Type type)
    {
      if (type.IsGenericTypeDefinition || !type.ContainsGenericParameters)
        return type.FullName;

      return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { type.Namespace, type.Name });
    }

    /// <summary>
    /// Creates properties for the given <see cref="JsonObjectContract"/>.
    /// </summary>
    /// <param name="contract">The contract to create properties for.</param>
    /// <returns>Properties for the given <see cref="JsonObjectContract"/>.</returns>
    protected virtual IList<JsonProperty> CreateProperties(JsonObjectContract contract)
    {
      List<MemberInfo> members = GetSerializableMembers(contract.UnderlyingType);
      if (members == null)
        throw new JsonSerializationException("Null collection of seralizable members returned.");

      JsonPropertyCollection properties = new JsonPropertyCollection(contract);

      foreach (MemberInfo member in members)
      {
        JsonProperty property = CreateProperty(contract, member);

        if (property != null)
          properties.AddProperty(property);
      }

      return properties;
    }

    /// <summary>
    /// Creates the <see cref="IValueProvider"/> used by the serializer to get and set values from a member.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>The <see cref="IValueProvider"/> used by the serializer to get and set values from a member.</returns>
    protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
    {
#if !PocketPC && !SILVERLIGHT
      return new DynamicValueProvider(member);
#else
      return new ReflectionValueProvider(member);
#endif
    }

    /// <summary>
    /// Creates a <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="contract">The member's declaring types <see cref="JsonObjectContract"/>.</param>
    /// <param name="member">The member to create a <see cref="JsonProperty"/> for.</param>
    /// <returns>A created <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.</returns>
    protected virtual JsonProperty CreateProperty(JsonObjectContract contract, MemberInfo member)
    {
      JsonProperty property = new JsonProperty();
      property.PropertyType = ReflectionUtils.GetMemberUnderlyingType(member);
      property.ValueProvider = CreateMemberValueProvider(member);
      
      // resolve converter for property
      // the class type might have a converter but the property converter takes presidence
      property.Converter = JsonTypeReflector.GetJsonConverter(member, property.PropertyType);

#if !PocketPC && !NET20
      DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(member.DeclaringType);

      DataMemberAttribute dataMemberAttribute;
      if (dataContractAttribute != null)
        dataMemberAttribute = JsonTypeReflector.GetAttribute<DataMemberAttribute>(member);
      else
        dataMemberAttribute = null;
#endif

      JsonPropertyAttribute propertyAttribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member);
      bool hasIgnoreAttribute = (JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(member) != null);

      string mappedName;
      if (propertyAttribute != null && propertyAttribute.PropertyName != null)
        mappedName = propertyAttribute.PropertyName;
#if !PocketPC && !NET20
      else if (dataMemberAttribute != null && dataMemberAttribute.Name != null)
        mappedName = dataMemberAttribute.Name;
#endif
      else
        mappedName = member.Name;

      property.PropertyName = ResolvePropertyName(mappedName);

      if (propertyAttribute != null)
        property.Required = propertyAttribute.Required;
#if !PocketPC && !NET20
      else if (dataMemberAttribute != null)
        property.Required = (dataMemberAttribute.IsRequired) ? Required.AllowNull : Required.Default;
#endif
      else
        property.Required = Required.Default;

      property.Ignored = (hasIgnoreAttribute ||
                      (contract.MemberSerialization == MemberSerialization.OptIn
                       && propertyAttribute == null
#if !PocketPC && !NET20
                       && dataMemberAttribute == null
#endif
));

      property.Readable = ReflectionUtils.CanReadMemberValue(member);
      property.Writable = ReflectionUtils.CanSetMemberValue(member);

      property.MemberConverter = JsonTypeReflector.GetJsonConverter(member, ReflectionUtils.GetMemberUnderlyingType(member));

      DefaultValueAttribute defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(member);
      property.DefaultValue = (defaultValueAttribute != null) ? defaultValueAttribute.Value : null;

      property.NullValueHandling = (propertyAttribute != null) ? propertyAttribute._nullValueHandling : null;
      property.DefaultValueHandling = (propertyAttribute != null) ? propertyAttribute._defaultValueHandling : null;
      property.ReferenceLoopHandling = (propertyAttribute != null) ? propertyAttribute._referenceLoopHandling : null;
      property.ObjectCreationHandling = (propertyAttribute != null) ? propertyAttribute._objectCreationHandling : null;
      property.IsReference = (propertyAttribute != null) ? propertyAttribute._isReference : null;

      return property;
    }

    /// <summary>
    /// Resolves the name of the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Name of the property.</returns>
    protected virtual string ResolvePropertyName(string propertyName)
    {
      return propertyName;
    }
  }
}