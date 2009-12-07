#if !SILVERLIGHT && !PocketPC
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using System.Threading;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal class DynamicWrapperBase
  {
    internal protected object UnderlyingObject;
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
      using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Newtonsoft.Json.Dynamic.snk"))
      {
        if (stream == null)
          throw new MissingManifestResourceException("Should have a Newtonsoft.Json.Dynamic.snk as an embedded resource.");

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
        wrapperType = GenerateWrapperType(interfaceType, realObjectType);
        _wrapperDictionary.SetType(interfaceType, realObjectType, wrapperType);
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

      foreach (MethodInfo method in interfaceType.AllMethods())
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

  internal static class TypeExtensions
  {
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

    public static IEnumerable<Type> AllInterfaces(this Type target)
    {
      foreach (var IF in target.GetInterfaces())
      {
        yield return IF;
        foreach (var childIF in IF.AllInterfaces())
        {
          yield return childIF;
        }
      }
    }

    public static IEnumerable<MethodInfo> AllMethods(this Type target)
    {
      var allTypes = target.AllInterfaces().ToList();
      allTypes.Add(target);

      return from type in allTypes
             from method in type.GetMethods()
             select method;
    }
  }
}
#endif