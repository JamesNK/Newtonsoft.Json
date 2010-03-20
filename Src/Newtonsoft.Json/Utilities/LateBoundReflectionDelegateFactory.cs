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

using System;
using System.Globalization;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
  internal class LateBoundReflectionDelegateFactory : ReflectionDelegateFactory
  {
    public static readonly LateBoundReflectionDelegateFactory Instance = new LateBoundReflectionDelegateFactory();

    public override MethodCall<T, object> CreateMethodCall<T>(MethodBase method)
    {
      ConstructorInfo c = method as ConstructorInfo;
      if (c != null)
        return (o, a) => c.Invoke(a);

      return (o, a) => method.Invoke(o, a);
    }

    public override Func<T> CreateDefaultConstructor<T>(Type type)
    {
      if (type.IsValueType)
        return () => (T)ReflectionUtils.CreateInstance(type);

      ConstructorInfo constructorInfo = ReflectionUtils.GetDefaultConstructor(type, true);

      return () => (T)constructorInfo.Invoke(null);
    }

    public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
    {
      return o => propertyInfo.GetValue(o, null);
    }

    public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
    {
      return o => fieldInfo.GetValue(o);
    }

    public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
    {
      return (o, v) => fieldInfo.SetValue(o, v);
    }

    public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
    {
      return (o, v) => propertyInfo.SetValue(o, v, null);
    }
  }
}