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

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
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
    public class Issue0198 : TestFixtureBase
    {
        [Test]
        public void Test_List()
        {
            IEnumerable<TestClass1> objects = new List<TestClass1>
            {
                new TestClass1
                {
                    Prop1 = new HashSet<TestClass2>
                    {
                        new TestClass2
                        {
                            MyProperty1 = "Test1",
                            MyProperty2 = "Test2",
                        }
                    },
                    Prop2 = new List<string>
                    {
                        "Test1",
                        "Test1"
                    },
                    Prop3 = new HashSet<TestClass2>
                    {
                        new TestClass2
                        {
                            MyProperty1 = "Test1",
                            MyProperty2 = "Test2",
                        }
                    },
                }
            };

            string serializedData = JsonConvert.SerializeObject(objects, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

            IEnumerable<TestClass1> a = JsonConvert.DeserializeObject<IEnumerable<TestClass1>>(serializedData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            TestClass1 o = a.First();

            Assert.AreEqual(1, o.Prop1.Count);
            Assert.AreEqual(1, o.Prop2.Count);
            Assert.AreEqual(1, o.Prop3.Count);
        }

        [Test]
        public void Test_Collection()
        {
            TestClass3 c = new TestClass3();
            c.Prop1 = new Dictionary<string, string>
            {
                ["key"] = "value"
            };

            string serializedData = JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

            TestClass3 a = JsonConvert.DeserializeObject<TestClass3>(serializedData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Assert.AreEqual(1, a.Prop1.Count);
        }

        class TestClass1 : AbstactClass
        {
            public TestClass1()
            {
                Prop1 = new HashSet<TestClass2>();
                Prop2 = new HashSet<string>();
            }

            public ICollection<TestClass2> Prop1 { get; set; }
            public ICollection<string> Prop2 { get; set; }
        }

        class TestClass2
        {
            public string MyProperty1 { get; set; }
            public string MyProperty2 { get; set; }
        }

        abstract class AbstactClass
        {
            public ICollection<TestClass2> Prop3 { get; set; } = new List<TestClass2>();
        }

        class TestClass3
        {
            public TestClass3()
            {
                Prop1 = new ModelStateDictionary<string>();
            }

            public IDictionary<string, string> Prop1 { get; set; }
        }
    }
}
#endif