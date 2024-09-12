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
using System.Collections.Generic;
using System.Dynamic;

namespace Newtonsoft.Json.Utilities
{
    internal class DynamicProxy<T>
    {
        public virtual IEnumerable<string> GetDynamicMemberNames(T instance)
        {
            return CollectionUtils.ArrayEmpty<string>();
        }

        public virtual bool TryBinaryOperation(T instance, BinaryOperationBinder binder, object arg, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryConvert(T instance, ConvertBinder binder, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryCreateInstance(T instance, CreateInstanceBinder binder, object[] args, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryDeleteIndex(T instance, DeleteIndexBinder binder, object[] indexes)
        {
            return false;
        }

        public virtual bool TryDeleteMember(T instance, DeleteMemberBinder binder)
        {
            return false;
        }

        public virtual bool TryGetIndex(T instance, GetIndexBinder binder, object[] indexes, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryGetMember(T instance, GetMemberBinder binder, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvoke(T instance, InvokeBinder binder, object[] args, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvokeMember(T instance, InvokeMemberBinder binder, object[] args, out object? result)
        {
            result = null;
            return false;
        }

        public virtual bool TrySetIndex(T instance, SetIndexBinder binder, object[] indexes, object value)
        {
            return false;
        }

        public virtual bool TrySetMember(T instance, SetMemberBinder binder, object value)
        {
            return false;
        }

        public virtual bool TryUnaryOperation(T instance, UnaryOperationBinder binder, out object? result)
        {
            result = null;
            return false;
        }
    }
}

#endif