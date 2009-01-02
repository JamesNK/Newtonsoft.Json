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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal static class JsonTypeReflector
  {
    private static readonly Dictionary<ICustomAttributeProvider, Type> ConverterTypeCache = new Dictionary<ICustomAttributeProvider, Type>();
    private static readonly Dictionary<Type, JsonMemberMappingCollection> TypeMemberMappingsCache = new Dictionary<Type, JsonMemberMappingCollection>();
    private static readonly Dictionary<Type, JsonContainerAttribute> TypeContainerAttributeCache = new Dictionary<Type, JsonContainerAttribute>();

    public static JsonContainerAttribute GetJsonContainerAttribute(Type type)
    {
      JsonContainerAttribute containerAttribute;

      if (TypeContainerAttributeCache.TryGetValue(type, out containerAttribute))
        return containerAttribute;

      containerAttribute = ReflectionUtils.GetAttribute<JsonContainerAttribute>(type);
      TypeContainerAttributeCache[type] = containerAttribute;

      return containerAttribute;
    }

    public static MemberSerialization GetObjectMemberSerialization(Type objectType)
    {
      JsonObjectAttribute objectAttribute = GetJsonContainerAttribute(objectType) as JsonObjectAttribute;

      if (objectAttribute == null)
        return MemberSerialization.OptOut;
      
      return objectAttribute.MemberSerialization;
    }

    public static JsonMemberMappingCollection GetMemberMappings(Type objectType)
    {
      JsonMemberMappingCollection memberMappings;

      if (TypeMemberMappingsCache.TryGetValue(objectType, out memberMappings))
        return memberMappings;

      memberMappings = CreateMemberMappings(objectType);
      TypeMemberMappingsCache[objectType] = memberMappings;

      return memberMappings;
    }

    public static JsonMemberMappingCollection CreateMemberMappings(Type objectType)
    {
      MemberSerialization memberSerialization = GetObjectMemberSerialization(objectType);

      List<MemberInfo> members = GetSerializableMembers(objectType);
      if (members == null)
        throw new JsonSerializationException("Null collection of seralizable members returned.");

      JsonMemberMappingCollection memberMappings = new JsonMemberMappingCollection();

      foreach (MemberInfo member in members)
      {
        JsonPropertyAttribute propertyAttribute = ReflectionUtils.GetAttribute<JsonPropertyAttribute>(member, true);
        bool hasIgnoreAttribute = member.IsDefined(typeof(JsonIgnoreAttribute), true);

        string mappedName = (propertyAttribute != null && propertyAttribute.PropertyName != null)
         ? propertyAttribute.PropertyName
         : member.Name;

        bool required = (propertyAttribute != null) ? propertyAttribute.IsRequired : false;

        bool ignored = (hasIgnoreAttribute
          || (memberSerialization == MemberSerialization.OptIn && propertyAttribute == null));

        bool readable = ReflectionUtils.CanReadMemberValue(member);
        bool writable = ReflectionUtils.CanSetMemberValue(member);

        JsonConverter memberConverter = GetConverter(member, ReflectionUtils.GetMemberUnderlyingType(member));

        DefaultValueAttribute defaultValueAttribute = ReflectionUtils.GetAttribute<DefaultValueAttribute>(member, true);
        object defaultValue = (defaultValueAttribute != null) ? defaultValueAttribute.Value : null;

        JsonMemberMapping memberMapping = new JsonMemberMapping(mappedName, member, ignored, readable, writable, memberConverter, defaultValue, required);

        memberMappings.AddMapping(memberMapping);
      }

      return memberMappings;
    }

    public static JsonConverter GetConverter(ICustomAttributeProvider attributeProvider, Type targetConvertedType)
    {
      Type converterType;

      if (!ConverterTypeCache.TryGetValue(attributeProvider, out converterType))
      {
        JsonConverterAttribute converterAttribute = ReflectionUtils.GetAttribute<JsonConverterAttribute>(attributeProvider, true);
        converterType = (converterAttribute != null)
          ? converterAttribute.ConverterType
          : null;

        ConverterTypeCache[attributeProvider] = converterType;
      }

      if (converterType != null)
      {
        JsonConverter memberConverter = JsonConverterAttribute.CreateJsonConverterInstance(converterType);

        if (!memberConverter.CanConvert(targetConvertedType))
          throw new JsonSerializationException("JsonConverter {0} on {1} is not compatible with member type {2}.".FormatWith(CultureInfo.InvariantCulture, memberConverter.GetType().Name, attributeProvider, targetConvertedType.Name));

        return memberConverter;
      }

      return null;
    }



    public static List<MemberInfo> GetSerializableMembers(Type objectType)
    {
      return ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Public | BindingFlags.Instance);
    }
  }
}
