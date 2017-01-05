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
using System.Globalization;
using System.IO;
#if !PORTABLE || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Text;
using System.Threading.Tasks;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class TraceWriterAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task WriteNullableByteAsync()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            await traceJsonWriter.WriteStartArrayAsync();
            await traceJsonWriter.WriteValueAsync((byte?)null);
            await traceJsonWriter.WriteEndArrayAsync();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

#if !PORTABLE || NETSTANDARD1_1
        [Test]
        public async Task TraceJsonWriterTestAsync()
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter w = new JsonTextWriter(sw);
            TraceJsonWriter traceWriter = new TraceJsonWriter(w);

            await traceWriter.WriteStartObjectAsync();
            await traceWriter.WritePropertyNameAsync("Array");
            await traceWriter.WriteStartArrayAsync();
            await traceWriter.WriteValueAsync("String!");
            await traceWriter.WriteValueAsync(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            await traceWriter.WriteValueAsync(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            await traceWriter.WriteValueAsync(1.1f);
            await traceWriter.WriteValueAsync(1.1d);
            await traceWriter.WriteValueAsync(1.1m);
            await traceWriter.WriteValueAsync(1);
            await traceWriter.WriteValueAsync('!');
            await traceWriter.WriteValueAsync((short)1);
            await traceWriter.WriteValueAsync((ushort)1);
            await traceWriter.WriteValueAsync(1);
            await traceWriter.WriteValueAsync(1U);
            await traceWriter.WriteValueAsync((sbyte)1);
            await traceWriter.WriteValueAsync((byte)1);
            await traceWriter.WriteValueAsync(1L);
            await traceWriter.WriteValueAsync(1UL);
            await traceWriter.WriteValueAsync(true);

            await traceWriter.WriteValueAsync((DateTime?)new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            await traceWriter.WriteValueAsync((DateTimeOffset?)new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            await traceWriter.WriteValueAsync((float?)1.1f);
            await traceWriter.WriteValueAsync((double?)1.1d);
            await traceWriter.WriteValueAsync((decimal?)1.1m);
            await traceWriter.WriteValueAsync((int?)1);
            await traceWriter.WriteValueAsync((char?)'!');
            await traceWriter.WriteValueAsync((short?)1);
            await traceWriter.WriteValueAsync((ushort?)1);
            await traceWriter.WriteValueAsync((int?)1);
            await traceWriter.WriteValueAsync((uint?)1);
            await traceWriter.WriteValueAsync((sbyte?)1);
            await traceWriter.WriteValueAsync((byte?)1);
            await traceWriter.WriteValueAsync((long?)1);
            await traceWriter.WriteValueAsync((ulong?)1);
            await traceWriter.WriteValueAsync((bool?)true);
            await traceWriter.WriteValueAsync(BigInteger.Parse("9999999990000000000000000000000000000000000"));

            await traceWriter.WriteValueAsync((object)true);
            await traceWriter.WriteValueAsync(TimeSpan.FromMinutes(1));
            await traceWriter.WriteValueAsync(Guid.Empty);
            await traceWriter.WriteValueAsync(new Uri("http://www.google.com/"));
            await traceWriter.WriteValueAsync(Encoding.UTF8.GetBytes("String!"));
            await traceWriter.WriteRawValueAsync("[1],");
            await traceWriter.WriteRawAsync("[2]");
            await traceWriter.WriteNullAsync();
            await traceWriter.WriteUndefinedAsync();
            await traceWriter.WriteStartConstructorAsync("ctor");
            await traceWriter.WriteValueAsync(1);
            await traceWriter.WriteEndConstructorAsync();
            await traceWriter.WriteCommentAsync("A comment");
            await traceWriter.WriteWhitespaceAsync("       ");
            await traceWriter.WriteEndAsync();
            await traceWriter.WriteEndObjectAsync();
            await traceWriter.FlushAsync();
            await traceWriter.CloseAsync();

            string json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    9999999990000000000000000000000000000000000,
    true,
    true,
    ""00:01:00"",
    ""00000000-0000-0000-0000-000000000000"",
    ""http://www.google.com/"",
    ""U3RyaW5nIQ=="",
    [1],[2],
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/       
  ]
}";

            StringAssert.AreEqual("Serialized JSON: " + Environment.NewLine + json, traceWriter.GetSerializedJsonMessage());
        }

        [Test]
        public async Task TraceJsonReaderTestAsync()
        {
            string json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+00:00"",
    ""U3RyaW5nIQ=="",
    1,
    1.1,
    1.2,
    9999999990000000000000000000000000000000000,
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/
  ]
}";

            StringReader sw = new StringReader(json);
            JsonTextReader w = new JsonTextReader(sw);
            TraceJsonReader traceReader = new TraceJsonReader(w);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.StartObject, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.PropertyName, traceReader.TokenType);
            Assert.AreEqual("Array", traceReader.Value);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.StartArray, traceReader.TokenType);
            Assert.AreEqual(null, traceReader.Value);

            await traceReader.ReadAsStringAsync();
            Assert.AreEqual(JsonToken.String, traceReader.TokenType);
            Assert.AreEqual('"', traceReader.QuoteChar);
            Assert.AreEqual("String!", traceReader.Value);

            // for great code coverage justice!
            traceReader.QuoteChar = '\'';
            Assert.AreEqual('\'', traceReader.QuoteChar);

            await traceReader.ReadAsStringAsync();
            Assert.AreEqual(JsonToken.String, traceReader.TokenType);
            Assert.AreEqual("2000-12-12T12:12:12Z", traceReader.Value);

            await traceReader.ReadAsDateTimeAsync();
            Assert.AreEqual(JsonToken.Date, traceReader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), traceReader.Value);

            await traceReader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, traceReader.TokenType);
            Assert.AreEqual(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), traceReader.Value);

            await traceReader.ReadAsBytesAsync();
            Assert.AreEqual(JsonToken.Bytes, traceReader.TokenType);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("String!"), (byte[])traceReader.Value);

            await traceReader.ReadAsInt32Async();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(1, traceReader.Value);

            await traceReader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, traceReader.TokenType);
            Assert.AreEqual(1.1m, traceReader.Value);

            await traceReader.ReadAsDoubleAsync();
            Assert.AreEqual(JsonToken.Float, traceReader.TokenType);
            Assert.AreEqual(1.2d, traceReader.Value);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(typeof(BigInteger), traceReader.ValueType);
            Assert.AreEqual(BigInteger.Parse("9999999990000000000000000000000000000000000"), traceReader.Value);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.Null, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.Undefined, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.StartConstructor, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(1L, traceReader.Value);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.EndConstructor, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.Comment, traceReader.TokenType);
            Assert.AreEqual("A comment", traceReader.Value);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.EndArray, traceReader.TokenType);

            await traceReader.ReadAsync();
            Assert.AreEqual(JsonToken.EndObject, traceReader.TokenType);

            Assert.IsFalse(await traceReader.ReadAsync());

            traceReader.Close();

            StringAssert.AreEqual("Deserialized JSON: " + Environment.NewLine + json, traceReader.GetDeserializedJsonMessage());
        }
#endif
    }
}

#endif