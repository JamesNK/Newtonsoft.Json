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

namespace Newtonsoft.Json.Utilities
{
    internal readonly struct StructMultiKey<T1, T2> : IEquatable<StructMultiKey<T1, T2>>
    {
        public readonly T1 Value1;
        public readonly T2 Value2;

        public StructMultiKey(T1 v1, T2 v2)
        {
            Value1 = v1;
            Value2 = v2;
        }

        public override int GetHashCode()
        {
            return (Value1?.GetHashCode() ?? 0) ^ (Value2?.GetHashCode() ?? 0);
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is StructMultiKey<T1, T2> key))
            {
                return false;
            }

            return Equals(key);
        }

        public bool Equals(StructMultiKey<T1, T2> other)
        {
            return (Equals(Value1, other.Value1) && Equals(Value2, other.Value2));
        }
    }
}