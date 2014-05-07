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
using Newtonsoft.Json.Serialization;

#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif

namespace Newtonsoft.Json.Utilities
{
    internal abstract class ReflectionDelegateFactory
    {
        public Func<T, object> CreateGet<T>(MemberInfo memberInfo)
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return CreateGet<T>(propertyInfo);

            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return CreateGet<T>(fieldInfo);

            throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
        }

        public Action<T, object> CreateSet<T>(MemberInfo memberInfo)
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return CreateSet<T>(propertyInfo);

            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return CreateSet<T>(fieldInfo);

            throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
        }

        public abstract MethodCall<T, object> CreateMethodCall<T>(MethodBase method);
        public abstract ObjectConstructor<object> CreateParametrizedConstructor(MethodBase method);
        public abstract Func<T> CreateDefaultConstructor<T>(Type type);
        public abstract Func<T, object> CreateGet<T>(PropertyInfo propertyInfo);
        public abstract Func<T, object> CreateGet<T>(FieldInfo fieldInfo);
        public abstract Action<T, object> CreateSet<T>(FieldInfo fieldInfo);
        public abstract Action<T, object> CreateSet<T>(PropertyInfo propertyInfo);
    }
}