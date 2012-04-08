#if !(NET35 || NET20 || WINDOWS_PHONE)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
  internal static class DynamicUtils
  {
    internal static class BinderWrapper
    {
#if !SILVERLIGHT
      public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
      public const string CSharpAssemblyName = "Microsoft.CSharp, Version=2.0.5.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#endif

      private const string BinderTypeName = "Microsoft.CSharp.RuntimeBinder.Binder, " + CSharpAssemblyName;
      private const string CSharpArgumentInfoTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, " + CSharpAssemblyName;
      private const string CSharpArgumentInfoFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, " + CSharpAssemblyName;
      private const string CSharpBinderFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, " + CSharpAssemblyName;

      private static object _getCSharpArgumentInfoArray;
      private static object _setCSharpArgumentInfoArray;
      private static MethodCall<object, object> _getMemberCall;
      private static MethodCall<object, object> _setMemberCall;
      private static bool _init;

      private static void Init()
      {
        if (!_init)
        {
          Type binderType = Type.GetType(BinderTypeName, false);
          if (binderType == null)
            throw new Exception("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.".FormatWith(CultureInfo.InvariantCulture, BinderTypeName));

          // None
          _getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
          // None, Constant | UseCompileTimeType
          _setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
          CreateMemberCalls();

          _init = true;
        }
      }

      private static object CreateSharpArgumentInfoArray(params int[] values)
      {
        Type csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName);
        Type csharpArgumentInfoFlags = Type.GetType(CSharpArgumentInfoFlagsTypeName);

        Array a = Array.CreateInstance(csharpArgumentInfoType, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
          MethodInfo createArgumentInfoMethod = csharpArgumentInfoType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { csharpArgumentInfoFlags, typeof(string) }, null);
          object arg = createArgumentInfoMethod.Invoke(null, new object[] { 0, null });
          a.SetValue(arg, i);
        }

        return a;
      }

      private static void CreateMemberCalls()
      {
        Type csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName);
        Type csharpBinderFlagsType = Type.GetType(CSharpBinderFlagsTypeName);
        Type binderType = Type.GetType(BinderTypeName);

        Type csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(csharpArgumentInfoType);

        MethodInfo getMemberMethod = binderType.GetMethod("GetMember", BindingFlags.Public | BindingFlags.Static, null, new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType }, null);
        _getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getMemberMethod);

        MethodInfo setMemberMethod = binderType.GetMethod("SetMember", BindingFlags.Public | BindingFlags.Static, null, new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType }, null);
        _setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(setMemberMethod);
      }

      public static CallSiteBinder GetMember(string name, Type context)
      {
        Init();
        return (CallSiteBinder)_getMemberCall(null, 0, name, context, _getCSharpArgumentInfoArray);
      }

      public static CallSiteBinder SetMember(string name, Type context)
      {
        Init();
        return (CallSiteBinder)_setMemberCall(null, 0, name, context, _setCSharpArgumentInfoArray);
      }
    }

    public static bool TryGetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, out object value)
    {
      ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");

      GetMemberBinder getMemberBinder = (GetMemberBinder) BinderWrapper.GetMember(name, typeof (DynamicUtils));

      CallSite<Func<CallSite, object, object>> callSite = CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember(getMemberBinder));

      object result = callSite.Target(callSite, dynamicProvider);

      if (!ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult))
      {
        value = result;
        return true;
      }
      else
      {
        value = null;
        return false;
      }
    }

    public static bool TrySetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, object value)
    {
      ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");

      SetMemberBinder binder = (SetMemberBinder)BinderWrapper.SetMember(name, typeof(DynamicUtils));

      var setterSite = CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember(binder));

      object result = setterSite.Target(setterSite, dynamicProvider, value);

      return !ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult);
    }

    public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
    {
      DynamicMetaObject metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
      return metaObject.GetDynamicMemberNames();
    }

    internal class NoThrowGetBinderMember : GetMemberBinder
    {
      private readonly GetMemberBinder _innerBinder;

      public NoThrowGetBinderMember(GetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
      {
        _innerBinder = innerBinder;
      }

      public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
      {
        DynamicMetaObject retMetaObject = _innerBinder.Bind(target, new DynamicMetaObject[] { });

        NoThrowExpressionVisitor noThrowVisitor = new NoThrowExpressionVisitor();
        Expression resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        DynamicMetaObject finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
        return finalMetaObject;
      }
    }

    internal class NoThrowSetBinderMember : SetMemberBinder
    {
      private readonly SetMemberBinder _innerBinder;

      public NoThrowSetBinderMember(SetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
      {
        _innerBinder = innerBinder;
      }

      public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
      {
        DynamicMetaObject retMetaObject = _innerBinder.Bind(target, new DynamicMetaObject[] { value });

        NoThrowExpressionVisitor noThrowVisitor = new NoThrowExpressionVisitor();
        Expression resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        DynamicMetaObject finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
        return finalMetaObject;
      }
    }


    internal class NoThrowExpressionVisitor : ExpressionVisitor
    {
      internal static readonly object ErrorResult = new object();

      protected override Expression VisitConditional(ConditionalExpression node)
      {
        // if the result of a test is to throw an error, rewrite to result an error result value
        if (node.IfFalse.NodeType == ExpressionType.Throw)
          return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));

        return base.VisitConditional(node);
      }
    }
  }
}
#endif