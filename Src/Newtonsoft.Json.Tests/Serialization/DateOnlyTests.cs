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

#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
#if !NET20
using System.Xml.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using Xunit.Abstractions;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class DateOnlyTests : TestFixtureBase
    {
        [Test]
        public void Serialize()
        {
            DateOnly d = new DateOnly(2000, 12, 29);
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"""2000-12-29""", json);
        }

        [Test]
        public void SerializeDefault()
        {
            DateOnly d = default;
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"""0001-01-01""", json);
        }

        [Test]
        public void SerializeMaxValue()
        {
            DateOnly d = DateOnly.MaxValue;
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"""9999-12-31""", json);
        }

        [Test]
        public void SerializeMinValue()
        {
            DateOnly d = DateOnly.MinValue;
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"""0001-01-01""", json);
        }

        [Test]
        public void SerializeNullable_Null()
        {
            DateOnly? d = default;
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual("null", json);
        }

        [Test]
        public void SerializeNullable_Value()
        {
            DateOnly? d = new DateOnly(2000, 12, 29);
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"""2000-12-29""", json);
        }

        [Test]
        public void SerializeList()
        {
            IList<DateOnly> d = new List<DateOnly>
            {
                new DateOnly(2000, 12, 29)
            };
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"[
  ""2000-12-29""
]", json);
        }

        [Test]
        public void SerializeList_Nullable()
        {
            IList<DateOnly?> d = new List<DateOnly?>
            {
                new DateOnly(2000, 12, 29),
                null
            };
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"[
  ""2000-12-29"",
  null
]", json);
        }

        [Test]
        public void Deserialize()
        {
            DateOnly d = JsonConvert.DeserializeObject<DateOnly>(@"""2000-12-29""");

            Assert.AreEqual(new DateOnly(2000, 12, 29), d);
        }

        [Test]
        public void DeserializeDefault()
        {
            DateOnly d = JsonConvert.DeserializeObject<DateOnly>(@"""0001-01-01""");

            Assert.AreEqual(default(DateOnly), d);
        }

        [Test]
        public void DeserializeMaxValue()
        {
            DateOnly d = JsonConvert.DeserializeObject<DateOnly>(@"""9999-12-31""");

            Assert.AreEqual(DateOnly.MaxValue, d);
        }

        [Test]
        public void DeserializeMinValue()
        {
            DateOnly d = JsonConvert.DeserializeObject<DateOnly>(@"""0001-01-01""");

            Assert.AreEqual(DateOnly.MinValue, d);
        }

        [Test]
        public void DeserializeNullable_Null()
        {
            DateOnly? d = JsonConvert.DeserializeObject<DateOnly?>(@"null");

            Assert.AreEqual(null, d);
        }

        [Test]
        public void DeserializeNullable_Value()
        {
            DateOnly? d = JsonConvert.DeserializeObject<DateOnly?>(@"""2000-12-29""");

            Assert.AreEqual(new DateOnly(2000, 12, 29), d);
        }

        [Test]
        public void DeserializeList()
        {
            var l = JsonConvert.DeserializeObject<IList<DateOnly>>(@"[
  ""2000-12-29""
]");

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(new DateOnly(2000, 12, 29), l[0]);
        }

        [Test]
        public void DeserializeList_Nullable()
        {
            var l = JsonConvert.DeserializeObject<IList<DateOnly?>>(@"[
  ""2000-12-29"",
  null
]");

            Assert.AreEqual(2, l.Count);
            Assert.AreEqual(new DateOnly(2000, 12, 29), l[0]);
            Assert.AreEqual(null, l[1]);
        }
    }
}
#endif
