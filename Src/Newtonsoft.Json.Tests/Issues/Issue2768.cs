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
using System.Globalization;
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
    public class Issue2768 : TestFixtureBase
    {
        [Test]
        public void Test_Serialize()
        {
            decimal d = 0.0m;
            string json = JsonConvert.SerializeObject(d);

            Assert.AreEqual("0.0", json);
        }

        [Test]
        public void Test_Serialize_NoTrailingZero()
        {
            decimal d = 0m;
            string json = JsonConvert.SerializeObject(d);

            Assert.AreEqual("0.0", json);
        }

        [Test]
        public void Test_Deserialize()
        {
            decimal d = JsonConvert.DeserializeObject<decimal>("0.0");

            Assert.AreEqual("0.0", d.ToString());
        }

        [Test]
        public void Test_Deserialize_NoTrailingZero()
        {
            decimal d = JsonConvert.DeserializeObject<decimal>("0");

            Assert.AreEqual("0", d.ToString());
        }

        [Test]
        public void ParseJsonDecimal()
        {
            var json = @"{ ""property"": 0.0 }";

            var reader = new JsonTextReader(new StringReader(json))
            {
                FloatParseHandling = FloatParseHandling.Decimal
            };

            decimal? parsedDecimal = null;
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.Float)
                {
                    parsedDecimal = (decimal)reader.Value;
                    break;
                }
            }

            Assert.AreEqual("0.0", parsedDecimal.ToString());
        }

        [Test]
        public void ParseJsonDecimal_IsBoxedInstanceSame()
        {
            var json = @"[ 0.0, 0.0 ]";

            var reader = new JsonTextReader(new StringReader(json))
            {
                FloatParseHandling = FloatParseHandling.Decimal
            };

            List<object> boxedDecimals = new List<object>();

            // Start array
            Assert.IsTrue(reader.Read());

            Assert.IsTrue(reader.Read());
            boxedDecimals.Add(reader.Value);

            Assert.IsTrue(reader.Read());
            boxedDecimals.Add(reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());

            // Boxed values will match or not depending on whether framework supports.
#if NET6_0_OR_GREATER
            Assert.IsTrue(object.ReferenceEquals(boxedDecimals[0], boxedDecimals[1]));
#else
            Assert.IsFalse(object.ReferenceEquals(boxedDecimals[0], boxedDecimals[1]));
#endif

        }
    }
}
#endif
