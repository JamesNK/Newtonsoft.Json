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

using System.Threading;
#if !(NET35 || NET20 || NETFX_CORE)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
    internal static class FSharpUtils
    {
        private static readonly object Lock = new object();

        private static bool _initialized;
        private static MethodInfo _ofSeq;

        public static Assembly FSharpCoreAssembly { get; private set; }
        public static MethodCall<object, object> IsUnion { get; private set; }
        public static MethodCall<object, object> GetUnionFields { get; private set; }
        public static MethodCall<object, object> GetUnionCases { get; private set; }
        public static MethodCall<object, object> MakeUnion { get; private set; }
        public static Func<object, object> GetUnionCaseInfoName { get; private set; }
        public static Func<object, object> GetUnionCaseInfo { get; private set; }
        public static Func<object, object> GetUnionCaseFields { get; private set; }
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

                        MethodInfo isUnionMethodInfo = fsharpType.GetMethod("IsUnion", BindingFlags.Public | BindingFlags.Static);
                        IsUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(isUnionMethodInfo);

                        MethodInfo getUnionCasesMethodInfo = fsharpType.GetMethod("GetUnionCases", BindingFlags.Public | BindingFlags.Static);
                        GetUnionCases = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getUnionCasesMethodInfo);

                        Type fsharpValue = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue");

                        MethodInfo getUnionFieldsMethodInfo = fsharpValue.GetMethod("GetUnionFields", BindingFlags.Public | BindingFlags.Static);
                        GetUnionFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getUnionFieldsMethodInfo);

                        GetUnionCaseInfo = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(getUnionFieldsMethodInfo.ReturnType.GetProperty("Item1"));
                        GetUnionCaseFields = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(getUnionFieldsMethodInfo.ReturnType.GetProperty("Item2"));

                        MethodInfo makeUnionMethodInfo = fsharpValue.GetMethod("MakeUnion", BindingFlags.Public | BindingFlags.Static);
                        MakeUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(makeUnionMethodInfo);

                        Type unionCaseInfo = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo");

                        GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name"));
                        GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields"));

                        Type listModule = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule");
                        _ofSeq = listModule.GetMethod("OfSeq");

#if !(NETFX_CORE || PORTABLE)
                        Thread.MemoryBarrier();
#endif
                        _initialized = true;
                    }
                }
            }
        }

        public static MethodInfo CreateSeq(Type t)
        {
            MethodInfo seqType = _ofSeq.MakeGenericMethod(t);

            return seqType;
        }

        public static MethodInfo CreateMap(Type keyType, Type valueType)
        {
            Type t = typeof(FSharpMapCreator<,>).MakeGenericType(keyType, valueType);

            MethodInfo initializeMethod = t.GetMethod("EnsureInitialized");

            initializeMethod.Invoke(null, new object[] { FSharpCoreAssembly });

            MethodInfo typedCreateMethod = t.GetMethod("CreateMapGeneric");

            return typedCreateMethod;
        }

    }

    internal static class FSharpMapCreator<TKey, TValue>
    {
        private static bool _initialized;
        private static MethodCall<object, object> _createMap;

        public static void EnsureInitialized(Assembly fsharpCoreAssembly)
        {
            if (!_initialized)
            {
                Type mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2");

                Type genericMapType = mapType.MakeGenericType(typeof(TKey), typeof(TValue));

                ConstructorInfo ctor = genericMapType.GetConstructor(new[] { typeof(IEnumerable<Tuple<TKey, TValue>>) });

                _createMap = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(ctor);
                
                _initialized = true;
            }
        }

        public static object CreateMapGeneric(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            IEnumerable<Tuple<TKey, TValue>> tupleValues = values.Select(kv => new Tuple<TKey, TValue>(kv.Key, kv.Value));

            object map = _createMap(null, tupleValues);
            return map;
        }
    }
}
#endif