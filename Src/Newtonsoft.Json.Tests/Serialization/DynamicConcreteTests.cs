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

#if !(PORTABLE || DNXCORE50)
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using System;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class DynamicConcreteTests : TestFixtureBase
    {
        public class DynamicConcreteContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                JsonContract contract = base.CreateContract(objectType);

                // create a dynamic mock object for interfaces or abstract classes
                if (contract.CreatedType.IsInterface || contract.CreatedType.IsAbstract)
                {
                    contract.DefaultCreator = () => DynamicConcrete.GetInstanceFor(contract.CreatedType);
                }

                return contract;
            }
        }

        [Test]
        public void UseDynamicConcreteIfTargetObjectTypeIsAnInterfaceWithNoBackingClass()
        {
            string json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<IInterfaceWithNoConcrete>(json, new JsonSerializerSettings
            {
                ContractResolver = new DynamicConcreteContractResolver()
            });

            Assert.AreEqual("Name!", c.Name);
        }

        [Test]
        public void UseDynamicConcreteIfTargetObjectTypeIsAnAbstractClassWithNoConcrete()
        {
            string json = @"{Name:""Name!"", Game:""Same""}";

            var c = JsonConvert.DeserializeObject<AbstractWithNoConcrete>(json, new JsonSerializerSettings
            {
                ContractResolver = new DynamicConcreteContractResolver()
            });

            Assert.AreEqual("Name!", c.Name);
            Assert.AreEqual("Same", c.Game);
        }

        [Test]
        public void AnyMethodsExposedByDynamicConcreteAreHarmless()
        {
            string json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<IInterfaceWithNoConcrete>(json, new JsonSerializerSettings
            {
                ContractResolver = new DynamicConcreteContractResolver()
            });

            c.FuncWithRefType(10, null);
            c.FuncWithValType_1();
            c.FuncWithValType_2();
        }
    }

    public abstract class AbstractWithNoConcrete
    {
        public string Name { get; set; }
        public abstract string Game { get; set; }
    }

    public interface IInterfaceWithNoConcrete
    {
        string Name { get; set; }
        object FuncWithRefType(int a, object b);
        int FuncWithValType_1();
        bool FuncWithValType_2();
    }

    /// <summary>
    /// Creates run-time backing types for abstract classes and interfaces
    /// </summary>
    public static class DynamicConcrete
    {
        /// <summary>
        /// Get an empty instance of a dynamic proxy for type T.
        /// All public fields are writable and all properties have both getters and setters.
        /// </summary>
        public static T GetInstanceFor<T>()
        {
            return (T)GetInstanceFor(typeof(T));
        }

        static readonly ModuleBuilder ModuleBuilder;
        static readonly AssemblyBuilder DynamicAssembly;

        /// <summary>
        /// Get an empty instance of a dynamic proxy for the given type.
        /// All public fields are writable and all properties have both getters and setters.
        /// </summary>
        public static object GetInstanceFor(Type targetType)
        {
            lock (DynamicAssembly)
            {
                var constructedType = DynamicAssembly.GetType(ProxyName(targetType)) ?? GetConstructedType(targetType);
                var instance = Activator.CreateInstance(constructedType);
                return instance;
            }
        }

        static string ProxyName(Type targetType)
        {
            return targetType.Name + "Proxy";
        }

        static DynamicConcrete()
        {
            var assemblyName = new AssemblyName("DynImpl");
            DynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = DynamicAssembly.DefineDynamicModule("DynImplModule");
        }

        static Type GetConstructedType(Type targetType)
        {
            var typeBuilder = ModuleBuilder.DefineType(targetType.Name + "Proxy", TypeAttributes.Public);

            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { });
            var ilGenerator = ctorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ret);

            IncludeType(targetType, typeBuilder);

            foreach (var face in targetType.GetInterfaces())
            {
                IncludeType(face, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        static void IncludeType(Type typeOfT, TypeBuilder typeBuilder)
        {
            var methodInfos = typeOfT.GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name.StartsWith("set_"))
                {
                    continue; // we always add a set for a get.
                }

                if (methodInfo.Name.StartsWith("get_"))
                {
                    BindProperty(typeBuilder, methodInfo);
                }
                else
                {
                    if (methodInfo.IsAbstract)
                    {
                        BindMethod(typeBuilder, methodInfo);
                    }
                }
            }

            if (typeOfT.IsInterface)
            {
                typeBuilder.AddInterfaceImplementation(typeOfT);
            }
            else if (typeOfT.IsAbstract)
            {
                typeBuilder.SetParent(typeOfT);
            }
        }

        static void BindMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(
                methodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                methodInfo.ReturnType,
                args
                );

            var methodILGen = methodBuilder.GetILGenerator();
            if (methodInfo.ReturnType == typeof(void))
            {
                methodILGen.Emit(OpCodes.Ret);
            }
            else
            {
                if (methodInfo.ReturnType.IsPrimitive)
                {
                    methodILGen.Emit(OpCodes.Ldc_I4_0);
                }
                else if (methodInfo.ReturnType.IsValueType || methodInfo.ReturnType.IsEnum)
                {
                    var getMethod = typeof(Activator).GetMethod("CreateInstance",
                        new[] { typeof(Type) });
                    var lb = methodILGen.DeclareLocal(methodInfo.ReturnType);
                    if (lb.LocalType != null)
                    {
                        methodILGen.Emit(OpCodes.Ldtoken, lb.LocalType);
                        methodILGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                        methodILGen.Emit(OpCodes.Callvirt, getMethod);
                        methodILGen.Emit(OpCodes.Unbox_Any, lb.LocalType);
                    }
                }
                else
                {
                    methodILGen.Emit(OpCodes.Ldnull);
                }
                methodILGen.Emit(OpCodes.Ret);
            }
            typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }

        /// <summary>
        /// Bind a new property into a type builder with getters and setters.
        /// </summary>
        public static void BindProperty(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            // Backing Field
            var propertyName = methodInfo.Name.Replace("get_", "");
            var propertyType = methodInfo.ReturnType;
            var backingField = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            //Getter
            var backingGet = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public |
                                                                             MethodAttributes.SpecialName | MethodAttributes.Virtual |
                                                                             MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = backingGet.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, backingField);
            getIl.Emit(OpCodes.Ret);

            //Setter
            var backingSet = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public |
                                                                             MethodAttributes.SpecialName | MethodAttributes.Virtual |
                                                                             MethodAttributes.HideBySig, null, new[] { propertyType });

            var setIl = backingSet.GetILGenerator();

            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, backingField);
            setIl.Emit(OpCodes.Ret);

            // Property
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
            propertyBuilder.SetGetMethod(backingGet);
            propertyBuilder.SetSetMethod(backingSet);
        }
    }
}

#endif