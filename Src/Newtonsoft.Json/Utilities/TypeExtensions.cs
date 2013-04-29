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
using System.Reflection;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Utilities
{
  internal static class TypeExtensions
  {
#if NETFX_CORE || PORTABLE
    private static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

    public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
    {
      return propertyInfo.GetGetMethod(false);
    }

    public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool nonPublic)
    {
      MethodInfo getMethod = propertyInfo.GetMethod;
      if (getMethod != null && (getMethod.IsPublic || nonPublic))
        return getMethod;

      return null;
    }

    public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
    {
      return propertyInfo.GetSetMethod(false);
    }

    public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool nonPublic)
    {
      MethodInfo setMethod = propertyInfo.SetMethod;
      if (setMethod != null && (setMethod.IsPublic || nonPublic))
        return setMethod;

      return null;
    }

    public static bool IsSubclassOf(this Type type, Type c)
    {
      return type.GetTypeInfo().IsSubclassOf(c);
    }

    public static bool IsAssignableFrom(this Type type, Type c)
    {
      return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
    }
#endif

    public static MethodInfo Method(this Delegate d)
    {
#if !(NETFX_CORE || PORTABLE)
      return d.Method;
#else
      return d.GetMethodInfo();
#endif
    }

    public static MemberTypes MemberType(this MemberInfo memberInfo)
    {
#if !(NETFX_CORE || PORTABLE || PORTABLE40)
      return memberInfo.MemberType;
#else
      if (memberInfo is PropertyInfo)
        return MemberTypes.Property;
      else if (memberInfo is FieldInfo)
        return MemberTypes.Field;
      else if (memberInfo is EventInfo)
        return MemberTypes.Event;
      else if (memberInfo is MethodInfo)
        return MemberTypes.Method;
      else
        return MemberTypes.Other;
#endif
    }

    public static bool ContainsGenericParameters(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.ContainsGenericParameters;
#else
      return type.GetTypeInfo().ContainsGenericParameters;
#endif
    }

    public static bool IsInterface(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsInterface;
#else
      return type.GetTypeInfo().IsInterface;
#endif
    }

    public static bool IsGenericType(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsGenericType;
#else
      return type.GetTypeInfo().IsGenericType;
#endif
    }

    public static bool IsGenericTypeDefinition(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsGenericTypeDefinition;
#else
      return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
    }

    public static Type BaseType(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.BaseType;
#else
      return type.GetTypeInfo().BaseType;
#endif
    }

    public static bool IsEnum(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsEnum;
#else
      return type.GetTypeInfo().IsEnum;
#endif
    }

    public static bool IsClass(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsClass;
#else
      return type.GetTypeInfo().IsClass;
#endif
    }

    public static bool IsSealed(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsSealed;
#else
      return type.GetTypeInfo().IsSealed;
#endif
    }

#if PORTABLE40
    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags, object placeholder1, Type propertyType, IList<Type> indexParameters, object placeholder2)
    {
      IList<PropertyInfo> propertyInfos = type.GetProperties(bindingFlags);

      return propertyInfos.Where(p =>
      {
        if (name != null && name != p.Name)
          return false;
        if (propertyType != null && propertyType != p.PropertyType)
          return false;
        if (indexParameters != null)
        {
          if (!p.GetIndexParameters().Select(ip => ip.ParameterType).SequenceEqual(indexParameters))
            return false;
        }

        return true;
      }).SingleOrDefault();
    }

    public static IEnumerable<MemberInfo> GetMember(this Type type, string name, MemberTypes memberType, BindingFlags bindingFlags)
    {
      return type.GetMembers(bindingFlags).Where(m =>
        {
          if (name != null && name != m.Name)
            return false;
          if (m.MemberType() != memberType)
            return false;

          return true;
        });
    }
#endif

#if (NETFX_CORE || PORTABLE)
    public static MethodInfo GetBaseDefinition(this MethodInfo method)
    {
      return method.GetRuntimeBaseDefinition();
    }
#endif

