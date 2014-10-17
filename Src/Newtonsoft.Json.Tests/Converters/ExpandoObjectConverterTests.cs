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

#if !(NET35 || NET20 || PORTABLE40)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
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

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class ExpandoObjectConverterTests : TestFixtureBase
    {
        public class ExpandoContainer
        {
            public string Before { get; set; }
            public ExpandoObject Expando { get; set; }
            public string After { get; set; }
        }

        [Test]
        public void SerializeExpandoObject()
        {
            ExpandoContainer d = new ExpandoContainer
            {
                Before = "Before!",
                Expando = new ExpandoObject(),
                After = "After!"
            };

            dynamic o = d.Expando;

            o.String = "String!";
            o.Integer = 234;
            o.Float = 1.23d;
            o.List = new List<string> { "First", "Second", "Third" };
            o.Object = new Dictionary<string, object>
            {
                { "First", 1 }
            };

            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}", json);
        }

        [Test]
        public void SerializeNullExpandoObject()
        {
            ExpandoContainer d = new ExpandoContainer();

            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}", json);
        }

        [Test]
        public void DeserializeExpandoObject()
        {
            string json = @"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}";

            ExpandoContainer o = JsonConvert.DeserializeObject<ExpandoContainer>(json);

            Assert.AreEqual(o.Before, "Before!");
            Assert.AreEqual(o.After, "After!");
            Assert.IsNotNull(o.Expando);

            dynamic d = o.Expando;
            CustomAssert.IsInstanceOfType(typeof(ExpandoObject), d);

            Assert.AreEqual("String!", d.String);
            CustomAssert.IsInstanceOfType(typeof(string), d.String);

            Assert.AreEqual(234, d.Integer);
            CustomAssert.IsInstanceOfType(typeof(long), d.Integer);

            Assert.AreEqual(1.23, d.Float);
            CustomAssert.IsInstanceOfType(typeof(double), d.Float);

            Assert.IsNotNull(d.List);
            Assert.AreEqual(3, d.List.Count);
            CustomAssert.IsInstanceOfType(typeof(List<object>), d.List);

            Assert.AreEqual("First", d.List[0]);
            CustomAssert.IsInstanceOfType(typeof(string), d.List[0]);

            Assert.AreEqual("Second", d.List[1]);
            Assert.AreEqual("Third", d.List[2]);

            Assert.IsNotNull(d.Object);
            CustomAssert.IsInstanceOfType(typeof(ExpandoObject), d.Object);

            Assert.AreEqual(1, d.Object.First);
            CustomAssert.IsInstanceOfType(typeof(long), d.Object.First);
        }

        [Test]
        public void DeserializeNullExpandoObject()
        {
            string json = @"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}";

            ExpandoContainer c = JsonConvert.DeserializeObject<ExpandoContainer>(json);

            Assert.AreEqual(null, c.Expando);
        }
    }
}

#endif