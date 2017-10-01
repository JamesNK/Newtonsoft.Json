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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
using System.Numerics;
#endif
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Xml;
using Newtonsoft.Json.Tests.JsonTextReaderTests;
using Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests;
using Newtonsoft.Json.Utilities;


namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class FloatTests : TestFixtureBase
    {
        [Test]
        public void Float_ReadAsString_Exact()
        {
            const string testJson = "{float: 0.0620}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            string s = reader.ReadAsString();
            Assert.AreEqual("0.0620", s);
        }

        [Test]
        public void Float_NaN_Read()
        {
            const string testJson = "{float: NaN}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NaN, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void Float_NaN_ReadAsInt32()
        {
            const string testJson = "{float: NaN}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Cannot read NaN value. Path 'float', line 1, position 11.");
        }

        [Test]
        public void Float_NaNAndInifinity_ReadAsDouble()
        {
            const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]"; ;

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(reader.Read());

            Assert.AreEqual(double.NaN, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NaN, reader.Value);

            Assert.AreEqual(double.PositiveInfinity, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.PositiveInfinity, reader.Value);

            Assert.AreEqual(double.NegativeInfinity, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NegativeInfinity, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void Float_NaNAndInifinity_ReadAsString()
        {
            const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]"; ;

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(reader.Read());

            Assert.AreEqual(JsonConvert.NaN, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.NaN, reader.Value);

            Assert.AreEqual(JsonConvert.PositiveInfinity, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.PositiveInfinity, reader.Value);

            Assert.AreEqual(JsonConvert.NegativeInfinity, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.NegativeInfinity, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void FloatParseHandling_ReadAsString()
        {
            string json = "[9223372036854775807, 1.7976931348623157E+308, 792281625142643375935439503.35, 792281625142643375935555555555555555555555555555555555555555555555555439503.35]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.AreEqual("9223372036854775807", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("9223372036854775807", reader.Value);

            Assert.AreEqual("1.7976931348623157E+308", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("1.7976931348623157E+308", reader.Value);

            Assert.AreEqual("792281625142643375935439503.35", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("792281625142643375935439503.35", reader.Value);

            Assert.AreEqual("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public void FloatParseHandling()
        {
            string json = "[1.0,1,9.9,1E-06]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1.0m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(9.9m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(Convert.ToDecimal(1E-06), reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public void FloatParseHandling_NaN()
        {
            string json = "[NaN]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Cannot read NaN value. Path '', line 1, position 4.");
        }

        [Test]
        public void FloatingPointNonFiniteNumbers()
        {
            string input = @"[
  NaN,
  Infinity,
  -Infinity
]";

            StringReader sr = new StringReader(input);

            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.NaN);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.PositiveInfinity);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.NegativeInfinity);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
            }
        }

        [Test]
        public void ReadFloatingPointNumber()
        {
            string json =
                @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN,0e-10,0.25e-5,0.3e10]";

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(json)))
            {
                jsonReader.Read();
                Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.1, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1.0, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1.000001, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1E-06, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(4.94065645841247E-324, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.PositiveInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NegativeInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NaN, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.MaxValue, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.MinValue, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.PositiveInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NegativeInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NaN, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0000025d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(3000000000d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);
            }
        }
    }
}