#if (NETFX_CORE || PORTABLE)
    public static bool IsDefined(this Type type, Type attributeType, bool inherit)
    {
      return type.GetTypeInfo().CustomAttributes.Any(a => a.AttributeType == attributeType);
    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
      return type.GetMethod(name, DefaultFlags);
    }

    public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().GetDeclaredMethod(name);
    }

    public static MethodInfo GetMethod(this Type type, IList<Type> parameterTypes)
    {
      return type.GetMethod(null, parameterTypes);
    }

    public static MethodInfo GetMethod(this Type type, string name, IList<Type> parameterTypes)
    {
      return type.GetMethod(name, DefaultFlags, null, parameterTypes, null);
    }

    public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags, object placeHolder1, IList<Type> parameterTypes, object placeHolder2)
    {
      return type.GetTypeInfo().DeclaredMethods.Where(m =>
      {
        if (name != null && m.Name != name)
          return false;

        if (!TestAccessibility(m, bindingFlags))
          return false;

        return m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes);
      }).SingleOrDefault();
    }

    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags, object placeholder1, Type propertyType, IList<Type> indexParameters, object placeholder2)
    {
      return type.GetTypeInfo().DeclaredProperties.Where(p =>
      {
        if (name != null && name != p.Name)
          return false;
        if (propertyType != null && propertyType != p.PropertyType)
          return false;
        if (indexParameters != null)
        {
          if (!p.GetIndexParameters().Select(ip => ip.ParameterType).SequenceEqual(indexParameters))
            return false;
        }

        return true;
      }).SingleOrDefault();
    }

    public static IEnumerable<MemberInfo> GetMember(this Type type, string name, MemberTypes memberType, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().GetMembersRecursive().Where(m =>
      {
        if (name != null && name != m.Name)
          return false;
        if (m.MemberType() != memberType)
          return false;
        if (!TestAccessibility(m, bindingFlags))
          return false;

        return true;
      });
    }

    public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
    {
      return type.GetConstructors(DefaultFlags);
    }

    public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, BindingFlags bindingFlags)
    {
      return type.GetConstructors(bindingFlags, null);
    }

    private static IEnumerable<ConstructorInfo> GetConstructors(this Type type, BindingFlags bindingFlags, IList<Type> parameterTypes)
    {
      return type.GetTypeInfo().DeclaredConstructors.Where(c =>
      {
        if (!TestAccessibility(c, bindingFlags))
          return false;

        if (parameterTypes != null && !c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes))
          return false;

        return true;
      });
    }

    public static ConstructorInfo GetConstructor(this Type type, IList<Type> parameterTypes)
    {
      return type.GetConstructor(DefaultFlags, null, parameterTypes, null);
    }

    public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingFlags, object placeholder1, IList<Type> parameterTypes, object placeholder2)
    {
      return type.GetConstructors(bindingFlags, parameterTypes).SingleOrDefault();
    }

    public static MemberInfo[] GetMember(this Type type, string member)
    {
      return type.GetMember(member, DefaultFlags);
    }

    public static MemberInfo[] GetMember(this Type type, string member, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().GetMembersRecursive().Where(m => m.Name == member && TestAccessibility(m, bindingFlags)).ToArray();
    }

    public static MemberInfo GetField(this Type type, string member)
    {
      return type.GetField(member, DefaultFlags);
    }

    public static MemberInfo GetField(this Type type, string member, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().GetDeclaredField(member);
    }

    public static IEnumerable<PropertyInfo> GetProperties(this Type type, BindingFlags bindingFlags)
    {
      IList<PropertyInfo> properties = (bindingFlags.HasFlag(BindingFlags.DeclaredOnly))
        ? type.GetTypeInfo().DeclaredProperties.ToList()
        : type.GetTypeInfo().GetPropertiesRecursive();

      return properties.Where(p => TestAccessibility(p, bindingFlags));
    }

    private static IList<MemberInfo> GetMembersRecursive(this TypeInfo type)
    {
      TypeInfo t = type;
      IList<MemberInfo> members = new List<MemberInfo>();
      while (t != null)
      {
        foreach (var member in t.DeclaredMembers)
        {
          if (!members.Any(p => p.Name == member.Name))
            members.Add(member);
        }
        t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
      }

      return members;
    }

    private static IList<PropertyInfo> GetPropertiesRecursive(this TypeInfo type)
    {
      TypeInfo t = type;
      IList<PropertyInfo> properties = new List<PropertyInfo>();
      while (t != null)
      {
        foreach (var member in t.DeclaredProperties)
        {
          if (!properties.Any(p => p.Name == member.Name))
            properties.Add(member);
        }
        t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
      }

      return properties;
    }

    private static IList<FieldInfo> GetFieldsRecursive(this TypeInfo type)
    {
      TypeInfo t = type;
      IList<FieldInfo> fields = new List<FieldInfo>();
      while (t != null)
      {
        foreach (var member in t.DeclaredFields)
        {
          if (!fields.Any(p => p.Name == member.Name))
            fields.Add(member);
        }
        t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
      }

      return fields;
    }

    public static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().DeclaredMethods;
    }

    public static PropertyInfo GetProperty(this Type type, string name)
    {
      return type.GetProperty(name, DefaultFlags);
    }

    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags)
    {
      return type.GetTypeInfo().GetDeclaredProperty(name);
    }

    public static IEnumerable<FieldInfo> GetFields(this Type type)
    {
      return type.GetFields(DefaultFlags);
    }

    public static IEnumerable<FieldInfo> GetFields(this Type type, BindingFlags bindingFlags)
    {
      IList<FieldInfo> fields = (bindingFlags.HasFlag(BindingFlags.DeclaredOnly))
        ? type.GetTypeInfo().DeclaredFields.ToList()
        : type.GetTypeInfo().GetFieldsRecursive();

      return fields.Where(f => TestAccessibility(f, bindingFlags)).ToList();
    }

    private static bool TestAccessibility(PropertyInfo member, BindingFlags bindingFlags)
    {
      if (member.GetMethod != null && TestAccessibility(member.GetMethod, bindingFlags))
        return true;

      if (member.SetMethod != null && TestAccessibility(member.SetMethod, bindingFlags))
        return true;

      return false;
    }

    private static bool TestAccessibility(MemberInfo member, BindingFlags bindingFlags)
    {
      if (member is FieldInfo)
      {
        return TestAccessibility((FieldInfo)member, bindingFlags);
      }
      else if (member is MethodBase)
      {
        return TestAccessibility((MethodBase)member, bindingFlags);
      }
      else if (member is PropertyInfo)
      {
        return TestAccessibility((PropertyInfo)member, bindingFlags);
      }

      throw new Exception("Unexpected member type.");
    }

    private static bool TestAccessibility(FieldInfo member, BindingFlags bindingFlags)
    {
      bool visibility = (member.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) ||
        (!member.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic));

      bool instance = (member.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) ||
        (!member.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance));

      return visibility && instance;
    }

    private static bool TestAccessibility(MethodBase member, BindingFlags bindingFlags)
    {
      bool visibility = (member.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) ||
        (!member.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic));

      bool instance = (member.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) ||
        (!member.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance));

      return visibility && instance;
    }

    public static Type[] GetGenericArguments(this Type type)
    {
      return type.GetTypeInfo().GenericTypeArguments;
    }

    public static IEnumerable<Type> GetInterfaces(this Type type)
    {
      return type.GetTypeInfo().ImplementedInterfaces;
    }

    public static IEnumerable<MethodInfo> GetMethods(this Type type)
    {
      return type.GetTypeInfo().DeclaredMethods;
    }
