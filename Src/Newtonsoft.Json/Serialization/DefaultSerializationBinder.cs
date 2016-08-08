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
using System.Runtime.Serialization;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// The default serialization binder used when resolving and loading classes from type names.
    /// </summary>
    public class DefaultSerializationBinder : SerializationBinder
    {
        internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

        private readonly ThreadSafeStore<TypeNameKey, Type> _typeCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSerializationBinder"/> class.
        /// </summary>
        public DefaultSerializationBinder()
        {
            _typeCache = new ThreadSafeStore<TypeNameKey, Type>(GetTypeFromTypeNameKey);
        }

        private Type GetTypeFromTypeNameKey(TypeNameKey typeNameKey)
        {
            string assemblyName = typeNameKey.AssemblyName;
            string typeName = typeNameKey.TypeName;

            if (assemblyName != null)
            {
                Assembly assembly;

#if !(DOTNET || PORTABLE40 || PORTABLE)
                // look, I don't like using obsolete methods as much as you do but this is the only way
                // Assembly.Load won't check the GAC for a partial name
#pragma warning disable 618,612
                assembly = Assembly.LoadWithPartialName(assemblyName);
#pragma warning restore 618,612
#elif DOTNET || PORTABLE
                assembly = Assembly.Load(new AssemblyName(assemblyName));
#else
                assembly = Assembly.Load(assemblyName);
#endif

#if !(PORTABLE40 || PORTABLE || DOTNET)
                if (assembly == null)
                {
                    // will find assemblies loaded with Assembly.LoadFile outside of the main directory
                    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly a in loadedAssemblies)
                    {
                        // check for both full name or partial name match
                        if (a.FullName == assemblyName || a.GetName().Name == assemblyName)
                        {
                            assembly = a;
                            break;
                        }
                    }
                }
#endif

                if (assembly == null)
                {
                    throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, assemblyName));
                }

                Type type = assembly.GetType(typeName);
                if (type == null)
                {
                    // if generic type, try manually parsing the type arguments for the case of dynamically loaded assemblies
                    // example generic typeName format: System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                    if (typeName.IndexOf('`') >= 0)
                    {
                        try
                        {
                            type = GetGenericTypeFromTypeName(typeName, assembly);
                        }
                        catch (Exception ex)
                        {
                            throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName), ex);
                        }
                    }

                    if (type == null)
                    {
                        throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName));
                    }
                }

                return type;
            }
            else
            {
                return Type.GetType(typeName);
            }
        }

        private Type GetGenericTypeFromTypeName(string typeName, Assembly assembly)
        {
            Type type = null;
            var openBracketIndex = typeName.IndexOf('[');
            if (openBracketIndex >= 0)
            {
                var genericTypeDefName = typeName.Substring(0, openBracketIndex);
                var genericTypeDef = assembly.GetType(genericTypeDefName);
                if (genericTypeDef != null)
                {
                    var typeArgs = new List<Type>();
                    var scope = 0;
                    var typeArgStartIndex = 0;
                    var endIndex = typeName.Length - 1;
                    for (var i = openBracketIndex + 1; i < endIndex; ++i)
                    {
                        var current = typeName[i];
                        switch (current)
                        {
                            case '[':
                                if (scope == 0)
                                {
                                    typeArgStartIndex = i + 1;
                                }
                                ++scope;
                                break;
                            case ']':
                                --scope;
                                if (scope == 0)
                                {
                                    var typeArgAssemblyQualifiedName = typeName.Substring(typeArgStartIndex, i - typeArgStartIndex);
                                    string typeArgTypeName;
                                    string typeArgAssemblyName;
                                    ReflectionUtils.SplitFullyQualifiedTypeName(typeArgAssemblyQualifiedName, out typeArgTypeName, out typeArgAssemblyName);
                                    var typeArg = BindToType(typeArgAssemblyName, typeArgTypeName);
                                    typeArgs.Add(typeArg);
                                }
                                break;
                        }
                    }
                    type = genericTypeDef.MakeGenericType(typeArgs.ToArray());
                }
            }
            return type;
        }

        internal struct TypeNameKey : IEquatable<TypeNameKey>
        {
            internal readonly string AssemblyName;
            internal readonly string TypeName;

            public TypeNameKey(string assemblyName, string typeName)
            {
                AssemblyName = assemblyName;
                TypeName = typeName;
            }

            public override int GetHashCode()
            {
                return ((AssemblyName != null) ? AssemblyName.GetHashCode() : 0)
                       ^ ((TypeName != null) ? TypeName.GetHashCode() : 0);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeNameKey))
                {
                    return false;
                }

                return Equals((TypeNameKey)obj);
            }

            public bool Equals(TypeNameKey other)
            {
                return (AssemblyName == other.AssemblyName && TypeName == other.TypeName);
            }
        }

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object.</param>
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            return _typeCache.Get(new TypeNameKey(assemblyName, typeName));
        }

#if !(NET35 || NET20)
        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. </param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object. </param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
#if (DOTNET || PORTABLE)
            assemblyName = serializedType.GetTypeInfo().Assembly.FullName;
            typeName = serializedType.FullName;
#else
            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName;
#endif
        }
#endif
    }
}