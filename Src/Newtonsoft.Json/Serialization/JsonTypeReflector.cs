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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
#if !NETFX_CORE
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif
using Newtonsoft.Json.Utilities;
#if NETFX_CORE
using ICustomAttributeProvider = Newtonsoft.Json.Utilities.CustomAttributeProvider;
#endif
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Serialization
{
#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
  internal interface IMetadataTypeAttribute
  {
    Type MetadataClassType { get; }
  }
#endif

  internal static class JsonTypeReflector
  {
    public const string IdPropertyName = "$id";
    public const string RefPropertyName = "$ref";
    public const string TypePropertyName = "$type";
    public const string ValuePropertyName = "$value";
    public const string ArrayValuesPropertyName = "$values";

    public const string ShouldSerializePrefix = "ShouldSerialize";
    public const string SpecifiedPostfix = "Specified";

    private static readonly ThreadSafeStore<ICustomAttributeProvider, Type> JsonConverterTypeCache = new ThreadSafeStore<ICustomAttributeProvider, Type>(GetJsonConverterTypeFromAttribute);
#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
    private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(GetAssociateMetadataTypeFromAttribute);

    private const string MetadataTypeAttributeTypeName =
      "System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
    private static Type _cachedMetadataTypeAttributeType;
#endif
#if SILVERLIGHT
    private static readonly ThreadSafeStore<ICustomAttributeProvider, Type> TypeConverterTypeCache = new ThreadSafeStore<ICustomAttributeProvider, Type>(GetTypeConverterTypeFromAttribute);

    private static Type GetTypeConverterTypeFromAttribute(ICustomAttributeProvider attributeProvider)
    {
      TypeConverterAttribute converterAttribute = GetAttribute<TypeConverterAttribute>(attributeProvider);
      if (converterAttribute == null)
        return null;

      return Type.GetType(converterAttribute.ConverterTypeName);
    }

    private static Type GetTypeConverterType(ICustomAttributeProvider attributeProvider)
    {
      return TypeConverterTypeCache.Get(attributeProvider);
    }
#endif

    public static JsonContainerAttribute GetJsonContainerAttribute(Type type)
    {
      return CachedAttributeGetter<JsonContainerAttribute>.GetAttribute(type.GetCustomAttributeProvider());
    }

    public static JsonObjectAttribute GetJsonObjectAttribute(Type type)
    {
      return GetJsonContainerAttribute(type) as JsonObjectAttribute;
    }

    public static JsonArrayAttribute GetJsonArrayAttribute(Type type)
    {
      return GetJsonContainerAttribute(type) as JsonArrayAttribute;
    }

#if !PocketPC && !NET20 
    public static DataContractAttribute GetDataContractAttribute(Type type)
    {
      // DataContractAttribute does not have inheritance
      DataContractAttribute result = null;
      Type currentType = type;
      while (result == null && currentType != null)
      {
        result = CachedAttributeGetter<DataContractAttribute>.GetAttribute(currentType.GetCustomAttributeProvider());
        currentType = currentType.BaseType();
      }

      return result;
    }

    public static DataMemberAttribute GetDataMemberAttribute(MemberInfo memberInfo)
    {
      // DataMemberAttribute does not have inheritance

      // can't override a field
      if (memberInfo.MemberType() == MemberTypes.Field)
        return CachedAttributeGetter<DataMemberAttribute>.GetAttribute(memberInfo.GetCustomAttributeProvider());

      // search property and then search base properties if nothing is returned and the property is virtual
      PropertyInfo propertyInfo = (PropertyInfo) memberInfo;
      DataMemberAttribute result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(propertyInfo.GetCustomAttributeProvider());
      if (result == null)
      {
        if (propertyInfo.IsVirtual())
        {
          Type currentType = propertyInfo.DeclaringType;

          while (result == null && currentType != null)
          {
            PropertyInfo baseProperty = (PropertyInfo)ReflectionUtils.GetMemberInfoFromType(currentType, propertyInfo);
            if (baseProperty != null && baseProperty.IsVirtual())
              result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(baseProperty.GetCustomAttributeProvider());

            currentType = currentType.BaseType();
          }
        }
      }

      return result;
    }
#endif

    public static MemberSerialization GetObjectMemberSerialization(Type objectType)
    {
      JsonObjectAttribute objectAttribute = GetJsonObjectAttribute(objectType);

      if (objectAttribute == null)
      {
#if !PocketPC && !NET20
        DataContractAttribute dataContractAttribute = GetDataContractAttribute(objectType);

        if (dataContractAttribute != null)
          return MemberSerialization.OptIn;
#endif

        return MemberSerialization.OptOut;
      }

      return objectAttribute.MemberSerialization;
    }

    private static Type GetJsonConverterType(ICustomAttributeProvider attributeProvider)
    {
      return JsonConverterTypeCache.Get(attributeProvider);
    }

    private static Type GetJsonConverterTypeFromAttribute(ICustomAttributeProvider attributeProvider)
    {
      JsonConverterAttribute converterAttribute = GetAttribute<JsonConverterAttribute>(attributeProvider);
      return (converterAttribute != null)
        ? converterAttribute.ConverterType
        : null;
    }

    public static JsonConverter GetJsonConverter(ICustomAttributeProvider attributeProvider, Type targetConvertedType)
    {
      object provider = null;
#if !NETFX_CORE
      provider = attributeProvider as MemberInfo;
#else
      provider = attributeProvider.UnderlyingObject;
#endif

      Type converterType = GetJsonConverterType(attributeProvider);

      if (converterType != null)
      {
        JsonConverter memberConverter = JsonConverterAttribute.CreateJsonConverterInstance(converterType);

        if (!memberConverter.CanConvert(targetConvertedType))
          throw new JsonSerializationException("JsonConverter {0} on {1} is not compatible with member type {2}.".FormatWith(CultureInfo.InvariantCulture, memberConverter.GetType().Name, provider, targetConvertedType.Name));

        return memberConverter;
      }

      return null;
    }

#if !NETFX_CORE
#if !PocketPC
    public static TypeConverter GetTypeConverter(Type type)
    {
#if !SILVERLIGHT
      return TypeDescriptor.GetConverter(type);
#else
      Type converterType = GetTypeConverterType(type);

      if (converterType != null)
        return (TypeConverter)ReflectionUtils.CreateInstance(converterType);

      return null;
#endif
#endif
    }
#endif

#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
    private static Type GetAssociatedMetadataType(Type type)
    {
      return AssociatedMetadataTypesCache.Get(type);
    }

    private static Type GetAssociateMetadataTypeFromAttribute(Type type)
    {
      Type metadataTypeAttributeType = GetMetadataTypeAttributeType();
      if (metadataTypeAttributeType == null)
        return null;

      object attribute = type.GetCustomAttributes(metadataTypeAttributeType, true).SingleOrDefault();
      if (attribute == null)
        return null;

      IMetadataTypeAttribute metadataTypeAttribute = (DynamicCodeGeneration)
                                                       ? DynamicWrapper.CreateWrapper<IMetadataTypeAttribute>(attribute)
                                                       : new LateBoundMetadataTypeAttribute(attribute);

      return metadataTypeAttribute.MetadataClassType;
    }

    private static Type GetMetadataTypeAttributeType()
    {
      // always attempt to get the metadata type attribute type
      // the assembly may have been loaded since last time
      if (_cachedMetadataTypeAttributeType == null)
      {
        Type metadataTypeAttributeType = Type.GetType(MetadataTypeAttributeTypeName);

        if (metadataTypeAttributeType != null)
          _cachedMetadataTypeAttributeType = metadataTypeAttributeType;
        else
          return null;
      }
      
      return _cachedMetadataTypeAttributeType;
    }
#endif

    private static T GetAttribute<T>(Type type) where T : Attribute
    {
      T attribute;

#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
      Type metadataType = GetAssociatedMetadataType(type);
      if (metadataType != null)
      {
        attribute = ReflectionUtils.GetAttribute<T>(metadataType, true);
        if (attribute != null)
          return attribute;
      }
#endif

      attribute = ReflectionUtils.GetAttribute<T>(type.GetCustomAttributeProvider(), true);
      if (attribute != null)
        return attribute;

      foreach (Type typeInterface in type.GetInterfaces())
      {
        attribute = ReflectionUtils.GetAttribute<T>(typeInterface.GetCustomAttributeProvider(), true);
        if (attribute != null)
          return attribute;
      }

      return null;
    }

    private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
    {
      T attribute;

#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
      Type metadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
      if (metadataType != null)
      {
        MemberInfo metadataTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(metadataType, memberInfo);

        if (metadataTypeMemberInfo != null)
        {
          attribute = ReflectionUtils.GetAttribute<T>(metadataTypeMemberInfo, true);
          if (attribute != null)
            return attribute;
        }
      }
#endif

      attribute = ReflectionUtils.GetAttribute<T>(memberInfo.GetCustomAttributeProvider(), true);
      if (attribute != null)
        return attribute;

      if (memberInfo.DeclaringType != null)
      {
        foreach (Type typeInterface in memberInfo.DeclaringType.GetInterfaces())
        {
          MemberInfo interfaceTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(typeInterface, memberInfo);

          if (interfaceTypeMemberInfo != null)
          {
            attribute = ReflectionUtils.GetAttribute<T>(interfaceTypeMemberInfo.GetCustomAttributeProvider(), true);
            if (attribute != null)
              return attribute;
          }
        }
      }

      return null;
    }

    public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
    {
      object provider = null;
#if !NETFX_CORE
      provider = attributeProvider;
#else
      provider = attributeProvider.UnderlyingObject;
#endif

      Type type = provider as Type;
      if (type != null)
        return GetAttribute<T>(type);

      MemberInfo memberInfo = provider as MemberInfo;
      if (memberInfo != null)
        return GetAttribute<T>(memberInfo);

      return ReflectionUtils.GetAttribute<T>(attributeProvider, true);
    }

    private static bool? _dynamicCodeGeneration;
    private static bool? _fullyTrusted;

#if DEBUG
    internal static void SetFullyTrusted(bool fullyTrusted)
    {
      _fullyTrusted = fullyTrusted;
    }

    internal static void SetDynamicCodeGeneration(bool dynamicCodeGeneration)
    {
      _dynamicCodeGeneration = dynamicCodeGeneration;
    }
#endif

    public static bool DynamicCodeGeneration
    {
      get
      {
        if (_dynamicCodeGeneration == null)
        {
#if !PocketPC && !SILVERLIGHT && !NETFX_CORE
          try
          {
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Demand();
            new SecurityPermission(SecurityPermissionFlag.SkipVerification).Demand();
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            _dynamicCodeGeneration = true;
          }
          catch (Exception)
          {
            _dynamicCodeGeneration = false;
          }
#else
          _dynamicCodeGeneration = false;
#endif
        }

        return _dynamicCodeGeneration.Value;
      }
    }

    public static bool FullyTrusted
    {
      get
      {
        if (_fullyTrusted == null)
        {
#if !(NET20 || NET35 || SILVERLIGHT)
          AppDomain appDomain = AppDomain.CurrentDomain;

          _fullyTrusted = appDomain.IsHomogenous && appDomain.IsFullyTrusted;
#else
          _fullyTrusted = true;
#endif
        }

        return _fullyTrusted.Value;
      }
    }

    public static ReflectionDelegateFactory ReflectionDelegateFactory
    {
      get
      {
#if !PocketPC && !SILVERLIGHT
        if (DynamicCodeGeneration)
          return DynamicReflectionDelegateFactory.Instance;
#endif

        return LateBoundReflectionDelegateFactory.Instance;
      }
    }
  }
}