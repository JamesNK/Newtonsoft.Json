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

#if HAVE_FSHARP_TYPES
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
    internal class FSharpFunction
    {
        private readonly object _instance;
        private readonly MethodCall<object, object> _invoker;

        public FSharpFunction(object instance, MethodCall<object, object> invoker)
        {
            _instance = instance;
            _invoker = invoker;
        }

        public object Invoke(params object[] args)
        {
            object o = _invoker(_instance, args);

            return o;
        }
    }

    internal static class FSharpUtils
    {
        private static readonly object Lock = new object();

        private static bool _initialized;
        private static MethodInfo _ofSeq;
        private static Type _mapType;

        public static Assembly FSharpCoreAssembly { get; private set; }
        public static MethodCall<object, object> IsUnion { get; private set; }
        public static MethodCall<object, object> GetUnionCases { get; private set; }
        public static MethodCall<object, object> PreComputeUnionTagReader { get; private set; }
        public static MethodCall<object, object> PreComputeUnionReader { get; private set; }
        public static MethodCall<object, object> PreComputeUnionConstructor { get; private set; }
        public static Func<object, object> GetUnionCaseInfoDeclaringType { get; private set; }
        public static Func<object, object> GetUnionCaseInfoName { get; private set; }
        public static Func<object, object> GetUnionCaseInfoTag { get; private set; }
        public static MethodCall<object, object> GetUnionCaseInfoFields { get; private set; }

        public const string FSharpSetTypeName = "FSharpSet`1";
        public const string FSharpListTypeName = "FSharpList`1";
        public const string FSharpMapTypeName = "FSharpMap`2";

        public static void EnsureInitialized(Assembly fsharpCoreAssembly)
        {
            if (!_initialized)
            {
                lock (Lock)
                {
                    if (!_initialized)
                    {
                        FSharpCoreAssembly = fsharpCoreAssembly;

                        Type fsharpType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpType");

                        MethodInfo isUnionMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "IsUnion", BindingFlags.Public | BindingFlags.Static);
                        IsUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(isUnionMethodInfo);

                        MethodInfo getUnionCasesMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "GetUnionCases", BindingFlags.Public | BindingFlags.Static);
                        GetUnionCases = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getUnionCasesMethodInfo);

                        Type fsharpValue = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue");

                        PreComputeUnionTagReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionTagReader");
                        PreComputeUnionReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionReader");
                        PreComputeUnionConstructor = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionConstructor");

                        Type unionCaseInfo = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo");

                        GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name"));
                        GetUnionCaseInfoTag = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Tag"));
                        GetUnionCaseInfoDeclaringType = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("DeclaringType"));
                        GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields"));

                        Type listModule = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule");
                        _ofSeq = listModule.GetMethod("OfSeq");

                        _mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2");

#if HAVE_MEMORY_BARRIER
                        Thread.MemoryBarrier();
#endif
                        _initialized = true;
                    }
                }
            }
        }

        private static MethodInfo GetMethodWithNonPublicFallback(Type type, string methodName, BindingFlags bindingFlags)
        {
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);

            // if no matching method then attempt to find with nonpublic flag
            // this is required because in WinApps some methods are private but always using NonPublic breaks medium trust
            // https://github.com/JamesNK/Newtonsoft.Json/pull/649
            // https://github.com/JamesNK/Newtonsoft.Json/issues/821
            if (methodInfo == null && (bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic)
            {
                methodInfo = type.GetMethod(methodName, bindingFlags | BindingFlags.NonPublic);
            }

            return methodInfo;
        }

        private static MethodCall<object, object> CreateFSharpFuncCall(Type type, string methodName)
        {
            MethodInfo innerMethodInfo = GetMethodWithNonPublicFallback(type, methodName, BindingFlags.Public | BindingFlags.Static);
            MethodInfo invokeFunc = innerMethodInfo.ReturnType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

            MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(innerMethodInfo);
            MethodCall<object, object> invoke = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(invokeFunc);

            MethodCall<object, object> createFunction = (target, args) =>
            {
                object result = call(target, args);

                FSharpFunction f = new FSharpFunction(result, invoke);
                return f;
            };

            return createFunction;
        }

        public static ObjectConstructor<object> CreateSeq(Type t)
        {
            MethodInfo seqType = _ofSeq.MakeGenericMethod(t);

            return JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(seqType);
        }

        public static ObjectConstructor<object> CreateMap(Type keyType, Type valueType)
        {
            MethodInfo creatorDefinition = typeof(FSharpUtils).GetMethod("BuildMapCreator");

            MethodInfo creatorGeneric = creatorDefinition.MakeGenericMethod(keyType, valueType);

            return (ObjectConstructor<object>)creatorGeneric.Invoke(null, null);
        }

        public static ObjectConstructor<object> BuildMapCreator<TKey, TValue>()
        {
            Type genericMapType = _mapType.MakeGenericType(typeof(TKey), typeof(TValue));
            ConstructorInfo ctor = genericMapType.GetConstructor(new[] { typeof(IEnumerable<Tuple<TKey, TValue>>) });
            ObjectConstructor<object> ctorDelegate = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(ctor);

            ObjectConstructor<object> creator = args =>
            {
                // convert dictionary KeyValuePairs to Tuples
                IEnumerable<KeyValuePair<TKey, TValue>> values = (IEnumerable<KeyValuePair<TKey, TValue>>)args[0];
                IEnumerable<Tuple<TKey, TValue>> tupleValues = values.Select(kv => new Tuple<TKey, TValue>(kv.Key, kv.Value));

                return ctorDelegate(tupleValues);
            };

            return creator;
        }
    }
}

#endif