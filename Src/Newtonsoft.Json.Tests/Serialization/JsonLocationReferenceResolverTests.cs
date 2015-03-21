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
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    public class CircularDictionary : Dictionary<string, CircularDictionary>
    {
    }

    public class CircularList : List<CircularList>
    {
    }

    [TestFixture]
    public class JsonLocationReferenceResolverTests : TestFixtureBase
    {
        public class Parent
        {
            public Child ReadOnlyChild
            {
                get { return Child1; }
            }

            public Child Child1 { get; set; }
            public Child Child2 { get; set; }

            public IList<string> ReadOnlyList
            {
                get { return List1; }
            }

            public IList<string> List1 { get; set; }
            public IList<string> List2 { get; set; }
        }

        public class Child
        {
            public string PropertyName { get; set; }
        }

        [Test]
        public void SerializeReadOnlyProperty()
        {
            Child c = new Child
            {
                PropertyName = "value?"
            };
            IList<string> l = new List<string>
            {
                "value!"
            };
            Parent p = new Parent
            {
                Child1 = c,
                Child2 = c,
                List1 = l,
                List2 = l
            };

            string json = JsonConvert.SerializeObject(p, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceResolver = new JsonLocationReferenceResolver()
            });

            Console.WriteLine(json);

            StringAssert.AreEqual(@"{
  ""ReadOnlyChild"": {
    ""PropertyName"": ""value?""
  },
  ""Child1"": {
    ""PropertyName"": ""value?""
  },
  ""Child2"": {
    ""$ref"": ""#/Child1""
  },
  ""ReadOnlyList"": [
    ""value!""
  ],
  ""List1"": [
    ""value!""
  ],
  ""List2"": {
    ""$ref"": ""#/List1""
  }
}", json);
        }

        public void DeserializeReadOnlyProperty()
        {
            string json = @"{
  ""ReadOnlyChild"": {
    ""PropertyName"": ""value?""
  },
  ""Child1"": {
    ""PropertyName"": ""value?""
  },
  ""Child2"": {
    ""$ref"": ""#/Child1""
  },
  ""ReadOnlyList"": [
    ""value!""
  ],
  ""List1"": [
    ""value!""
  ],
  ""List2"": {
    ""$ref"": ""#/List1""
  }
}";

            Parent newP = JsonConvert.DeserializeObject<Parent>(json, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceResolver = new JsonLocationReferenceResolver()
            });

            Assert.AreEqual("value?", newP.Child1.PropertyName);
            Assert.AreEqual(newP.Child1, newP.Child2);
            Assert.AreEqual(newP.Child1, newP.ReadOnlyChild);

            Assert.AreEqual("value!", newP.List1[0]);
            Assert.AreEqual(newP.List1, newP.List2);
            Assert.AreEqual(newP.List1, newP.ReadOnlyList);
        }

        public void DeserializeWithEmptyObject()
        {
            string json = @"{
  ""ReadOnlyChild"": {
    ""PropertyName"": ""value?""
  },
  ""Child1"": {
  },
  ""Child2"": {
    ""$ref"": ""#/Child1""
  },
  ""ReadOnlyList"": [
    ""value!""
  ],
  ""List1"": [
    ""value!""
  ],
  ""List2"": {
    ""$ref"": ""#/List1""
  }
}";

            Parent newP = JsonConvert.DeserializeObject<Parent>(json, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceResolver = new JsonLocationReferenceResolver()
            });

            Assert.AreEqual("value?", newP.Child1.PropertyName);
            Assert.AreEqual(newP.Child1, newP.Child2);
            Assert.AreEqual(newP.Child1, newP.ReadOnlyChild);

            Assert.AreEqual("value!", newP.List1[0]);
            Assert.AreEqual(newP.List1, newP.List2);
            Assert.AreEqual(newP.List1, newP.ReadOnlyList);
        }
    }
}