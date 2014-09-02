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

#if !(PORTABLE || NETFX_CORE || PORTABLE40)
using System;
using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Serialization;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
    internal class DynamicReflectionDelegateFactory : ReflectionDelegateFactory
    {
        public static DynamicReflectionDelegateFactory Instance = new DynamicReflectionDelegateFactory();

        private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
        {
            DynamicMethod dynamicMethod = !owner.IsInterface()
                ? new DynamicMethod(name, returnType, parameterTypes, owner, true)
                : new DynamicMethod(name, returnType, parameterTypes, owner.Module, true);

            return dynamicMethod;
        }

        public override ObjectConstructor<object> CreateParametrizedConstructor(MethodBase method)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod(method.ToString(), typeof(object), new[] { typeof(object[]) }, method.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateMethodCallIL(method, generator, 0);

            return (ObjectConstructor<object>)dynamicMethod.CreateDelegate(typeof(ObjectConstructor<object>));
        }

        public override MethodCall<T, object> CreateMethodCall<T>(MethodBase method)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod(method.ToString(), typeof(object), new[] { typeof(object), typeof(object[]) }, method.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateMethodCallIL(method, generator, 1);

            return (MethodCall<T, object>)dynamicMethod.CreateDelegate(typeof(MethodCall<T, object>));
        }

        private void GenerateCreateMethodCallIL(MethodBase method, ILGenerator generator, int argsIndex)
        {
            ParameterInfo[] args = method.GetParameters();

            Label argsOk = generator.DefineLabel();

            // throw an error if the number of argument values doesn't match method parameters
            generator.Emit(OpCodes.Ldarg, argsIndex);
            generator.Emit(OpCodes.Ldlen);
            generator.Emit(OpCodes.Ldc_I4, args.Length);
            generator.Emit(OpCodes.Beq, argsOk);
            generator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(ReflectionUtils.EmptyTypes));
            generator.Emit(OpCodes.Throw);

            generator.MarkLabel(argsOk);

            if (!method.IsConstructor && !method.IsStatic)
                generator.PushInstance(method.DeclaringType);

            int localVariableCount = 0;

            for (int i = 0; i < args.Length; i++)
            {
                ParameterInfo parameter = args[i];
                Type parameterType = parameter.ParameterType;

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();

                    LocalBuilder localVariable = generator.DeclareLocal(parameterType);

                    // don't need to set variable for 'out' parameter
                    if (!parameter.IsOut)
                    {
                        generator.PushArrayInstance(argsIndex, i);

                        if (parameterType.IsValueType())
                        {
                            Label skipSettingDefault = generator.DefineLabel();
                            Label finishedProcessingParameter = generator.DefineLabel();

                            // check if parameter is not null
                            generator.Emit(OpCodes.Brtrue_S, skipSettingDefault);

                            // parameter has no value, initialize to default
                            generator.Emit(OpCodes.Ldloca_S, localVariable);
                            generator.Emit(OpCodes.Initobj, parameterType);
                            generator.Emit(OpCodes.Br_S, finishedProcessingParameter);

                            // parameter has value, get value from array again and unbox and set to variable
                            generator.MarkLabel(skipSettingDefault);
                            generator.PushArrayInstance(argsIndex, i);
                            generator.UnboxIfNeeded(parameterType);
                            generator.Emit(OpCodes.Stloc, localVariableCount);

                            // parameter finished, we out!
                            generator.MarkLabel(finishedProcessingParameter);
                        }
                        else
                        {
                            generator.UnboxIfNeeded(parameterType);
                            generator.Emit(OpCodes.Stloc, localVariableCount);
                        }
                    }

                    generator.Emit(OpCodes.Ldloca_S, localVariable);

                    localVariableCount++;
                }
                else if (parameterType.IsValueType())
                {
                    generator.PushArrayInstance(argsIndex, i);

                    // have to check that value type parameters aren't null
                    // otherwise they will error when unboxed
                    Label skipSettingDefault = generator.DefineLabel();
                    Label finishedProcessingParameter = generator.DefineLabel();

                    // check if parameter is not null
                    generator.Emit(OpCodes.Brtrue_S, skipSettingDefault);

                    // parameter has no value, initialize to default
                    LocalBuilder localVariable = generator.DeclareLocal(parameterType);
                    generator.Emit(OpCodes.Ldloca_S, localVariable);
                    generator.Emit(OpCodes.Initobj, parameterType);
                    generator.Emit(OpCodes.Ldloc, localVariableCount);
                    generator.Emit(OpCodes.Br_S, finishedProcessingParameter);

                    // parameter has value, get value from array again and unbox
                    generator.MarkLabel(skipSettingDefault);
                    generator.PushArrayInstance(argsIndex, i);
                    generator.UnboxIfNeeded(parameterType);

                    // parameter finished, we out!
                    generator.MarkLabel(finishedProcessingParameter);
                    localVariableCount++;
                }
                else 
                {
                    generator.PushArrayInstance(argsIndex, i);

                    generator.UnboxIfNeeded(parameterType);
                }
            }

            if (method.IsConstructor)
                generator.Emit(OpCodes.Newobj, (ConstructorInfo)method);
            else
                generator.CallMethod((MethodInfo)method);

            Type returnType = method.IsConstructor
                ? method.DeclaringType
                : ((MethodInfo)method).ReturnType;

            if (returnType != typeof(void))
                generator.BoxIfNeeded(returnType);
            else
                generator.Emit(OpCodes.Ldnull);

            generator.Return();
        }

        public override Func<T> CreateDefaultConstructor<T>(Type type)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod("Create" + type.FullName, typeof(T), ReflectionUtils.EmptyTypes, type);
            dynamicMethod.InitLocals = true;
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateDefaultConstructorIL(type, generator);

            return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
        }

        private void GenerateCreateDefaultConstructorIL(Type type, ILGenerator generator)
        {
            if (type.IsValueType())
            {
                generator.DeclareLocal(type);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Box, type);
            }
            else
            {
                ConstructorInfo constructorInfo =
                    type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        ReflectionUtils.EmptyTypes, null);

                if (constructorInfo == null)
                    throw new ArgumentException("Could not get constructor for {0}.".FormatWith(CultureInfo.InvariantCulture, type));

                generator.Emit(OpCodes.Newobj, constructorInfo);
            }

            generator.Return();
        }

        public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + propertyInfo.Name, typeof(T), new[] { typeof(object) }, propertyInfo.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateGetPropertyIL(propertyInfo, generator);

            return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
        }

        private void GenerateCreateGetPropertyIL(PropertyInfo propertyInfo, ILGenerator generator)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException("Property '{0}' does not have a getter.".FormatWith(CultureInfo.InvariantCulture, propertyInfo.Name));

            if (!getMethod.IsStatic)
                generator.PushInstance(propertyInfo.DeclaringType);

            generator.CallMethod(getMethod);
            generator.BoxIfNeeded(propertyInfo.PropertyType);
            generator.Return();
        }

        public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + fieldInfo.Name, typeof(T), new[] { typeof(object) }, fieldInfo.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateGetFieldIL(fieldInfo, generator);

            return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
        }

        private void GenerateCreateGetFieldIL(FieldInfo fieldInfo, ILGenerator generator)
        {
            if (!fieldInfo.IsStatic)
            {
                generator.PushInstance(fieldInfo.DeclaringType);
                generator.Emit(OpCodes.Ldfld, fieldInfo);
            }
            else
            {
                generator.Emit(OpCodes.Ldsfld, fieldInfo);
            }

            generator.BoxIfNeeded(fieldInfo.FieldType);
            generator.Return();
        }

        public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + fieldInfo.Name, null, new[] { typeof(T), typeof(object) }, fieldInfo.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateSetFieldIL(fieldInfo, generator);

            return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
        }

        internal static void GenerateCreateSetFieldIL(FieldInfo fieldInfo, ILGenerator generator)
        {
            if (!fieldInfo.IsStatic)
                generator.PushInstance(fieldInfo.DeclaringType);

            generator.Emit(OpCodes.Ldarg_1);
            generator.UnboxIfNeeded(fieldInfo.FieldType);

            if (!fieldInfo.IsStatic)
                generator.Emit(OpCodes.Stfld, fieldInfo);
            else
                generator.Emit(OpCodes.Stsfld, fieldInfo);

            generator.Return();
        }

        public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
        {
            DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + propertyInfo.Name, null, new[] { typeof(T), typeof(object) }, propertyInfo.DeclaringType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            GenerateCreateSetPropertyIL(propertyInfo, generator);

            return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
        }

        internal static void GenerateCreateSetPropertyIL(PropertyInfo propertyInfo, ILGenerator generator)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod(true);
            if (!setMethod.IsStatic)
                generator.PushInstance(propertyInfo.DeclaringType);

            generator.Emit(OpCodes.Ldarg_1);
            generator.UnboxIfNeeded(propertyInfo.PropertyType);
            generator.CallMethod(setMethod);
            generator.Return();
        }
    }
}
#endif