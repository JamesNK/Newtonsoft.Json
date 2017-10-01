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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
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
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JTokenWriterAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task ValueFormattingAsync()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world.");

            JToken root;
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                await jsonWriter.WriteStartArrayAsync();
                await jsonWriter.WriteValueAsync('@');
                await jsonWriter.WriteValueAsync("\r\n\t\f\b?{\\r\\n\"\'");
                await jsonWriter.WriteValueAsync(true);
                await jsonWriter.WriteValueAsync(10);
                await jsonWriter.WriteValueAsync(10.99);
                await jsonWriter.WriteValueAsync(0.99);
                await jsonWriter.WriteValueAsync(0.000000000000000001d);
                await jsonWriter.WriteValueAsync(0.000000000000000001m);
                await jsonWriter.WriteValueAsync((string)null);
                await jsonWriter.WriteValueAsync("This is a string.");
                await jsonWriter.WriteNullAsync();
                await jsonWriter.WriteUndefinedAsync();
                await jsonWriter.WriteValueAsync(data);
                await jsonWriter.WriteEndArrayAsync();

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
        public async Task StateAsync()
        {
            using (JsonWriter jsonWriter = new JTokenWriter())
            {
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);

                await jsonWriter.WriteStartObjectAsync();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                await jsonWriter.WritePropertyNameAsync("CPU");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

                await jsonWriter.WriteValueAsync("Intel");
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                await jsonWriter.WritePropertyNameAsync("Drives");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

                await jsonWriter.WriteStartArrayAsync();
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

                await jsonWriter.WriteValueAsync("DVD read/writer");
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
                await jsonWriter.WriteValueAsync(new BigInteger(123));
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
#endif

                await jsonWriter.WriteValueAsync(new byte[0]);
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

                await jsonWriter.WriteEndAsync();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

                await jsonWriter.WriteEndObjectAsync();
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
            }
        }

        [Test]
        public async Task CurrentTokenAsync()
        {
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
                Assert.AreEqual(null, jsonWriter.CurrentToken);

                await jsonWriter.WriteStartObjectAsync();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(jsonWriter.Token, jsonWriter.CurrentToken);

                JObject o = (JObject)jsonWriter.Token;

                await jsonWriter.WritePropertyNameAsync("CPU");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
                Assert.AreEqual(o.Property("CPU"), jsonWriter.CurrentToken);

                await jsonWriter.WriteValueAsync("Intel");
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(o["CPU"], jsonWriter.CurrentToken);

                await jsonWriter.WritePropertyNameAsync("Drives");
                Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
                Assert.AreEqual(o.Property("Drives"), jsonWriter.CurrentToken);

                await jsonWriter.WriteStartArrayAsync();
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(o["Drives"], jsonWriter.CurrentToken);

                JArray a = (JArray)jsonWriter.CurrentToken;

                await jsonWriter.WriteValueAsync("DVD read/writer");
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
                await jsonWriter.WriteValueAsync(new BigInteger(123));
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);
#endif

                await jsonWriter.WriteValueAsync(new byte[0]);
                Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
                Assert.AreEqual(a[a.Count - 1], jsonWriter.CurrentToken);

                await jsonWriter.WriteEndAsync();
                Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
                Assert.AreEqual(a, jsonWriter.CurrentToken);

                await jsonWriter.WriteEndObjectAsync();
                Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
                Assert.AreEqual(o, jsonWriter.CurrentToken);
            }
        }

        [Test]
        public async Task WriteCommentAsync()
        {
            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();
            await writer.WriteCommentAsync("fail");
            await writer.WriteEndArrayAsync();

            StringAssert.AreEqual(@"[
  /*fail*/]", writer.Token.ToString());
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task WriteBigIntegerAsync()
        {
            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();
            await writer.WriteValueAsync(new BigInteger(123));
            await writer.WriteEndArrayAsync();

            JValue i = (JValue)writer.Token[0];

            Assert.AreEqual(new BigInteger(123), i.Value);
            Assert.AreEqual(JTokenType.Integer, i.Type);

            StringAssert.AreEqual(@"[
  123
]", writer.Token.ToString());
        }
#endif

        [Test]
        public async Task WriteRawAsync()
        {
            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();
            await writer.WriteRawAsync("fail");
            await writer.WriteRawAsync("fail");
            await writer.WriteEndArrayAsync();

            // this is a bug. See non-async equivalent test.
            StringAssert.AreEqual(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Test]
        public async Task WriteTokenWithParentAsync()
        {
            JObject o = new JObject
            {
                ["prop1"] = new JArray(1),
                ["prop2"] = 1
            };

            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();

            await writer.WriteTokenAsync(o.CreateReader());

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            await writer.WriteEndArrayAsync();

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
        public async Task WriteTokenWithPropertyParentAsync()
        {
            JValue v = new JValue(1);

            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("Prop1");

            await writer.WriteTokenAsync(v.CreateReader());

            Assert.AreEqual(WriteState.Object, writer.WriteState);

            await writer.WriteEndObjectAsync();

            StringAssert.AreEqual(@"{
  ""Prop1"": 1
}", writer.Token.ToString());
        }

        [Test]
        public async Task WriteValueTokenWithParentAsync()
        {
            JValue v = new JValue(1);

            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();

            await writer.WriteTokenAsync(v.CreateReader());

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            await writer.WriteEndArrayAsync();

            StringAssert.AreEqual(@"[
  1
]", writer.Token.ToString());
        }

        [Test]
        public async Task WriteEmptyTokenAsync()
        {
            JObject o = new JObject();
            JsonReader reader = o.CreateReader();
            while (reader.Read())
            {   
            }

            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();

            await writer.WriteTokenAsync(reader);

            Assert.AreEqual(WriteState.Array, writer.WriteState);

            await writer.WriteEndArrayAsync();

            StringAssert.AreEqual(@"[]", writer.Token.ToString());
        }

        [Test]
        public async Task WriteRawValueAsync()
        {
            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartArrayAsync();
            await writer.WriteRawValueAsync("fail");
            await writer.WriteRawValueAsync("fail");
            await writer.WriteEndArrayAsync();

            StringAssert.AreEqual(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Test]
        public async Task WriteDuplicatePropertyNameAsync()
        {
            JTokenWriter writer = new JTokenWriter();

            await writer.WriteStartObjectAsync();

            await writer.WritePropertyNameAsync("prop1");
            await writer.WriteStartObjectAsync();
            await writer.WriteEndObjectAsync();

            await writer.WritePropertyNameAsync("prop1");
            await writer.WriteStartArrayAsync();
            await writer.WriteEndArrayAsync();

            await writer.WriteEndObjectAsync();

            StringAssert.AreEqual(@"{
  ""prop1"": []
}", writer.Token.ToString());
        }

        [Test]
        public async Task DateTimeZoneHandlingAsync()
        {
            JTokenWriter writer = new JTokenWriter
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            await writer.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

            JValue value = (JValue)writer.Token;
            DateTime dt = (DateTime)value.Value;

            Assert.AreEqual(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc), dt);
        }
    }
}

#endif