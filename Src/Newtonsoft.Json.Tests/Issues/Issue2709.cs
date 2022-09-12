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

#if !NET20
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.Documentation.Samples.Serializer;
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2709 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            int maxDepth = 512;

            var currentFoo = new Foo(null);

            for (var i = 0; i < 100; i++)
            {
                currentFoo = new Foo(currentFoo);
            }

            var fooBar = new FooBar();
            fooBar.AddFoo("main", currentFoo);

            var json = JsonConvert.SerializeObject(fooBar, SerializeSettings(maxDepth));

            JsonConvert.DeserializeObject<FooBar>(json, DeserializeSettings(maxDepth));
        }

        [Test]
        public void Test_Failure()
        {
            int maxDepth = 512;

            var currentFoo = new Foo(null);

            for (var i = 0; i < 600; i++)
            {
                currentFoo = new Foo(currentFoo);
            }

            var fooBar = new FooBar();
            fooBar.AddFoo("main", currentFoo);

            var json = JsonConvert.SerializeObject(fooBar, SerializeSettings(maxDepth));

            var ex = ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<FooBar>(json, DeserializeSettings(maxDepth)));
            Assert.IsTrue(ex.Message.StartsWith("The reader's MaxDepth of 512 has been exceeded."));
        }

        [Test]
        public void Test_Failure2()
        {
            int maxDepth = 10;

            var currentFoo = new Foo(null);

            for (var i = 0; i < 20; i++)
            {
                currentFoo = new Foo(currentFoo);
            }

            var fooBar = new FooBar();
            fooBar.AddFoo("main", currentFoo);

            var json = JsonConvert.SerializeObject(fooBar, SerializeSettings(maxDepth));

            var ex = ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<FooBar>(json, DeserializeSettings(maxDepth)));
            Assert.IsTrue(ex.Message.StartsWith("The reader's MaxDepth of 10 has been exceeded."));
        }

        [Serializable]
        public class FooBar : ISerializable
        {
            private Dictionary<string, Foo> _myData = new Dictionary<string, Foo>();

            public IList<Foo> FooList => _myData.Values.ToList();

            public FooBar()
            {
            }

            public FooBar(SerializationInfo info, StreamingContext context)
            {
                _myData = (Dictionary<string, Foo>)info.GetValue(nameof(_myData), typeof(Dictionary<string, Foo>));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(nameof(_myData), _myData);
            }

            public void AddFoo(string name, Foo myFoo)
            {
                _myData[name] = myFoo;
            }
        }

        public class Foo
        {
            public Guid Id { get; }
            public Foo MyFoo { get; set; }

            public Foo(Foo myFoo)
            {
                MyFoo = myFoo;
                Id = Guid.NewGuid();
            }
        }

        private JsonSerializerSettings DeserializeSettings(int maxDepth) => new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = maxDepth
        };

        private JsonSerializerSettings SerializeSettings(int maxDepth) => new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            MaxDepth = maxDepth
        };
    }
}
#endif