#endif

    public static bool IsAbstract(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsAbstract;
#else
      return type.GetTypeInfo().IsAbstract;
#endif
    }

    public static bool IsVisible(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsVisible;
#else
      return type.GetTypeInfo().IsVisible;
#endif
    }

    public static bool IsValueType(this Type type)
    {
#if !(NETFX_CORE || PORTABLE)
      return type.IsValueType;
#else
      return type.GetTypeInfo().IsValueType;
#endif
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

        current = current.BaseType();
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

    public static MethodInfo GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
    {
      var methods = type.GetMethods().Where(method => method.Name == name);

      foreach (var method in methods)
      {
        if (method.HasParameters(parameterTypes))
          return method;
      }

      return null;
    }

    public static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
    {
      var methodParameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

      if (methodParameters.Length != parameterTypes.Length)
        return false;

      for (int i = 0; i < methodParameters.Length; i++)
        if (methodParameters[i].ToString() != parameterTypes[i].ToString())
          return false;

      return true;
    }

    public static IEnumerable<Type> GetAllInterfaces(this Type target)
    {
      foreach (var i in target.GetInterfaces())
      {
        yield return i;
        foreach (var ci in i.GetInterfaces())
        {
          yield return ci;
        }
      }
    }

    public static IEnumerable<MethodInfo> GetAllMethods(this Type target)
    {
      var allTypes = target.GetAllInterfaces().ToList();
      allTypes.Add(target);

      return from type in allTypes
             from method in type.GetMethods()
             select method;
    }
  }
}