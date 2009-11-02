#if !PocketPC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal static class LateBoundDelegateFactory
  {
    public static Func<T, object> CreateGet<T>(MemberInfo memberInfo)
    {
      PropertyInfo propertyInfo = memberInfo as PropertyInfo;
      if (propertyInfo != null)
        return CreateGet<T>(propertyInfo);

      FieldInfo fieldInfo = memberInfo as FieldInfo;
      if (fieldInfo != null)
        return CreateGet<T>(fieldInfo);

      throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
    }

    private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
    {
      DynamicMethod dynamicMethod = !owner.IsInterface
        ? new DynamicMethod(name, returnType, parameterTypes, owner, true)
        : new DynamicMethod(name, returnType, parameterTypes, (Module)null, true);

      return dynamicMethod;
    }

    public static Action<T, object> CreateSet<T>(MemberInfo memberInfo)
    {
      PropertyInfo propertyInfo = memberInfo as PropertyInfo;
      if (propertyInfo != null)
        return CreateSet<T>(propertyInfo);

      FieldInfo fieldInfo = memberInfo as FieldInfo;
      if (fieldInfo != null)
        return CreateSet<T>(fieldInfo);

      throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
    }

    public static MemberHandler<object> CreateMethodHandler(MethodBase method)
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
        generator.Emit(OpCodes.Call, (MethodInfo)method);
      else
        generator.Emit(OpCodes.Callvirt, (MethodInfo)method);

      Type returnType = method.IsConstructor
        ? method.DeclaringType
        : ((MethodInfo)method).ReturnType;

      if (returnType != typeof(void))
        generator.BoxIfNeeded(returnType);
      else
        generator.Emit(OpCodes.Ldnull);

      generator.Emit(OpCodes.Ret);

      return (MemberHandler<object>)dynamicMethod.CreateDelegate(typeof(MemberHandler<object>));
    }


    public static Func<object> CreateDefaultConstructor(Type type)
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

      generator.Emit(OpCodes.Ret);

      return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
    }

    public static Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
    {
      MethodInfo getMethod = propertyInfo.GetGetMethod(true);
      if (getMethod == null)
        throw new Exception("Property '{0}' does not have a getter.".FormatWith(CultureInfo.InvariantCulture,
                                                                                propertyInfo.Name));

      DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + propertyInfo.Name, typeof(T), new[] { typeof(object) }, propertyInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!getMethod.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      if (getMethod.IsFinal || !getMethod.IsVirtual)
        generator.Emit(OpCodes.Call, getMethod);
      else
        generator.Emit(OpCodes.Callvirt, getMethod);

      generator.BoxIfNeeded(propertyInfo.PropertyType);
      generator.Emit(OpCodes.Ret);

      return (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
    }

    static void PushInstance(this ILGenerator generator, Type type)
    {
      generator.Emit(OpCodes.Ldarg_0);
      if (type.IsValueType)
        generator.Emit(OpCodes.Unbox, type);
    }

    static void BoxIfNeeded(this ILGenerator generator, Type type)
    {
      if (type.IsValueType)
        generator.Emit(OpCodes.Box, type);
    }

    static void UnboxIfNeeded(this ILGenerator generator, Type type)
    {
      if (type.IsValueType)
        generator.Emit(OpCodes.Unbox_Any, type);
    }

    public static Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod("Get" + fieldInfo.Name, typeof(T), new[] { typeof(object) }, fieldInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!fieldInfo.IsStatic)
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldfld, fieldInfo);
      generator.BoxIfNeeded(fieldInfo.FieldType);
      generator.Emit(OpCodes.Ret);

      return (Func<T, object>)
          dynamicMethod.CreateDelegate(typeof(Func<T, object>));
    }

    public static Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
    {
      DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + fieldInfo.Name, null, new[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType);
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!fieldInfo.IsStatic)
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(fieldInfo.FieldType);
      generator.Emit(OpCodes.Stfld, fieldInfo);
      generator.Emit(OpCodes.Ret);

      return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
    }

    public static Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
    {
      MethodInfo getMethod = propertyInfo.GetSetMethod(true);
      DynamicMethod dynamicMethod = CreateDynamicMethod("Set" + propertyInfo.Name, null, new[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType);
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!getMethod.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(propertyInfo.PropertyType);

      if (getMethod.IsFinal || !getMethod.IsVirtual)
        generator.Emit(OpCodes.Call, getMethod);
      else
        generator.Emit(OpCodes.Callvirt, getMethod);

      generator.Emit(OpCodes.Ret);

      return (Action<T, object>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
    }
  }
}
#endif