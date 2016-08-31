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
#if DOTNET || PORTABLE
#if !DOTNET
        private static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod(false);
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool nonPublic)
        {
            MethodInfo getMethod = propertyInfo.GetMethod;
            if (getMethod != null && (getMethod.IsPublic || nonPublic))
            {
                return getMethod;
            }

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
            {
                return setMethod;
            }

            return null;
        }
#endif

        public static bool IsSubclassOf(this Type type, Type c)
        {
            return type.GetTypeInfo().IsSubclassOf(c);
        }

#if !DOTNET
        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }
#endif

        public static bool IsInstanceOfType(this Type type, object o)
        {
            if (o == null)
            {
                return false;
            }

            return type.IsAssignableFrom(o.GetType());
        }
#endif

        public static MethodInfo Method(this Delegate d)
        {
#if !(DOTNET || PORTABLE)
            return d.Method;
#else
            return d.GetMethodInfo();
#endif
        }

        public static MemberTypes MemberType(this MemberInfo memberInfo)
        {
#if !(DOTNET || PORTABLE || PORTABLE40)
            return memberInfo.MemberType;
#else
            if (memberInfo is PropertyInfo)
            {
                return MemberTypes.Property;
            }
            else if (memberInfo is FieldInfo)
            {
                return MemberTypes.Field;
            }
            else if (memberInfo is EventInfo)
            {
                return MemberTypes.Event;
            }
            else if (memberInfo is MethodInfo)
            {
                return MemberTypes.Method;
            }
            else
            {
                return MemberTypes.Other;
            }
#endif
        }

        public static bool ContainsGenericParameters(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.ContainsGenericParameters;
#else
            return type.GetTypeInfo().ContainsGenericParameters;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsGenericTypeDefinition;
#else
            return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }

        public static Assembly Assembly(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsClass;
#else
            return type.GetTypeInfo().IsClass;
#endif
        }

        public static bool IsSealed(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsSealed;
#else
            return type.GetTypeInfo().IsSealed;
#endif
        }

#if (PORTABLE40 || DOTNET || PORTABLE)
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags, object placeholder1, Type propertyType, IList<Type> indexParameters, object placeholder2)
        {
            IEnumerable<PropertyInfo> propertyInfos = type.GetProperties(bindingFlags);

            return propertyInfos.Where(p =>
            {
                if (name != null && name != p.Name)
                {
                    return false;
                }
                if (propertyType != null && propertyType != p.PropertyType)
                {
                    return false;
                }
                if (indexParameters != null)
                {
                    if (!p.GetIndexParameters().Select(ip => ip.ParameterType).SequenceEqual(indexParameters))
                    {
                        return false;
                    }
                }

                return true;
            }).SingleOrDefault();
        }

        public static IEnumerable<MemberInfo> GetMember(this Type type, string name, MemberTypes memberType, BindingFlags bindingFlags)
        {
#if PORTABLE
            return type.GetMemberInternal(name, memberType, bindingFlags);
#else
            return type.GetMember(name, bindingFlags).Where(m =>
            {
                if (m.MemberType() != memberType)
                {
                    return false;
                }

                return true;
            });
#endif
        }
#endif

#if (DOTNET || PORTABLE)
        public static MethodInfo GetBaseDefinition(this MethodInfo method)
        {
            return method.GetRuntimeBaseDefinition();
        }
#endif

#if (DOTNET || PORTABLE)
        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().CustomAttributes.Any(a => a.AttributeType == attributeType);
        }

