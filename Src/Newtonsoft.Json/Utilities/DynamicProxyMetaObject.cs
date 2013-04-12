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

#if !(NET35 || NET20 || PORTABLE40)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
  internal sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
  {
    private readonly DynamicProxy<T> _proxy;
    private readonly bool _dontFallbackFirst;

    internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy, bool dontFallbackFirst)
      : base(expression, BindingRestrictions.Empty, value)
    {
      _proxy = proxy;
      _dontFallbackFirst = dontFallbackFirst;
    }

    private new T Value { get { return (T)base.Value; } }

    private bool IsOverridden(string method)
    {
      return ReflectionUtils.IsMethodOverridden(_proxy.GetType(), typeof (DynamicProxy<T>), method);
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
      return IsOverridden("TryGetMember")
           ? CallMethodWithResult("TryGetMember", binder, NoArgs, e => binder.FallbackGetMember(this, e))
           : base.BindGetMember(binder);
    }

    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
    {
      return IsOverridden("TrySetMember")
           ? CallMethodReturnLast("TrySetMember", binder, GetArgs(value), e => binder.FallbackSetMember(this, value, e))
           : base.BindSetMember(binder, value);
    }

    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
    {
      return IsOverridden("TryDeleteMember")
           ? CallMethodNoResult("TryDeleteMember", binder, NoArgs, e => binder.FallbackDeleteMember(this, e))
           : base.BindDeleteMember(binder);
    }


    public override DynamicMetaObject BindConvert(ConvertBinder binder)
    {
      return IsOverridden("TryConvert")
           ? CallMethodWithResult("TryConvert", binder, NoArgs, e => binder.FallbackConvert(this, e))
           : base.BindConvert(binder);
    }

    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
      if (!IsOverridden("TryInvokeMember"))
        return base.BindInvokeMember(binder, args);

      //
      // Generate a tree like:
      //
      // {
      //   object result;
      //   TryInvokeMember(payload, out result)
      //      ? result
      //      : TryGetMember(payload, out result)
      //          ? FallbackInvoke(result)
      //          : fallbackResult
      // }
      //
      // Then it calls FallbackInvokeMember with this tree as the
      // "error", giving the language the option of using this
      // tree or doing .NET binding.
      //
      Fallback fallback = e => binder.FallbackInvokeMember(this, args, e);

      DynamicMetaObject call = BuildCallMethodWithResult(
          "TryInvokeMember",
          binder,
          GetArgArray(args),
          BuildCallMethodWithResult(
              "TryGetMember",
              new GetBinderAdapter(binder),
              NoArgs,
              fallback(null),
              e => binder.FallbackInvoke(e, args, null)
          ),
          null
      );

      return _dontFallbackFirst ? call : fallback(call);
    }


    public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
    {
      return IsOverridden("TryCreateInstance")
           ? CallMethodWithResult("TryCreateInstance", binder, GetArgArray(args), e => binder.FallbackCreateInstance(this, args, e))
           : base.BindCreateInstance(binder, args);
    }

    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
    {
      return IsOverridden("TryInvoke")
        ? CallMethodWithResult("TryInvoke", binder, GetArgArray(args), e => binder.FallbackInvoke(this, args, e))
        : base.BindInvoke(binder, args);
    }

    public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
    {
      return IsOverridden("TryBinaryOperation")
        ? CallMethodWithResult("TryBinaryOperation", binder, GetArgs(arg), e => binder.FallbackBinaryOperation(this, arg, e))
        : base.BindBinaryOperation(binder, arg);
    }

    public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
    {
      return IsOverridden("TryUnaryOperation")
           ? CallMethodWithResult("TryUnaryOperation", binder, NoArgs, e => binder.FallbackUnaryOperation(this, e))
           : base.BindUnaryOperation(binder);
    }

    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
    {
      return IsOverridden("TryGetIndex")
           ? CallMethodWithResult("TryGetIndex", binder, GetArgArray(indexes), e => binder.FallbackGetIndex(this, indexes, e))
           : base.BindGetIndex(binder, indexes);
    }

    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
    {
      return IsOverridden("TrySetIndex")
           ? CallMethodReturnLast("TrySetIndex", binder, GetArgArray(indexes, value), e => binder.FallbackSetIndex(this, indexes, value, e))
           : base.BindSetIndex(binder, indexes, value);
    }

    public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
    {
      return IsOverridden("TryDeleteIndex")
           ? CallMethodNoResult("TryDeleteIndex", binder, GetArgArray(indexes), e => binder.FallbackDeleteIndex(this, indexes, e))
           : base.BindDeleteIndex(binder, indexes);
    }

    private delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

    private readonly static Expression[] NoArgs = new Expression[0];

    private static Expression[] GetArgs(params DynamicMetaObject[] args)
    {
      return args.Select(arg => Expression.Convert(arg.Expression, typeof(object))).ToArray();
    }

    private static Expression[] GetArgArray(DynamicMetaObject[] args)
    {
      return new[] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
    }

    private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
    {
      return new Expression[]
            {
                Expression.NewArrayInit(typeof(object), GetArgs(args)),
                Expression.Convert(value.Expression, typeof(object))
            };
    }

    private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
    {
      Type t = binder.GetType();
      while (!t.IsVisible())
        t = t.BaseType();
      return Expression.Constant(binder, t);
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic that returns a result
    /// </summary>
    private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback, Fallback fallbackInvoke = null)
    {
      //
      // First, call fallback to do default binding
      // This produces either an error or a call to a .NET member
      //
      DynamicMetaObject fallbackResult = fallback(null);

      DynamicMetaObject callDynamic = BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);

      //
      // Now, call fallback again using our new MO as the error
      // When we do this, one of two things can happen:
      //   1. Binding will succeed, and it will ignore our call to
      //      the dynamic method, OR
      //   2. Binding will fail, and it will use the MO we created
      //      above.
      //

      return _dontFallbackFirst ? callDynamic : fallback(callDynamic);
    }

    private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicMetaObject fallbackResult, Fallback fallbackInvoke)
    {
      //
      // Build a new expression like:
      // {
      //   object result;
      //   TryGetMember(payload, out result) ? fallbackInvoke(result) : fallbackResult
      // }
      //
      ParameterExpression result = Expression.Parameter(typeof(object), null);

      IList<Expression> callArgs = new List<Expression>();
      callArgs.Add(Expression.Convert(Expression, typeof(T)));
      callArgs.Add(Constant(binder));
      callArgs.AddRange(args);
      callArgs.Add(result);

      DynamicMetaObject resultMetaObject = new DynamicMetaObject(result, BindingRestrictions.Empty);

      // Need to add a conversion if calling TryConvert
      if (binder.ReturnType != typeof (object))
      {
        UnaryExpression convert = Expression.Convert(resultMetaObject.Expression, binder.ReturnType);
        // will always be a cast or unbox

        resultMetaObject = new DynamicMetaObject(convert, resultMetaObject.Restrictions);
      }

      if (fallbackInvoke != null)
        resultMetaObject = fallbackInvoke(resultMetaObject);

      DynamicMetaObject callDynamic = new DynamicMetaObject(
        Expression.Block(
          new[] {result},
          Expression.Condition(
            Expression.Call(
              Expression.Constant(_proxy),
              typeof(DynamicProxy<T>).GetMethod(methodName),
              callArgs
              ),
            resultMetaObject.Expression,
            fallbackResult.Expression,
            binder.ReturnType
            )
          ),
        GetRestrictions().Merge(resultMetaObject.Restrictions).Merge(fallbackResult.Restrictions)
        );

      return callDynamic;
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic, but uses one of the arguments for
    /// the result.
    /// </summary>
    private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
    {
      //
      // First, call fallback to do default binding
      // This produces either an error or a call to a .NET member
      //
      DynamicMetaObject fallbackResult = fallback(null);

      //
      // Build a new expression like:
      // {
      //   object result;
      //   TrySetMember(payload, result = value) ? result : fallbackResult
      // }
      //
      ParameterExpression result = Expression.Parameter(typeof(object), null);

      IList<Expression> callArgs = new List<Expression>();
      callArgs.Add(Expression.Convert(Expression, typeof (T)));
      callArgs.Add(Constant(binder));
      callArgs.AddRange(args);
      callArgs[args.Length + 1] = Expression.Assign(result, callArgs[args.Length + 1]);

      DynamicMetaObject callDynamic = new DynamicMetaObject(
          Expression.Block(
              new[] { result },
              Expression.Condition(
                  Expression.Call(
                      Expression.Constant(_proxy),
                      typeof(DynamicProxy<T>).GetMethod(methodName),
                      callArgs
                  ),
                  result,
                  fallbackResult.Expression,
                  typeof(object)
              )
          ),
          GetRestrictions().Merge(fallbackResult.Restrictions)
      );

      //
      // Now, call fallback again using our new MO as the error
      // When we do this, one of two things can happen:
      //   1. Binding will succeed, and it will ignore our call to
      //      the dynamic method, OR
      //   2. Binding will fail, and it will use the MO we created
      //      above.
      //
      return _dontFallbackFirst ? callDynamic : fallback(callDynamic);
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic, but uses one of the arguments for
    /// the result.
    /// </summary>
    private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
    {
      //
      // First, call fallback to do default binding
      // This produces either an error or a call to a .NET member
      //
      DynamicMetaObject fallbackResult = fallback(null);

      IList<Expression> callArgs = new List<Expression>();
      callArgs.Add(Expression.Convert(Expression, typeof(T)));
      callArgs.Add(Constant(binder));
      callArgs.AddRange(args);

      //
      // Build a new expression like:
      //   if (TryDeleteMember(payload)) { } else { fallbackResult }
      //
      DynamicMetaObject callDynamic = new DynamicMetaObject(
        Expression.Condition(
          Expression.Call(
            Expression.Constant(_proxy),
            typeof(DynamicProxy<T>).GetMethod(methodName),
            callArgs
            ),
          Expression.Empty(),
          fallbackResult.Expression,
          typeof (void)
          ),
        GetRestrictions().Merge(fallbackResult.Restrictions)
        );

      //
      // Now, call fallback again using our new MO as the error
      // When we do this, one of two things can happen:
      //   1. Binding will succeed, and it will ignore our call to
      //      the dynamic method, OR
      //   2. Binding will fail, and it will use the MO we created
      //      above.
      //
      return _dontFallbackFirst ? callDynamic : fallback(callDynamic);
    }

    /// <summary>
    /// Returns a Restrictions object which includes our current restrictions merged
    /// with a restriction limiting our type
    /// </summary>
    private BindingRestrictions GetRestrictions()
    {
      return (Value == null && HasValue)
           ? BindingRestrictions.GetInstanceRestriction(Expression, null)
           : BindingRestrictions.GetTypeRestriction(Expression, LimitType);
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return _proxy.GetDynamicMemberNames(Value);
    }

    // It is okay to throw NotSupported from this binder. This object
    // is only used by DynamicObject.GetMember--it is not expected to
    // (and cannot) implement binding semantics. It is just so the DO
    // can use the Name and IgnoreCase properties.
    private sealed class GetBinderAdapter : GetMemberBinder
    {
      internal GetBinderAdapter(InvokeMemberBinder binder) :
        base(binder.Name, binder.IgnoreCase)
      {
      }

      public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
      {
        throw new NotSupportedException();
      }
    }
  }
}
#endif