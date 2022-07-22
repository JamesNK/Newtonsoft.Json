﻿#region License
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

using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(NET20 || DNXCORE50) || NETSTANDARD2_0 || NET6_0_OR_GREATER
    [DataContract]
    public class NullableStructPropertyClass
    {
        private StructISerializable _foo1;
        private StructISerializable? _foo2;

        [DataMember]
        public StructISerializable Foo1
        {
            get { return _foo1; }
            set { _foo1 = value; }
        }

        [DataMember]
        public StructISerializable? Foo2
        {
            get { return _foo2; }
            set { _foo2 = value; }
        }
    }
#endif
}
