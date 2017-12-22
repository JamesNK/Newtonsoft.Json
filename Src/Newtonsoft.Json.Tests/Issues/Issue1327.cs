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

#if !(DNXCORE50 || PORTABLE40) || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
#if !NET20
using System.Xml.Linq;
#endif
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
    public class Issue1327 : TestFixtureBase
    {
#if !PORTABLE || NETSTANDARD2_0
        public class PersonWithXmlNode
        {
            public XmlNode TestXml { get; set; }

            public string Name { get; set; }

            public int IdNumber { get; set; }
        }
#endif

#if !NET20
        public class PersonWithXObject
        {
            public XObject TestXml1 { get; set; }
            public XNode TestXml2 { get; set; }
            public XContainer TestXml3 { get; set; }

            public string Name { get; set; }

            public int IdNumber { get; set; }
        }
#endif

#if !PORTABLE || NETSTANDARD2_0
        [Test]
        public void Test_XmlNode()
        {
            string json = @"{
  ""TestXml"": {
    ""orders"": {
      ""order"": {
        ""id"": ""550268"",
        ""name"": ""vinoth""
      }
    }
  },
  ""Name"": ""Kumar"",
  ""IdNumber"": 990268
}";

            var p = JsonConvert.DeserializeObject<PersonWithXmlNode>(json);

            Assert.AreEqual("Kumar", p.Name);
            Assert.AreEqual("vinoth", p.TestXml.SelectSingleNode("//name").InnerText);
        }
#endif

#if !NET20
        [Test]
        public void Test_XObject()
        {
            string json = @"{
  ""TestXml1"": {
    ""orders"": {
      ""order"": {
        ""id"": ""550268"",
        ""name"": ""vinoth""
      }
    }
  },
  ""TestXml2"": {
    ""orders"": {
      ""order"": {
        ""id"": ""550268"",
        ""name"": ""vinoth""
      }
    }
  },
  ""TestXml3"": {
    ""orders"": {
      ""order"": {
        ""id"": ""550268"",
        ""name"": ""vinoth""
      }
    }
  },
  ""Name"": ""Kumar"",
  ""IdNumber"": 990268
}";

            var p = JsonConvert.DeserializeObject<PersonWithXObject>(json);

            Assert.AreEqual("Kumar", p.Name);
            Assert.AreEqual("vinoth", (string)((XDocument)p.TestXml1).Root.Element("order").Element("name"));
            Assert.AreEqual("vinoth", (string)((XDocument)p.TestXml2).Root.Element("order").Element("name"));
            Assert.AreEqual("vinoth", (string)((XDocument)p.TestXml3).Root.Element("order").Element("name"));
        }
#endif
    }
}
#endif