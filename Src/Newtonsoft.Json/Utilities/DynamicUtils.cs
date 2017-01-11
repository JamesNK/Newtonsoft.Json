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

#if HAVE_DYNAMIC
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
#if !HAVE_REFLECTION_BINDER
using System.Reflection;
#else
using Microsoft.CSharp.RuntimeBinder;
#endif
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
#if !HAVE_REFLECTION_BINDER
            public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

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
                    {
                        throw new InvalidOperationException("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.".FormatWith(CultureInfo.InvariantCulture, BinderTypeName));
                    }

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
                    MethodInfo createArgumentInfoMethod = csharpArgumentInfoType.GetMethod("Create", new[] { csharpArgumentInfoFlags, typeof(string) });
                    object arg = createArgumentInfoMethod.Invoke(null, new object[] { 0, null });
                    a.SetValue(arg, i);
                }

                return a;
            }

            private static void CreateMemberCalls()
            {
                Type csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName, true);
                Type csharpBinderFlagsType = Type.GetType(CSharpBinderFlagsTypeName, true);
                Type binderType = Type.GetType(BinderTypeName, true);

                Type csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(csharpArgumentInfoType);

                MethodInfo getMemberMethod = binderType.GetMethod("GetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
                _getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getMemberMethod);

                MethodInfo setMemberMethod = binderType.GetMethod("SetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
                _setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(setMemberMethod);
            }
#endif

            public static CallSiteBinder GetMember(string name, Type context)
            {
#if !HAVE_REFLECTION_BINDER
                Init();
                return (CallSiteBinder)_getMemberCall(null, 0, name, context, _getCSharpArgumentInfoArray);
#else
                return Binder.GetMember(
                    CSharpBinderFlags.None, name, context, new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
#endif
            }

            public static CallSiteBinder SetMember(string name, Type context)
            {
#if !HAVE_REFLECTION_BINDER
                Init();
                return (CallSiteBinder)_setMemberCall(null, 0, name, context, _setCSharpArgumentInfoArray);
#else
                return Binder.SetMember(
                    CSharpBinderFlags.None, name, context, new[]
                                                               {
                                                                   CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                                                   CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
                                                               });
#endif
            }
        }

        public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
        {
            DynamicMetaObject metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
            return metaObject.GetDynamicMemberNames();
        }
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
            DynamicMetaObject retMetaObject = _innerBinder.Bind(target, CollectionUtils.ArrayEmpty<DynamicMetaObject>());

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
            {
                return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));
            }

            return base.VisitConditional(node);
        }
    }
}

#endif