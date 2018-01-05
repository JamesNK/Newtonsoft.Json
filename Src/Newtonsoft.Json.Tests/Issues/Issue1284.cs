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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1284
    {
        [Test]
        public void Test()
        {
            var ser = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };
            var b = new B(1, "Hello");

            var a = new A(1.234) { B = b };
            b.As = new List<A> { a };

            var aChild = new AChild(1.45f) { A = a };

            a.Child = aChild;

            string json = JsonConvert.SerializeObject(b, ser);

            Debug.WriteLine(json);

            var deserializedObject = JsonConvert.DeserializeObject<B>(json, ser);

            Assert.IsNotNull(deserializedObject);
            Assert.IsNotNull(deserializedObject.As);
            Assert.AreEqual(1, deserializedObject.As.Count);
            Assert.IsNotNull(deserializedObject.As[0]);
            Assert.IsNotNull(deserializedObject.As[0].B);
            Assert.IsNotNull(deserializedObject.As[0].Child);
            Assert.IsNotNull(deserializedObject.As[0].Child.A);
            Assert.IsNotNull(deserializedObject.As[0]);
        }

        public class B
        {
            public int Id { get; set; }
            private string Message;

            public List<A> As;

            public B(int id, string message)
            {
                Id = id;
                Message = message;
            }

            //public B() // Must be present to deserialize A -> B
            //{

            //}
        }

        public class A
        {
            public double D { get; set; }

            public B B
            {
                get;
                set;
            }

            public AChild Child { get; set; }

            public A(double d)
            {
                D = d;
            }

            //public A() // Must be present to deserialize AChild -> A
            //{

            //}
        }

        public class AChild
        {
            public A A { get; set; }
            public float F { get; set; }

            //public AChild() // Can be removed
            //{

            //}

            public AChild(float f)
            {
                F = f;
            }
        }
    }
}
