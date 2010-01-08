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
using System.Text;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal delegate object MemberHandler<T>(T target, params object[] args);

  internal static class ReflectionUtils
  {
    public static Type GetObjectType(object v)
    {
      return (v != null) ? v.GetType() : null;
    }

    public static bool IsInstantiatableType(Type t)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

      if (t.IsAbstract || t.IsInterface || t.IsArray || t.IsGenericTypeDefinition || t == typeof(void))
        return false;

      if (!HasDefaultConstructor(t))
        return false;

      return true;
    }

    public static bool HasDefaultConstructor(Type t)
    {
      return HasDefaultConstructor(t, false);
    }

    public static bool HasDefaultConstructor(Type t, bool nonPublic)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

      if (t.IsValueType)
        return true;

      return (GetDefaultConstructor(t, nonPublic) != null);
    }

    public static ConstructorInfo GetDefaultConstructor(Type t)
    {
      return GetDefaultConstructor(t, false);
    }

    public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
    {
      BindingFlags accessModifier = BindingFlags.Public;
      
      if (nonPublic)
        accessModifier = accessModifier | BindingFlags.NonPublic;

      return t.GetConstructor(accessModifier | BindingFlags.Instance, null, new Type[0], null);
    }

    public static bool IsNullable(Type t)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

      if (t.IsValueType)
        return IsNullableType(t);

      return true;
    }

    public static bool IsNullableType(Type t)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

      return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    //public static bool IsValueTypeUnitializedValue(ValueType value)
    //{
    //  if (value == null)
    //    return true;

    //  return value.Equals(CreateUnitializedValue(value.GetType()));
    //}

    public static bool IsUnitializedValue(object value)
    {
      if (value == null)
      {
        return true;
      }
      else
      {
        object unitializedValue = CreateUnitializedValue(value.GetType());
        return value.Equals(unitializedValue);
      }
    }

    public static object CreateUnitializedValue(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

      if (type.IsGenericTypeDefinition)
        throw new ArgumentException("Type {0} is a generic type definition and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, type), "type");

      if (type.IsClass || type.IsInterface || type == typeof(void))
        return null;
      else if (type.IsValueType)
        return Activator.CreateInstance(type);
      else
        throw new ArgumentException("Type {0} cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, type), "type");
    }

    public static bool IsPropertyIndexed(PropertyInfo property)
    {
      ValidationUtils.ArgumentNotNull(property, "property");

      return !CollectionUtils.IsNullOrEmpty<ParameterInfo>(property.GetIndexParameters());
    }

    public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
    {
      Type implementingType;
      return ImplementsGenericDefinition(type, genericInterfaceDefinition, out implementingType);
    }

    public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
    {
      ValidationUtils.ArgumentNotNull(type, "type");
      ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, "genericInterfaceDefinition");

      if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
        throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, genericInterfaceDefinition));

      if (type.IsInterface)
      {
        if (type.IsGenericType)
        {
          Type interfaceDefinition = type.GetGenericTypeDefinition();

          if (genericInterfaceDefinition == interfaceDefinition)
          {
            implementingType = type;
            return true;
          }
        }
      }

      foreach (Type i in type.GetInterfaces())
      {
        if (i.IsGenericType)
        {
          Type interfaceDefinition = i.GetGenericTypeDefinition();

          if (genericInterfaceDefinition == interfaceDefinition)
          {
            implementingType = i;
            return true;
          }
        }
      }

      implementingType = null;
      return false;
    }

    public static bool AssignableToTypeName(this Type type, string fullTypeName, out Type match)
    {
      Type current = type;

      while (current != null)
      {
        if (string.Equals(current.FullName, fullTypeName, StringComparison.Ordinal))
        {
          match = current;
          return true;
        }

        current = current.BaseType;
      }

      foreach (Type i in type.GetInterfaces())
      {
        if (string.Equals(i.Name, fullTypeName, StringComparison.Ordinal))
        {
          match = type;
          return true;
        }
      }

      match = null;
      return false;
    }

    public static bool AssignableToTypeName(this Type type, string fullTypeName)
    {
      Type match;
      return type.AssignableToTypeName(fullTypeName, out match);
    }

    public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
    {
      Type implementingType;
      return InheritsGenericDefinition(type, genericClassDefinition, out implementingType);
    }

    public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type implementingType)
    {
      ValidationUtils.ArgumentNotNull(type, "type");
      ValidationUtils.ArgumentNotNull(genericClassDefinition, "genericClassDefinition");

      if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
        throw new ArgumentNullException("'{0}' is not a generic class definition.".FormatWith(CultureInfo.InvariantCulture, genericClassDefinition));

      return InheritsGenericDefinitionInternal(type, type, genericClassDefinition, out implementingType);
    }

    private static bool InheritsGenericDefinitionInternal(Type initialType, Type currentType, Type genericClassDefinition, out Type implementingType)
    {
      if (currentType.IsGenericType)
      {
        Type currentGenericClassDefinition = currentType.GetGenericTypeDefinition();

        if (genericClassDefinition == currentGenericClassDefinition)
        {
          implementingType = currentType;
          return true;
        }
      }

      if (currentType.BaseType == null)
      {
        implementingType = null;
        return false;
      }

      return InheritsGenericDefinitionInternal(initialType, currentType.BaseType, genericClassDefinition, out implementingType);
    }

    /// <summary>
    /// Gets the type of the typed collection's items.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type of the typed collection's items.</returns>
    public static Type GetCollectionItemType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");
      Type genericListType;

      if (type.IsArray)
      {
        return type.GetElementType();
      }
      else if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out genericListType))
      {
        if (genericListType.IsGenericTypeDefinition)
          throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));

        return genericListType.GetGenericArguments()[0];
      }
      else if (typeof(IEnumerable).IsAssignableFrom(type))
      {
        return null;
      }
      else
      {
        throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
      }
    }

    public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
    {
      ValidationUtils.ArgumentNotNull(dictionaryType, "type");

      Type genericDictionaryType;
      if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out genericDictionaryType))
      {
        if (genericDictionaryType.IsGenericTypeDefinition)
          throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));

        Type[] dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

        keyType = dictionaryGenericArguments[0];
        valueType = dictionaryGenericArguments[1];
        return;
      }
      else if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
      {
        keyType = null;
        valueType = null;
        return;
      }
      else
      {
        throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
      }
    }

    public static Type GetDictionaryValueType(Type dictionaryType)
    {
      Type keyType;
      Type valueType;
      GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

      return valueType;
    }

    public static Type GetDictionaryKeyType(Type dictionaryType)
    {
      Type keyType;
      Type valueType;
      GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

      return keyType;
    }

    /// <summary>
    /// Tests whether the list's items are their unitialized value.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>Whether the list's items are their unitialized value</returns>
    public static bool ItemsUnitializedValue<T>(IList<T> list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      Type elementType = GetCollectionItemType(list.GetType());

      if (elementType.IsValueType)
      {
        object unitializedValue = CreateUnitializedValue(elementType);

        for (int i = 0; i < list.Count; i++)
        {
          if (!list[i].Equals(unitializedValue))
            return false;
        }
      }
      else if (elementType.IsClass)
      {
        for (int i = 0; i < list.Count; i++)
        {
          object value = list[i];

          if (value != null)
            return false;
        }
      }
      else
      {
        throw new Exception("Type {0} is neither a ValueType or a Class.".FormatWith(CultureInfo.InvariantCulture, elementType));
      }

      return true;
    }

    /// <summary>
    /// Gets the member's underlying type.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>The underlying type of the member.</returns>
    public static Type GetMemberUnderlyingType(MemberInfo member)
    {
      ValidationUtils.ArgumentNotNull(member, "member");

      switch (member.MemberType)
      {
        case MemberTypes.Field:
          return ((FieldInfo)member).FieldType;
        case MemberTypes.Property:
          return ((PropertyInfo)member).PropertyType;
        case MemberTypes.Event:
          return ((EventInfo)member).EventHandlerType;
        default:
          throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member");
      }
    }

    /// <summary>
    /// Determines whether the member is an indexed property.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>
    /// 	<c>true</c> if the member is an indexed property; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsIndexedProperty(MemberInfo member)
    {
      ValidationUtils.ArgumentNotNull(member, "member");

      PropertyInfo propertyInfo = member as PropertyInfo;

      if (propertyInfo != null)
        return IsIndexedProperty(propertyInfo);
      else
        return false;
    }

    /// <summary>
    /// Determines whether the property is an indexed property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    /// 	<c>true</c> if the property is an indexed property; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsIndexedProperty(PropertyInfo property)
    {
      ValidationUtils.ArgumentNotNull(property, "property");

      return (property.GetIndexParameters().Length > 0);
    }

    /// <summary>
    /// Gets the member's value on the object.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The member's value on the object.</returns>
    public static object GetMemberValue(MemberInfo member, object target)
    {
      ValidationUtils.ArgumentNotNull(member, "member");
      ValidationUtils.ArgumentNotNull(target, "target");

      switch (member.MemberType)
      {
        case MemberTypes.Field:
          return ((FieldInfo)member).GetValue(target);
        case MemberTypes.Property:
          try
          {
            return ((PropertyInfo)member).GetValue(target, null);
          }
          catch (TargetParameterCountException e)
          {
            throw new ArgumentException("MemberInfo '{0}' has index parameters".FormatWith(CultureInfo.InvariantCulture, member.Name), e);
          }
        default:
          throw new ArgumentException("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, CultureInfo.InvariantCulture, member.Name), "member");
      }
    }

    /// <summary>
    /// Sets the member's value on the target object.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    public static void SetMemberValue(MemberInfo member, object target, object value)
    {
      ValidationUtils.ArgumentNotNull(member, "member");
      ValidationUtils.ArgumentNotNull(target, "target");

      switch (member.MemberType)
      {
        case MemberTypes.Field:
          ((FieldInfo)member).SetValue(target, value);
          break;
        case MemberTypes.Property:
          ((PropertyInfo)member).SetValue(target, value, null);
          break;
        default:
          throw new ArgumentException("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), "member");
      }
    }

    /// <summary>
    /// Determines whether the specified MemberInfo can be read.
    /// </summary>
    /// <param name="member">The MemberInfo to determine whether can be read.</param>
    /// <returns>
    /// 	<c>true</c> if the specified MemberInfo can be read; otherwise, <c>false</c>.
    /// </returns>
    public static bool CanReadMemberValue(MemberInfo member)
    {
      switch (member.MemberType)
      {
        case MemberTypes.Field:
          return true;
        case MemberTypes.Property:
          return ((PropertyInfo)member).CanRead;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the specified MemberInfo can be set.
    /// </summary>
    /// <param name="member">The MemberInfo to determine whether can be set.</param>
    /// <returns>
    /// 	<c>true</c> if the specified MemberInfo can be set; otherwise, <c>false</c>.
    /// </returns>
    public static bool CanSetMemberValue(MemberInfo member)
    {
      switch (member.MemberType)
      {
        case MemberTypes.Field:
          return !((FieldInfo)member).IsInitOnly;
        case MemberTypes.Property:
          return ((PropertyInfo)member).CanWrite;
        default:
          return false;
      }
    }

    public static List<MemberInfo> GetFieldsAndProperties<T>(BindingFlags bindingAttr)
    {
      return GetFieldsAndProperties(typeof(T), bindingAttr);
    }

    public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
    {
      List<MemberInfo> targetMembers = new List<MemberInfo>();

      targetMembers.AddRange(type.GetFields(bindingAttr));
      targetMembers.AddRange(type.GetProperties(bindingAttr));

      // for some reason .NET returns multiple members when overriding a generic member on a base class
      // http://forums.msdn.microsoft.com/en-US/netfxbcl/thread/b5abbfee-e292-4a64-8907-4e3f0fb90cd9/
      // filter members to only return the override on the topmost class
      List<MemberInfo> distinctMembers = new List<MemberInfo>(targetMembers.Count);

      var groupedMembers = targetMembers.GroupBy(m => m.Name).Select(g => new { Count = g.Count(), Members = g.Cast<MemberInfo>() });
      foreach (var groupedMember in groupedMembers)
      {
        if (groupedMember.Count == 1)
        {
          distinctMembers.Add(groupedMember.Members.First());
        }
        else
        {
          var members = groupedMember.Members.Where(m => !IsOverridenGenericMember(m, bindingAttr) || m.Name == "Item");

          distinctMembers.AddRange(members);
        }
      }

      return distinctMembers;
    }

    private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
    {
      if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
        throw new ArgumentException("Member must be a field or property.");

      Type declaringType = memberInfo.DeclaringType;
      if (!declaringType.IsGenericType)
        return false;
      Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
      if (genericTypeDefinition == null)
        return false;
      MemberInfo[] members = genericTypeDefinition.GetMember(memberInfo.Name, bindingAttr);
      if (members.Length == 0)
        return false;
      Type memberUnderlyingType = GetMemberUnderlyingType(members[0]);
      if (!memberUnderlyingType.IsGenericParameter)
        return false;

      return true;
    }

    public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
    {
      return GetAttribute<T>(attributeProvider, true);
    }

    public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
    {
      T[] attributes = GetAttributes<T>(attributeProvider, inherit);

      return CollectionUtils.GetSingleItem(attributes, true);
    }

    public static T[] GetAttributes<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
    {
      ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");

      return (T[])attributeProvider.GetCustomAttributes(typeof(T), inherit);
    }

    public static string GetNameAndAssessmblyName(Type t)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

      return t.FullName + ", " + t.Assembly.GetName().Name;
    }

    public static Type MakeGenericType(Type genericTypeDefinition, params Type[] innerTypes)
    {
      ValidationUtils.ArgumentNotNull(genericTypeDefinition, "genericTypeDefinition");
      ValidationUtils.ArgumentNotNullOrEmpty<Type>(innerTypes, "innerTypes");
      ValidationUtils.ArgumentConditionTrue(genericTypeDefinition.IsGenericTypeDefinition, "genericTypeDefinition", "Type {0} is not a generic type definition.".FormatWith(CultureInfo.InvariantCulture, genericTypeDefinition));

      return genericTypeDefinition.MakeGenericType(innerTypes);
    }

    public static object CreateGeneric(Type genericTypeDefinition, Type innerType, params object[] args)
    {
      return CreateGeneric(genericTypeDefinition, new Type[] { innerType }, args);
    }

    public static object CreateGeneric(Type genericTypeDefinition, IList<Type> innerTypes, params object[] args)
    {
      return CreateGeneric(genericTypeDefinition, innerTypes, (t, a) => ReflectionUtils.CreateInstance(t, a.ToArray()), args);
    }

    public static object CreateGeneric(Type genericTypeDefinition, IList<Type> innerTypes, Func<Type, IList<object>, object> instanceCreator, params object[] args)
    {
      ValidationUtils.ArgumentNotNull(genericTypeDefinition, "genericTypeDefinition");
      ValidationUtils.ArgumentNotNullOrEmpty(innerTypes, "innerTypes");
      ValidationUtils.ArgumentNotNull(instanceCreator, "createInstance");

      Type specificType = MakeGenericType(genericTypeDefinition, innerTypes.ToArray());

      return instanceCreator(specificType, args);
    }

     public static bool IsCompatibleValue(object value, Type type)
     {
       if (value == null && IsNullable(type))
         return true;

       if (type.IsAssignableFrom(value.GetType()))
         return true;

       return false;
     }

     public static object CreateInstance(Type type, params object[] args)
     {
       ValidationUtils.ArgumentNotNull(type, "type");

#if !PocketPC
       return Activator.CreateInstance(type, args);
#else
       if (type.IsValueType)
         return Activator.CreateInstance(type);

       ConstructorInfo[] constructors = type.GetConstructors();
       ConstructorInfo match = constructors.Where(c =>
         {
           ParameterInfo[] parameters = c.GetParameters();
           if (parameters.Length != args.Length)
             return false;

           for (int i = 0; i < parameters.Length; i++)
           {
             ParameterInfo parameter = parameters[i];
             object value = args[i];

             if (!IsCompatibleValue(value, parameter.ParameterType))
               return false;
           }

           return true;
         }).FirstOrDefault();

       if (match == null)
         throw new Exception("Could not create '{0}' with given parameters.".FormatWith(CultureInfo.InvariantCulture, type));

       return match.Invoke(args);
#endif
     }

    public static void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName)
    {
      int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

      if (assemblyDelimiterIndex != null)
      {
        typeName = fullyQualifiedTypeName.Substring(0, assemblyDelimiterIndex.Value).Trim();
        assemblyName = fullyQualifiedTypeName.Substring(assemblyDelimiterIndex.Value + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.Value - 1).Trim();
      }
      else
      {
        typeName = fullyQualifiedTypeName;
        assemblyName = null;
      }

    }

    private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
    {
      // we need to get the first comma following all surrounded in brackets because of generic types
      // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      int scope = 0;
      for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
      {
        char current = fullyQualifiedTypeName[i];
        switch (current)
        {
          case '[':
            scope++;
            break;
          case ']':
            scope--;
            break;
          case ',':
            if (scope == 0)
              return i;
            break;
        }
      }

      return null;
    }
  }
}