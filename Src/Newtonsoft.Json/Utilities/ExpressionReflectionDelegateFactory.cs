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

#if !(PORTABLE40 || NET20 || NET35)
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
    internal class ExpressionReflectionDelegateFactory : ReflectionDelegateFactory
    {
        private static readonly ExpressionReflectionDelegateFactory _instance = new ExpressionReflectionDelegateFactory();

        internal static ReflectionDelegateFactory Instance
        {
            get { return _instance; }
        }

        public override MethodCall<T, object> CreateMethodCall<T>(MethodBase method)
        {
            ValidationUtils.ArgumentNotNull(method, "method");

            Type type = typeof(object);

            ParameterExpression targetParameterExpression = Expression.Parameter(type, "target");
            ParameterExpression argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

            ParameterInfo[] parametersInfo = method.GetParameters();

            Expression[] argsExpression = new Expression[parametersInfo.Length];

            for (int i = 0; i < parametersInfo.Length; i++)
            {
                Expression indexExpression = Expression.Constant(i);

                Expression paramAccessorExpression = Expression.ArrayIndex(argsParameterExpression, indexExpression);

                paramAccessorExpression = EnsureCastExpression(paramAccessorExpression, parametersInfo[i].ParameterType);

                argsExpression[i] = paramAccessorExpression;
            }

            Expression callExpression;
            if (method.IsConstructor)
            {
                callExpression = Expression.New((ConstructorInfo)method, argsExpression);
            }
            else if (method.IsStatic)
            {
                callExpression = Expression.Call((MethodInfo)method, argsExpression);
            }
            else
            {
                Expression readParameter = EnsureCastExpression(targetParameterExpression, method.DeclaringType);

                callExpression = Expression.Call(readParameter, (MethodInfo)method, argsExpression);
            }

            if (method is MethodInfo)
            {
                MethodInfo m = (MethodInfo)method;
                if (m.ReturnType != typeof(void))
                    callExpression = EnsureCastExpression(callExpression, type);
                else
                    callExpression = Expression.Block(callExpression, Expression.Constant(null));
            }
            else
            {
                callExpression = EnsureCastExpression(callExpression, type);
            }

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(MethodCall<T, object>), callExpression, targetParameterExpression, argsParameterExpression);

            MethodCall<T, object> compiled = (MethodCall<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        public override Func<T> CreateDefaultConstructor<T>(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, "type");

            // avoid error from expressions compiler because of abstract class
            if (type.IsAbstract())
                return () => (T)Activator.CreateInstance(type);

            try
            {
                Type resultType = typeof(T);

                Expression expression = Expression.New(type);

                expression = EnsureCastExpression(expression, resultType);

                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);

                Func<T> compiled = (Func<T>)lambdaExpression.Compile();
                return compiled;
            }
            catch
            {
                // an error can be thrown if constructor is not valid on Win8
                // will have INVOCATION_FLAGS_NON_W8P_FX_API invocation flag
                return () => (T)Activator.CreateInstance(type);
            }
        }

        public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");

            Type instanceType = typeof(T);
            Type resultType = typeof(object);

            ParameterExpression parameterExpression = Expression.Parameter(instanceType, "instance");
            Expression resultExpression;

            MethodInfo getMethod = propertyInfo.GetGetMethod(true);

            if (getMethod.IsStatic)
            {
                resultExpression = Expression.MakeMemberAccess(null, propertyInfo);
            }
            else
            {
                Expression readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType);

                resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
            }

            resultExpression = EnsureCastExpression(resultExpression, resultType);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T, object>), resultExpression, parameterExpression);

            Func<T, object> compiled = (Func<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
        {
            ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");

            ParameterExpression sourceParameter = Expression.Parameter(typeof(T), "source");

            Expression fieldExpression;
            if (fieldInfo.IsStatic)
            {
                fieldExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression sourceExpression = EnsureCastExpression(sourceParameter, fieldInfo.DeclaringType);

                fieldExpression = Expression.Field(sourceExpression, fieldInfo);
            }

            fieldExpression = EnsureCastExpression(fieldExpression, typeof(object));

            Func<T, object> compiled = Expression.Lambda<Func<T, object>>(fieldExpression, sourceParameter).Compile();
            return compiled;
        }

        public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
        {
            ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");

            // use reflection for structs
            // expression doesn't correctly set value
            if (fieldInfo.DeclaringType.IsValueType() || fieldInfo.IsInitOnly)
                return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(fieldInfo);

            ParameterExpression sourceParameterExpression = Expression.Parameter(typeof(T), "source");
            ParameterExpression valueParameterExpression = Expression.Parameter(typeof(object), "value");

            Expression fieldExpression;
            if (fieldInfo.IsStatic)
            {
                fieldExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression sourceExpression = EnsureCastExpression(sourceParameterExpression, fieldInfo.DeclaringType);

                fieldExpression = Expression.Field(sourceExpression, fieldInfo);
            }

            Expression valueExpression = EnsureCastExpression(valueParameterExpression, fieldExpression.Type);

            BinaryExpression assignExpression = Expression.Assign(fieldExpression, valueExpression);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object>), assignExpression, sourceParameterExpression, valueParameterExpression);

            Action<T, object> compiled = (Action<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");

            // use reflection for structs
            // expression doesn't correctly set value
            if (propertyInfo.DeclaringType.IsValueType())
                return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(propertyInfo);

            Type instanceType = typeof(T);
            Type valueType = typeof(object);

            ParameterExpression instanceParameter = Expression.Parameter(instanceType, "instance");

            ParameterExpression valueParameter = Expression.Parameter(valueType, "value");
            Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);

            MethodInfo setMethod = propertyInfo.GetSetMethod(true);

            Expression setExpression;
            if (setMethod.IsStatic)
            {
                setExpression = Expression.Call(setMethod, readValueParameter);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType);

                setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
            }

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object>), setExpression, instanceParameter, valueParameter);

            Action<T, object> compiled = (Action<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        private Expression EnsureCastExpression(Expression expression, Type targetType)
        {
            Type expressionType = expression.Type;

            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType() && targetType.IsAssignableFrom(expressionType)))
                return expression;

            return Expression.Convert(expression, targetType);
        }
    }
}

#endif