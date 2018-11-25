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

#if (NETSTANDARD2_0)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using System.Text;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1757 : TestFixtureBase
    {
        [Test]
        public void Test_Serialize()
        {
            JsonConvert.SerializeObject(new TestObject());
        }

        [Test]
        public void Test_SerializeEncoding()
        {
            JsonConvert.SerializeObject(Encoding.UTF8);
        }

        [Test]
        public void Test_Deserialize()
        {
            JsonConvert.DeserializeObject<TestObject>(@"{'Room':{},'RefLike':{}}");
        }

        public class TestObject
        {
            public Span<int> this[int i]
            {
                get { return default(Span<int>); }
                set { DoNothing(value); }
            }
            public static Span<int> Space
            {
                get { return default(Span<int>); }
                set { DoNothing(value); }
            }
            public Span<int> Room
            {
                get { return default(Span<int>); }
                set { DoNothing(value); }
            }
            public MyByRefLikeType RefLike
            {
                get { return default(MyByRefLikeType); }
                set { }
            }
            private static void DoNothing(Span<int> param)
            {
                throw new InvalidOperationException("Should never be called.");
            }
            public string PrintMySpan(string str, Span<int> mySpan = default)
            {
                return str;
            }

            public Span<int> GetSpan(int[] array)
            {
                return array.AsSpan();
            }
        }

        public ref struct MyByRefLikeType
        {
            public MyByRefLikeType(int i) { }
            public static int Index;
        }
    }
}
#endif