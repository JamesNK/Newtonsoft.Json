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

#if !PocketPC && !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal class DynamicReflectionDelegateFactory : ReflectionDelegateFactory
  {
    public static DynamicReflectionDelegateFactory Instance = new DynamicReflectionDelegateFactory();

    private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
    {
      DynamicMethod dynamicMethod = !owner.IsInterface
        ? new DynamicMethod(name, returnType, parameterTypes, owner, true)
        : new DynamicMethod(name, returnType, parameterTypes, (Module)null, true);

      return dynamicMethod;
    }

    public override MethodCall<object> CreateMethodCall(MethodBase method)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod(method.ToString(), typeof(object), new[] { typeof(object), typeof(object[]) }, method.DeclaringType);
      ILGenerator generator = dynamicMethod.GetILGenerator();

      ParameterInfo[] args = method.GetParameters();

      Label argsOk = generator.DefineLabel();

      generator.Emit(OpCodes.Ldarg_1);
      generator.Emit(OpCodes.Ldlen);
      generator.Emit(OpCodes.Ldc_I4, args.Length);
      generator.Emit(OpCodes.Beq, argsOk);

      generator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes));
      generator.Emit(OpCodes.Throw);

      generator.MarkLabel(argsOk);

      if (!method.IsConstructor)
        generator.PushInstance(method.DeclaringType);

      for (int i = 0; i < args.Length; i++)
      {
        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Ldc_I4, i);
        generator.Emit(OpCodes.Ldelem_Ref);

        generator.UnboxIfNeeded(args[i].ParameterType);
      }

      if (method.IsConstructor)
        generator.Emit(OpCodes.Newobj, (ConstructorInfo)method);
      else if (method.IsFinal || !method.IsVirtual)
        generator.CallMethod((MethodInfo)method);

      Type returnType = method.IsConstructor
        ? method.DeclaringType
        : ((MethodInfo)method).ReturnType;

      if (returnType != typeof(void))
        generator.BoxIfNeeded(returnType);
      else
        generator.Emit(OpCodes.Ldnull);

      generator.Return();

      return (MethodCall<object>)dynamicMethod.CreateDelegate(typeof(MethodCall<object>));
    }

    public override Func<object> CreateDefaultConstructor(Type type)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod("Create" + type.FullName, typeof(object), Type.EmptyTypes, type);
      dynamicMethod.InitLocals = true;
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (type.IsValueType)
      {
        generator.DeclareLocal(type);
        generator.Emit(OpCodes.Ldloc_0);
        generator.Emit(OpCodes.Box, type);
      }
      else
      {
        ConstructorInfo constructorInfo =
          type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                              Type.EmptyTypes, null);

        if (constructorInfo == null)
          throw new Exception("Could not get constructor for {0}.".FormatWith(CultureInfo.InvariantCulture, type));

        generator.Emit(OpCodes.Newobj, constructorInfo);
      }

      generator.Return();

      return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
    }

    public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
    {
      MethodInfo getMethod = propertyInfo.GetGetMethod(true);
      if (getMethod == null)
        throw new Exception("Property '{0}' does not have a getter.".FormatWith(CultureInfo.InvariantCulture,
                                                                                propertyInfo.Name));

      DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + propertyInfo.Name, typeof(T), new[] { typeof(object) }, propertyInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!getMethod.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      generator.CallMethod(getMethod);
      generator.BoxIfNeeded(propertyInfo.PropertyType);
      generator.Return();

      return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
    }

    public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + fieldInfo.Name, typeof(T), new[] { typeof(object) }, fieldInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!fieldInfo.IsStatic)
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldfld, fieldInfo);
      generator.BoxIfNeeded(fieldInfo.FieldType);
      generator.Return();

      return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
    }

    public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + fieldInfo.Name, null, new[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType);
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!fieldInfo.IsStatic)
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(fieldInfo.FieldType);
      generator.Emit(OpCodes.Stfld, fieldInfo);
      generator.Return();

      return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
    }

    public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
    {
      MethodInfo setMethod = propertyInfo.GetSetMethod(true);
      DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + propertyInfo.Name, null, new[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType);
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!setMethod.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(propertyInfo.PropertyType);
      generator.CallMethod(setMethod);
      generator.Return();

      return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
    }
  }
}
#endif