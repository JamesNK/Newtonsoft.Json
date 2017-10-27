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
#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
using Newtonsoft.Json.Linq;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JTokenWriterTest : TestFixtureBase
    {
        [Test]
        public void ValueFormatting()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world.");

            JToken root;
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue('@');
                jsonWriter.WriteValue("\r\n\t\f\b?{\\r\\n\"\'");
                jsonWriter.WriteValue(true);
                jsonWriter.WriteValue(10);
                jsonWriter.WriteValue(10.99);
                jsonWriter.WriteValue(0.99);
                jsonWriter.WriteValue(0.000000000000000001d);
                jsonWriter.WriteValue(0.000000000000000001m);
                jsonWriter.WriteValue((string)null);
                jsonWriter.WriteValue("This is a string.");
                jsonWriter.WriteNull();
                jsonWriter.WriteUndefined();
                jsonWriter.WriteValue(data);
                jsonWriter.WriteEndArray();

                root = jsonWriter.Token;
            }

            CustomAssert.IsInstanceOfType(typeof(JArray), root);
            Assert.AreEqual(13, root.Children().Count());
            Assert.AreEqual("@", (string)root[0]);
            Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", (string)root[1]);
            Assert.AreEqual(true, (bool)root[2]);
            Assert.AreEqual(10, (int)root[3]);
            Assert.AreEqual(10.99, (double)root[4]);
            Assert.AreEqual(0.99, (double)root[5]);
            Assert.AreEqual(0.000000000000000001d, (double)root[6]);
            Assert.AreEqual(0.000000000000000001m, (decimal)root[7]);
            Assert.AreEqual(null, (string)root[8]);
            Assert.AreEqual("This is a string.", (string)root[9]);
            Assert.AreEqual(null, ((JValue)root[10]).Value);
            Assert.AreEqual(null, ((JValue)root[11]).Value);
            Assert.AreEqual(data, (byte[])root[12]);
        }

        [Test]
        public void State()
        {
            using (JsonWriter jsonWriter = new JTokenWriter())
            {
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);

                jsonWriter.WriteStartObject();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WritePropertyName("CPU");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

                jsonWriter.WriteValue("Intel");
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WritePropertyName("Drives");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

                jsonWriter.WriteStartArray();
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

                jsonWriter.WriteValue("DVD read/writer");
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
                jsonWriter.WriteValue(new BigInteger(123));
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
#endif

                jsonWriter.WriteValue(new byte[0]);
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

                jsonWriter.WriteEnd();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WriteEndObject();
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
            }
        }

        [Test]
        public void CurrentToken()
        {
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
                Assert.AreEqual(null, jsonWriter.CurrentToken);

                jsonWriter.WriteStartObject();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(jsonWriter.Token, jsonWriter.CurrentToken);

                JObject o = (JObject)jsonWriter.Token;

                jsonWriter.WritePropertyName("CPU");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
                Assert.AreEqual(o.Property("CPU"), jsonWriter.CurrentToken);

                jsonWriter.WriteValue("Intel");
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(o["CPU"], jsonWriter.CurrentToken);

                jsonWriter.WritePropertyName("Drives");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
                Assert.AreEqual(o.Property("Drives"), jsonWriter.CurrentToken);

                jsonWriter.WriteStartArray();
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(o["Drives"], jsonWriter.CurrentToken);

                JArray a = (JArray)jsonWriter.CurrentToken;

                jsonWriter.WriteValue("DVD read/writer");
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
                jsonWriter.WriteValue(new BigInteger(123));
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);
#endif

                jsonWriter.WriteValue(new byte[0]);
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);

                jsonWriter.WriteEnd();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(a, jsonWriter.CurrentToken);

                jsonWriter.WriteEndObject();
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
                Assert.AreEqual(o, jsonWriter.CurrentToken);
            }
        }

        [Test]
        public void WriteComment()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteComment("fail");
            writer.WriteEndArray();

            StringAssert.AreEqual(@"[
  /*fail*/]", writer.Token.ToString());
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void WriteBigInteger()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteValue(new BigInteger(123));
            writer.WriteEndArray();

            JValue i = (JValue)writer.Token[0];

            Assert.AreEqual(new BigInteger(123), i.Value);
            Assert.AreEqual(JTokenType.Integer, i.Type);

            StringAssert.AreEqual(@"[
  123
]", writer.Token.ToString());
        }
#endif

        [Test]
        public void WriteRaw()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteRaw("fail");
            writer.WriteRaw("fail");
            writer.WriteEndArray();

            // this is a bug. write raw shouldn't be autocompleting like this
            // hard to fix without introducing Raw and RawValue token types
            // meh
            StringAssert.AreEqual(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Test]
        public void WriteTokenWithParent()
        {
            JObject o = new JObject
            {
                ["prop1"] = new JArray(1),
                ["prop2"] = 1
            };

            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();

            writer.WriteToken(o.CreateReader());

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            writer.WriteEndArray();

            Console.WriteLine(writer.Token.ToString());

            StringAssert.AreEqual(@"[
  {
    ""prop1"": [
      1
    ],
    ""prop2"": 1
  }
]", writer.Token.ToString());
        }

        [Test]
        public void WriteTokenWithPropertyParent()
        {
            JValue v = new JValue(1);

            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            writer.WritePropertyName("Prop1");

            writer.WriteToken(v.CreateReader());

            Assert.AreEqual(WriteState.Object, writer.WriteState);

            writer.WriteEndObject();

            StringAssert.AreEqual(@"{
  ""Prop1"": 1
}", writer.Token.ToString());
        }

        [Test]
        public void WriteValueTokenWithParent()
        {
            JValue v = new JValue(1);

            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();

            writer.WriteToken(v.CreateReader());

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            writer.WriteEndArray();

            StringAssert.AreEqual(@"[
  1
]", writer.Token.ToString());
        }

        [Test]
        public void WriteEmptyToken()
        {
            JObject o = new JObject();
            JsonReader reader = o.CreateReader();
            while (reader.Read())
            {   
            }

            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();

            writer.WriteToken(reader);

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            writer.WriteEndArray();

            StringAssert.AreEqual(@"[]", writer.Token.ToString());
        }

        [Test]
        public void WriteRawValue()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteRawValue("fail");
            writer.WriteRawValue("fail");
            writer.WriteEndArray();

            StringAssert.AreEqual(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Test]
        public void WriteDuplicatePropertyName()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();

            writer.WritePropertyName("prop1");
            writer.WriteStartObject();
            writer.WriteEndObject();

            writer.WritePropertyName("prop1");
            writer.WriteStartArray();
            writer.WriteEndArray();

            writer.WriteEndObject();

            StringAssert.AreEqual(@"{
  ""prop1"": []
}", writer.Token.ToString());
        }

        [Test]
        public void DateTimeZoneHandling()
        {
            JTokenWriter writer = new JTokenWriter
            {
                DateTimeZoneHandling = Json.DateTimeZoneHandling.Utc
            };

            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

            JValue value = (JValue)writer.Token;
            DateTime dt = (DateTime)value.Value;

            Assert.AreEqual(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc), dt);
        }
    }
}