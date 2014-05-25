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

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Utilities
{
    internal class DynamicWrapperBase
    {
        protected internal object UnderlyingObject;
    }

    internal static class DynamicWrapper
    {
        private static readonly object _lock = new object();
        private static readonly WrapperDictionary _wrapperDictionary = new WrapperDictionary();

        private static ModuleBuilder _moduleBuilder;

        private static ModuleBuilder ModuleBuilder
        {
            get
            {
                Init();
                return _moduleBuilder;
            }
        }

        private static void Init()
        {
            if (_moduleBuilder == null)
            {
                lock (_lock)
                {
                    if (_moduleBuilder == null)
                    {
                        AssemblyName assemblyName = new AssemblyName("Newtonsoft.Json.Dynamic");
                        assemblyName.KeyPair = new StrongNameKeyPair(GetStrongKey());

                        AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                        _moduleBuilder = assembly.DefineDynamicModule("Newtonsoft.Json.DynamicModule", false);
                    }
                }
            }
        }

        private static byte[] GetStrongKey()
        {
            const string name = "Newtonsoft.Json.Dynamic.snk";

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new MissingManifestResourceException("Should have " + name + " as an embedded resource.");

                int length = (int)stream.Length;
                byte[] buffer = new byte[length];
                stream.Read(buffer, 0, length);

                return buffer;
            }
        }

        public static Type GetWrapper(Type interfaceType, Type realObjectType)
        {
            Type wrapperType = _wrapperDictionary.GetType(interfaceType, realObjectType);

            if (wrapperType == null)
            {
                lock (_lock)
                {
                    wrapperType = _wrapperDictionary.GetType(interfaceType, realObjectType);

                    if (wrapperType == null)
                    {
                        wrapperType = GenerateWrapperType(interfaceType, realObjectType);
                        _wrapperDictionary.SetType(interfaceType, realObjectType, wrapperType);
                    }
                }
            }

            return wrapperType;
        }

        public static object GetUnderlyingObject(object wrapper)
        {
            DynamicWrapperBase wrapperBase = wrapper as DynamicWrapperBase;
            if (wrapperBase == null)
                throw new ArgumentException("Object is not a wrapper.", "wrapper");

            return wrapperBase.UnderlyingObject;
        }

        private static Type GenerateWrapperType(Type interfaceType, Type underlyingType)
        {
            TypeBuilder wrapperBuilder = ModuleBuilder.DefineType(
                "{0}_{1}_Wrapper".FormatWith(CultureInfo.InvariantCulture, interfaceType.Name, underlyingType.Name),
                TypeAttributes.NotPublic | TypeAttributes.Sealed,
                typeof(DynamicWrapperBase),
                new[] { interfaceType });

            WrapperMethodBuilder wrapperMethod = new WrapperMethodBuilder(underlyingType, wrapperBuilder);

            foreach (MethodInfo method in interfaceType.GetAllMethods())
            {
                wrapperMethod.Generate(method);
            }

            return wrapperBuilder.CreateType();
        }

        public static T CreateWrapper<T>(object realObject) where T : class
        {
            var dynamicType = GetWrapper(typeof(T), realObject.GetType());
            var dynamicWrapper = (DynamicWrapperBase)Activator.CreateInstance(dynamicType);

            dynamicWrapper.UnderlyingObject = realObject;

            return dynamicWrapper as T;
        }
    }

    internal class WrapperMethodBuilder
    {
        private readonly Type _realObjectType;
        private readonly TypeBuilder _wrapperBuilder;

        public WrapperMethodBuilder(Type realObjectType, TypeBuilder proxyBuilder)
        {
            _realObjectType = realObjectType;
            _wrapperBuilder = proxyBuilder;
        }

        public void Generate(MethodInfo newMethod)
        {
            if (newMethod.IsGenericMethod)
                newMethod = newMethod.GetGenericMethodDefinition();

            FieldInfo srcField = typeof(DynamicWrapperBase).GetField("UnderlyingObject", BindingFlags.Instance | BindingFlags.NonPublic);

            var parameters = newMethod.GetParameters();
            var parameterTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

            MethodBuilder methodBuilder = _wrapperBuilder.DefineMethod(
                newMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                newMethod.ReturnType,
                parameterTypes);

            if (newMethod.IsGenericMethod)
            {
                methodBuilder.DefineGenericParameters(
                    newMethod.GetGenericArguments().Select(arg => arg.Name).ToArray());
            }

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            LoadUnderlyingObject(ilGenerator, srcField);
            PushParameters(parameters, ilGenerator);
            ExecuteMethod(newMethod, parameterTypes, ilGenerator);
            Return(ilGenerator);
        }

        private static void Return(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void ExecuteMethod(MethodBase newMethod, Type[] parameterTypes, ILGenerator ilGenerator)
        {
            MethodInfo srcMethod = GetMethod(newMethod, parameterTypes);

            if (srcMethod == null)
                throw new MissingMethodException("Unable to find method " + newMethod.Name + " on " + _realObjectType.FullName);

            ilGenerator.Emit(OpCodes.Call, srcMethod);
        }

        private MethodInfo GetMethod(MethodBase realMethod, Type[] parameterTypes)
        {
            if (realMethod.IsGenericMethod)
                return _realObjectType.GetGenericMethod(realMethod.Name, parameterTypes);

            return _realObjectType.GetMethod(realMethod.Name, parameterTypes);
        }

        private static void PushParameters(ICollection<ParameterInfo> parameters, ILGenerator ilGenerator)
        {
            for (int i = 1; i < parameters.Count + 1; i++)
                ilGenerator.Emit(OpCodes.Ldarg, i);
        }

        private static void LoadUnderlyingObject(ILGenerator ilGenerator, FieldInfo srcField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, srcField);
        }
    }

    internal class WrapperDictionary
    {
        private readonly Dictionary<string, Type> _wrapperTypes = new Dictionary<string, Type>();

        private static string GenerateKey(Type interfaceType, Type realObjectType)
        {
            return interfaceType.Name + "_" + realObjectType.Name;
        }

        public Type GetType(Type interfaceType, Type realObjectType)
        {
            string key = GenerateKey(interfaceType, realObjectType);

            if (_wrapperTypes.ContainsKey(key))
                return _wrapperTypes[key];

            return null;
        }

        public void SetType(Type interfaceType, Type realObjectType, Type wrapperType)
        {
            string key = GenerateKey(interfaceType, realObjectType);

            if (_wrapperTypes.ContainsKey(key))
                _wrapperTypes[key] = wrapperType;
            else
                _wrapperTypes.Add(key, wrapperType);
        }
    }
}

#endif