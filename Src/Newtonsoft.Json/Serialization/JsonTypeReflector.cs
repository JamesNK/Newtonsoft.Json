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
using System.Collections.Generic;
#if !SILVERLIGHT && !PocketPC
using System.ComponentModel.DataAnnotations;
#endif
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal static class JsonTypeReflector
  {
    public const string IdPropertyName = "$id";
    public const string RefPropertyName = "$ref";
    public const string TypePropertyName = "$type";
    public const string ArrayValuesPropertyName = "$values";

    private static readonly ThreadSafeStore<ICustomAttributeProvider, Type> ConverterTypeCache = new ThreadSafeStore<ICustomAttributeProvider, Type>(GetConverterTypeFromAttribute);
#if !SILVERLIGHT && !PocketPC
    private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(GetAssociateMetadataTypeFromAttribute);
#endif

    public static JsonContainerAttribute GetJsonContainerAttribute(Type type)
    {
      return CachedAttributeGetter<JsonContainerAttribute>.GetAttribute(type);
    }

    public static JsonObjectAttribute GetJsonObjectAttribute(Type type)
    {
      return GetJsonContainerAttribute(type) as JsonObjectAttribute;
    }

#if !PocketPC
    public static DataContractAttribute GetDataContractAttribute(Type type)
    {
      return CachedAttributeGetter<DataContractAttribute>.GetAttribute(type);
    }
#endif

    public static MemberSerialization GetObjectMemberSerialization(Type objectType)
    {
      JsonObjectAttribute objectAttribute = GetJsonObjectAttribute(objectType);

      if (objectAttribute == null)
      {
#if !PocketPC
        DataContractAttribute dataContractAttribute = GetDataContractAttribute(objectType);

        if (dataContractAttribute != null)
          return MemberSerialization.OptIn;
#endif
        
        return MemberSerialization.OptOut;
      }

      return objectAttribute.MemberSerialization;
    }

    private static Type GetConverterType(ICustomAttributeProvider attributeProvider)
    {
      return ConverterTypeCache.Get(attributeProvider);
    }

    private static Type GetConverterTypeFromAttribute(ICustomAttributeProvider attributeProvider)
    {
      JsonConverterAttribute converterAttribute = GetAttribute<JsonConverterAttribute>(attributeProvider);
      return (converterAttribute != null)
        ? converterAttribute.ConverterType
        : null;
    }

    public static JsonConverter GetConverter(ICustomAttributeProvider attributeProvider, Type targetConvertedType)
    {
      Type converterType = GetConverterType(attributeProvider);

      if (converterType != null)
      {
        JsonConverter memberConverter = JsonConverterAttribute.CreateJsonConverterInstance(converterType);

        if (!memberConverter.CanConvert(targetConvertedType))
          throw new JsonSerializationException("JsonConverter {0} on {1} is not compatible with member type {2}.".FormatWith(CultureInfo.InvariantCulture, memberConverter.GetType().Name, attributeProvider, targetConvertedType.Name));

        return memberConverter;
      }

      return null;
    }

#if !SILVERLIGHT && !PocketPC
    private static Type GetAssociatedMetadataType(Type type)
    {
      return AssociatedMetadataTypesCache.Get(type);
    }

    private static Type GetAssociateMetadataTypeFromAttribute(Type type)
    {
      MetadataTypeAttribute metadataTypeAttribute = ReflectionUtils.GetAttribute<MetadataTypeAttribute>(type, true);

      return (metadataTypeAttribute != null) ? metadataTypeAttribute.MetadataClassType : null;
    }

    private static T GetAttribute<T>(Type type) where T : Attribute
    {
      Type metadataType = GetAssociatedMetadataType(type);
      if (metadataType != null)
        return ReflectionUtils.GetAttribute<T>(metadataType, true);

      return ReflectionUtils.GetAttribute<T>(type, true);
    }

    private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
    {
      Type metadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
      if (metadataType != null)
      {
        MemberInfo metadataTypeMemberInfo = metadataType.GetMember(memberInfo.Name,
          memberInfo.MemberType,
          BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).SingleOrDefault();

        if (metadataTypeMemberInfo != null)
          return ReflectionUtils.GetAttribute<T>(metadataTypeMemberInfo, true);
      }

      return ReflectionUtils.GetAttribute<T>(memberInfo, true);
    }

    public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
    {
      Type type = attributeProvider as Type;
      if (type != null)
        return GetAttribute<T>(type);

      MemberInfo memberInfo = attributeProvider as MemberInfo;
      if (memberInfo != null)
        return GetAttribute<T>(memberInfo);

      return ReflectionUtils.GetAttribute<T>(attributeProvider, true);
    }
#else
    public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
    {
      return ReflectionUtils.GetAttribute<T>(attributeProvider, true);
    }
#endif


  }
}