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
    public class TimeOnlyTests : TestFixtureBase
    {
        [Test]
        public void Serialize()
        {
            TimeOnly t = new TimeOnly(23, 59, 59);
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""23:59:59""", json);
        }

        [Test]
        public void Serialize_Milliseconds()
        {
            TimeOnly t = new TimeOnly(23, 59, 59, 999);
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""23:59:59.999""", json);
        }

        [Test]
        public void SerializeDefault()
        {
            TimeOnly t = default;
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""00:00:00""", json);
        }

        [Test]
        public void SerializeMaxValue()
        {
            TimeOnly t = TimeOnly.MaxValue;
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""23:59:59.9999999""", json);
        }

        [Test]
        public void SerializeMinValue()
        {
            TimeOnly t = TimeOnly.MinValue;
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""00:00:00""", json);
        }

        [Test]
        public void SerializeNullable_Null()
        {
            TimeOnly? t = default;
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual("null", json);
        }

        [Test]
        public void SerializeNullable_Value()
        {
            TimeOnly? t = new TimeOnly(23, 59, 59, 999);
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            Assert.AreEqual(@"""23:59:59.999""", json);
        }

        [Test]
        public void SerializeList()
        {
            IList<TimeOnly> l = new List<TimeOnly>
            {
                new TimeOnly(23, 59, 59)
            };
            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            Assert.AreEqual(@"[
  ""23:59:59""
]", json);
        }

        [Test]
        public void SerializeList_Nullable()
        {
            IList<TimeOnly?> l = new List<TimeOnly?>
            {
                new TimeOnly(23, 59, 59),
                null
            };
            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            Assert.AreEqual(@"[
  ""23:59:59"",
  null
]", json);
        }

        [Test]
        public void Deserialize()
        {
            TimeOnly t = JsonConvert.DeserializeObject<TimeOnly>(@"""23:59:59""");

            Assert.AreEqual(new TimeOnly(23, 59, 59), t);
        }

        [Test]
        public void DeserializeDefault()
        {
            TimeOnly t = JsonConvert.DeserializeObject<TimeOnly>(@"""00:00:00""");

            Assert.AreEqual(default(TimeOnly), t);
        }

        [Test]
        public void DeserializeMaxValue()
        {
            TimeOnly t = JsonConvert.DeserializeObject<TimeOnly>(@"""23:59:59.9999999""");

            Assert.AreEqual(TimeOnly.MaxValue, t);
        }

        [Test]
        public void DeserializeMinValue()
        {
            TimeOnly t = JsonConvert.DeserializeObject<TimeOnly>(@"""00:00:00""");

            Assert.AreEqual(TimeOnly.MinValue, t);
        }

        [Test]
        public void DeserializeNullable_Null()
        {
            TimeOnly? t = JsonConvert.DeserializeObject<TimeOnly?>(@"null");

            Assert.AreEqual(null, t);
        }

        [Test]
        public void DeserializeNullable_Value()
        {
            TimeOnly? t = JsonConvert.DeserializeObject<TimeOnly?>(@"""23:59:59""");

            Assert.AreEqual(new TimeOnly(23, 59, 59), t);
        }

        [Test]
        public void DeserializeList()
        {
            var l = JsonConvert.DeserializeObject<IList<TimeOnly>>(@"[
  ""23:59:59""
]");

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(new TimeOnly(23, 59, 59), l[0]);
        }

        [Test]
        public void DeserializeList_Nullable()
        {
            var l = JsonConvert.DeserializeObject<IList<TimeOnly?>>(@"[
  ""23:59:59"",
  null
]");

            Assert.AreEqual(2, l.Count);
            Assert.AreEqual(new TimeOnly(23, 59, 59), l[0]);
            Assert.AreEqual(null, l[1]);
        }
    }
}
#endif
