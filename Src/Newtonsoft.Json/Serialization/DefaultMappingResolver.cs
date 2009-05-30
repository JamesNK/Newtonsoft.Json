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
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Resolves member mappings for a type.
  /// </summary>
  public class DefaultMappingResolver : IMappingResolver
  {
    internal static readonly IMappingResolver Instance = new DefaultMappingResolver();

    private readonly Dictionary<Type, JsonMemberMappingCollection> _typeMemberMappingsCache = new Dictionary<Type, JsonMemberMappingCollection>();

    /// <summary>
    /// Gets or sets the default members search flags.
    /// </summary>
    /// <value>The default members search flags.</value>
    public BindingFlags DefaultMembersSearchFlags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMappingResolver"/> class.
    /// </summary>
    public DefaultMappingResolver()
    {
      DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.Instance;
    }

    /// <summary>
    /// Returns a collection of <see cref="JsonMemberMapping"/> for the specified <see cref="Type"/>
    /// to be used during serialization.
    /// </summary>
    /// <param name="type">The type to be resolved.</param>
    /// <returns>A collection of <see cref="JsonMemberMapping"/> to be used during serialization.</returns>
    public virtual JsonMemberMappingCollection ResolveMappings(Type type)
    {
      JsonMemberMappingCollection memberMappings;

      if (_typeMemberMappingsCache.TryGetValue(type, out memberMappings))
        return memberMappings;

      // double check locking to avoid threading issues
      lock (_typeMemberMappingsCache)
      {
        if (_typeMemberMappingsCache.TryGetValue(type, out memberMappings))
          return memberMappings;

        memberMappings = CreateMemberMappings(type);
        _typeMemberMappingsCache[type] = memberMappings;

        return memberMappings;
      }
    }

    /// <summary>
    /// Gets the serializable members for the type.
    /// </summary>
    /// <param name="objectType">The type to get serializable members for.</param>
    /// <returns>The serializable members for the type.</returns>
    protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
    {
#if !PocketPC
      DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
#endif
      
      List<MemberInfo> defaultMembers = ReflectionUtils.GetFieldsAndProperties(objectType, DefaultMembersSearchFlags);
      List<MemberInfo> allMembers = ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

      List<MemberInfo> serializableMembers = new List<MemberInfo>();
      foreach (MemberInfo member in allMembers)
      {
        if (defaultMembers.Contains(member))
        {
          serializableMembers.Add(member);
        }
        else
        {
          if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member) != null)
            serializableMembers.Add(member);
#if !PocketPC
          else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(member) != null)
            serializableMembers.Add(member);
#endif
        }
      }

      return serializableMembers;
    }

    private JsonMemberMappingCollection CreateMemberMappings(Type objectType)
    {
      MemberSerialization memberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType);

      List<MemberInfo> members = GetSerializableMembers(objectType);
      if (members == null)
        throw new JsonSerializationException("Null collection of seralizable members returned.");

      JsonMemberMappingCollection memberMappings = new JsonMemberMappingCollection();

      foreach (MemberInfo member in members)
      {
        JsonMemberMapping memberMapping = CreateMemberMapping(memberSerialization, member);

        if (memberMapping != null)
          memberMappings.AddMapping(memberMapping);
      }

      return memberMappings;
    }

    /// <summary>
    /// Creates a <see cref="JsonMemberMapping"/> for the given <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberSerialization">The member's declaring type member serialization mode.</param>
    /// <param name="member">The member to create a <see cref="JsonMemberMapping"/> for.</param>
    /// <returns>A created <see cref="JsonMemberMapping"/> for the given <see cref="MemberInfo"/>.</returns>
    protected virtual JsonMemberMapping CreateMemberMapping(MemberSerialization memberSerialization, MemberInfo member)
    {
      JsonObjectAttribute jsonObjectAttribute = JsonTypeReflector.GetJsonContainerAttribute(member.DeclaringType) as JsonObjectAttribute;

#if !PocketPC
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
#if !PocketPC
      else if (dataMemberAttribute != null && dataMemberAttribute.Name != null)
        mappedName = dataMemberAttribute.Name;
#endif
      else
        mappedName = member.Name;

      string resolvedMappedName = ResolvePropertyName(mappedName);

      bool required;
      if (propertyAttribute != null)
        required = propertyAttribute.IsRequired;
#if !PocketPC
      else if (dataMemberAttribute != null)
        required = dataMemberAttribute.IsRequired;
#endif
      else
        required = false;

      bool ignored = (hasIgnoreAttribute ||
                      (memberSerialization == MemberSerialization.OptIn
                       && propertyAttribute == null
#if !PocketPC
                       && dataMemberAttribute == null
#endif
                       ));

      bool readable = ReflectionUtils.CanReadMemberValue(member);
      bool writable = ReflectionUtils.CanSetMemberValue(member);

      JsonConverter memberConverter = JsonTypeReflector.GetConverter(member, ReflectionUtils.GetMemberUnderlyingType(member));

      DefaultValueAttribute defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(member);
      object defaultValue = (defaultValueAttribute != null) ? defaultValueAttribute.Value : null;

      NullValueHandling? nullValueHandling = (propertyAttribute != null) ? propertyAttribute._nullValueHandling : null;
      DefaultValueHandling? defaultValueHandling = (propertyAttribute != null) ? propertyAttribute._defaultValueHandling : null;
      ReferenceLoopHandling? referenceLoopHandling = (propertyAttribute != null) ? propertyAttribute._referenceLoopHandling : null;
      
      bool isReference;
      if (jsonObjectAttribute != null)
        isReference = jsonObjectAttribute.IsReference;
      else
        isReference = false;

      return new JsonMemberMapping(resolvedMappedName, member, ignored, readable, writable, memberConverter, defaultValue, required, isReference, nullValueHandling, defaultValueHandling, referenceLoopHandling);
    }

    /// <summary>
    /// Resolves the name of the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns></returns>
    protected virtual string ResolvePropertyName(string propertyName)
    {
      return propertyName;
    }
  }
}