#if !DOTNET
        
        private static readonly Type[] PrimitiveTypes = new Type[] 
        {
            typeof(bool),  typeof(char),   typeof(sbyte), typeof(byte),
            typeof(short), typeof(ushort), typeof(int),   typeof(uint),
            typeof(long),  typeof(ulong),  typeof(float), typeof(double)
        };

        private static readonly int[] WideningMasks = new int[]
        {
            0x0001,        0x0FE2,         0x0D54,        0x0FFA,
            0x0D50,        0x0FE2,         0x0D40,        0x0F80,
            0x0D00,        0x0E00,         0x0C00,        0x0800
        };
        
        internal static bool CanConvertPrimitive(Type from, Type to)
        {
            if (from == to) return true;

            int fromMask = 0, toMask = 0;

            for (int i = 0; i < PrimitiveTypes.Length && (fromMask == 0 || toMask == 0); i++)
            {
                if (PrimitiveTypes[i] == from)
                {
                    fromMask = WideningMasks[i];
                }
                else if (PrimitiveTypes[i] == to)
                {
                    toMask = 1 << i;
                }
            }
            
            return (fromMask & toMask) != 0;
        }

        private static bool FilterParameters(ParameterInfo[] parameters, IList<Type> types, bool enableParamArray)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            if (parameters.Length == 0)
            {
                // fast check for parameterless methods
                return types.Count == 0;
            }
            
            if (parameters.Length > types.Count)
            {
                // not all declared parameters were specified (optional parameters are not supported)
                return false;
            }

            // check if the last parameter is ParamArray
            Type paramArrayType = null;
            
            if (enableParamArray &&
                parameters[parameters.Length - 1].ParameterType.IsArray &&
                parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute)))
            {
                paramArrayType = parameters[parameters.Length - 1].ParameterType.GetElementType();
            }

            if (paramArrayType == null && parameters.Length != types.Count)
            {
                // when there's no ParamArray, number of parameters should match
                return false;
            }
            
            for (int i = 0; i < types.Count; i++)
            {
                var paramType = (paramArrayType != null && i >= parameters.Length - 1) ? paramArrayType : parameters[i].ParameterType;

                // exact match with provided type
                if (paramType == types[i]) continue;

                // parameter of type object matches anything
                if (paramType == typeof(object)) continue;

                if (paramType.IsPrimitive())
                {
                    // primitive parameter can only be assigned from compatible primitive type
                    if (!types[i].IsPrimitive() || !CanConvertPrimitive(types[i], paramType))
                        return false;
                }
                else
                {
                    if (!paramType.IsAssignableFrom(types[i]))
                        return false;
                }
            }

            return true;
        }

        private class ParametersMatchComparer : IComparer<ParameterInfo[]>
        {
            private readonly IList<Type> _types;
            private readonly bool _enableParamArray;

            public ParametersMatchComparer(IList<Type> types, bool enableParamArray)
            {
                if (types == null)
                    throw new ArgumentNullException(nameof(types));

                _types = types;
                _enableParamArray = enableParamArray;
            }

            public int Compare(ParameterInfo[] parameters1, ParameterInfo[] parameters2)
            {
                if (parameters1 == null)
                    throw new ArgumentNullException(nameof(parameters1));
                if (parameters2 == null)
                    throw new ArgumentNullException(nameof(parameters2));

                // parameterless method wins
                if (parameters1.Length == 0) return -1;
                if (parameters2.Length == 0) return 1;

                Type paramArrayType1 = null, paramArrayType2 = null;

                if (_enableParamArray)
                {
                    if (parameters1[parameters1.Length - 1].ParameterType.IsArray &&
                        parameters1[parameters1.Length - 1].IsDefined(typeof(ParamArrayAttribute)))
                    {
                        paramArrayType1 = parameters1[parameters1.Length - 1].ParameterType.GetElementType();
                    }

                    if (parameters2[parameters2.Length - 1].ParameterType.IsArray &&
                        parameters2[parameters2.Length - 1].IsDefined(typeof(ParamArrayAttribute)))
                    {
                        paramArrayType2 = parameters2[parameters2.Length - 1].ParameterType.GetElementType();
                    }

                    // A method using params always loses to one not using params
                    if (paramArrayType1 != null && paramArrayType2 == null) return 1;
                    if (paramArrayType2 != null && paramArrayType1 == null) return -1;
                }

                for (int i = 0; i < _types.Count; i++)
                {
                    var type1 = (paramArrayType1 != null && i >= parameters1.Length - 1) ? paramArrayType1 : parameters1[i].ParameterType;
                    var type2 = (paramArrayType2 != null && i >= parameters2.Length - 1) ? paramArrayType2 : parameters2[i].ParameterType;

                    // exact match between parameter types doesn't change score
                    if (type1 == type2) continue;

                    // exact match with source type decides winner immediately
                    if (type1 == _types[i]) return -1;
                    if (type2 == _types[i]) return 1;

                    var r = ChooseMorePreciseType(type1, type2);
                    if (r != 0) return r;
                }

                return 0;
            }

            private static int ChooseMorePreciseType(Type type1, Type type2)
            {
                if (type1.IsByRef || type2.IsByRef)
                {
                    if (type1.IsByRef && type2.IsByRef)
                    {
                        type1 = type1.GetElementType();
                        type2 = type2.GetElementType();
                    }
                    else if (type1.IsByRef)
                    {
                        type1 = type1.GetElementType();
                        if (type1 == type2) return 1;
                    }
                    else
                    {
                        type2 = type2.GetElementType();
                        if (type2 == type1) return -1;
                    }
                }

                bool c1FromC2, c2FromC1;

                if (type1.IsPrimitive() && type2.IsPrimitive())
                {
                    c1FromC2 = CanConvertPrimitive(type2, type1);
                    c2FromC1 = CanConvertPrimitive(type1, type2);
                }
                else
                {
                    c1FromC2 = type1.IsAssignableFrom(type2);
                    c2FromC1 = type2.IsAssignableFrom(type1);
                }

                if (c1FromC2 == c2FromC1) return 0;

                return c1FromC2 ? 1 : -1;
            }

        }
        
        private static MethodBase SelectMethod(IEnumerable<MethodBase> candidates, BindingFlags bindingFlags, IList<Type> types)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            // ParamArrays are not supported by ReflectionDelegateFactory
            // They will be treated like ordinary array arguments
            const bool enableParamArray = false;

            return candidates
                .Where(m => TestAccessibility(m, bindingFlags) && FilterParameters(m.GetParameters(), types, enableParamArray))
                .OrderBy(m => m.GetParameters(), new ParametersMatchComparer(types, enableParamArray))
                .FirstOrDefault();
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
            return (MethodInfo)SelectMethod(type.GetTypeInfo().DeclaredMethods.Where(m => name == null || m.Name == name), bindingFlags, parameterTypes);
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return type.GetConstructors(DefaultFlags);
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().DeclaredConstructors.Where(c => TestAccessibility(c, bindingFlags));
        }
        
        public static ConstructorInfo GetConstructor(this Type type, IList<Type> parameterTypes)
        {
            return type.GetConstructor(DefaultFlags, null, parameterTypes, null);
        }

        public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingFlags, object placeholder1, IList<Type> parameterTypes, object placeholder2)
        {
            return (ConstructorInfo)SelectMethod(type.GetTypeInfo().DeclaredConstructors, bindingFlags, parameterTypes);
        }
        
        public static MemberInfo[] GetMember(this Type type, string member)
        {
            return type.GetMemberInternal(member, null, DefaultFlags);
        }

        public static MemberInfo[] GetMember(this Type type, string member, BindingFlags bindingFlags)
        {
            return type.GetMemberInternal(member, null, bindingFlags);
        }

        public static MemberInfo[] GetMemberInternal(this Type type, string member, MemberTypes? memberType, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetMembersRecursive().Where(m =>
                m.Name == member &&
                // test type before accessibility - accessibility doesn't support some types
                (memberType == null || m.MemberType() == memberType) &&
                TestAccessibility(m, bindingFlags)).ToArray();
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
                foreach (MemberInfo member in t.DeclaredMembers)
                {
                    if (!members.Any(p => p.Name == member.Name))
                    {
                        members.Add(member);
                    }
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
                foreach (PropertyInfo member in t.DeclaredProperties)
                {
                    if (!properties.Any(p => p.Name == member.Name))
                    {
                        properties.Add(member);
                    }
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
                foreach (FieldInfo member in t.DeclaredFields)
                {
                    if (!fields.Any(p => p.Name == member.Name))
                    {
                        fields.Add(member);
                    }
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
            {
                return true;
            }

            if (member.SetMethod != null && TestAccessibility(member.SetMethod, bindingFlags))
            {
                return true;
            }

            return false;
        }

        private static bool TestAccessibility(MemberInfo member, BindingFlags bindingFlags)
        {
            if (member is FieldInfo)
            {
                return TestAccessibility((FieldInfo) member, bindingFlags);
            }
            else if (member is MethodBase)
            {
                return TestAccessibility((MethodBase) member, bindingFlags);
            }
            else if (member is PropertyInfo)
            {
                return TestAccessibility((PropertyInfo) member, bindingFlags);
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
#endif

        public static bool IsAbstract(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsAbstract;
#else
            return type.GetTypeInfo().IsAbstract;
#endif
        }

        public static bool IsVisible(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsVisible;
#else
            return type.GetTypeInfo().IsVisible;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }
        
        public static bool IsPrimitive(this Type type)
        {
#if !(DOTNET || PORTABLE)
            return type.IsPrimitive;
#else
            return type.GetTypeInfo().IsPrimitive;
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

        public static bool ImplementInterface(this Type type, Type interfaceType)
        {
            for (Type currentType = type; currentType != null; currentType = currentType.BaseType())
            {
                IEnumerable<Type> interfaces = currentType.GetInterfaces();
                foreach (Type i in interfaces)
                {
                    if (i == interfaceType || (i != null && i.ImplementInterface(interfaceType)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}