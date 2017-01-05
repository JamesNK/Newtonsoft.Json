﻿#region License
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
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if !PORTABLE || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Text;
using System.Text.RegularExpressions;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Bson
{
    [TestFixture]
    public class BsonReaderAsyncTests : TestFixtureBase
    {
        private const char Euro = '\u20ac';

        [Test]
        public async Task DeserializeLargeBsonObjectAsync()
        {
            byte[] data = System.IO.File.ReadAllBytes(@"SpaceShipV2.bson");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            JObject o = (JObject)await JToken.ReadFromAsync(reader);

            Assert.AreEqual("1", (string)o["$id"]);
        }

        public class MyTest
        {
            public DateTime TimeStamp { get; set; }
            public string UserName { get; set; }
            public MemoryStream Blob { get; set; }
        }

        public async Task Bson_SupportMultipleContentAsync()
        {
            MemoryStream myStream = new MemoryStream();
            BsonWriter writer = new BsonWriter(myStream);
            JsonSerializer serializer = new JsonSerializer();
            MyTest tst1 = new MyTest
            {
                TimeStamp = new DateTime(2000, 12, 20, 12, 59, 59, DateTimeKind.Utc),
                UserName = "Joe Doe"
            };
            MyTest tst2 = new MyTest
            {
                TimeStamp = new DateTime(2010, 12, 20, 12, 59, 59, DateTimeKind.Utc),
                UserName = "Bob"
            };
            await serializer.SerializeAsync(writer, tst1);
            await serializer.SerializeAsync( writer, tst2);

            myStream.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(myStream)
            {
                SupportMultipleContent = true,
                DateTimeKindHandling = DateTimeKind.Utc
            };

            MyTest tst1A = await serializer.DeserializeAsync<MyTest>(reader);

            await reader.ReadAsync();

            MyTest tst2A = await serializer.DeserializeAsync<MyTest>(reader);

            Assert.AreEqual(tst1.UserName, tst1A.UserName);
            Assert.AreEqual(tst1.TimeStamp, tst1A.TimeStamp);

            Assert.AreEqual(tst2.UserName, tst2A.UserName);
            Assert.AreEqual(tst2.TimeStamp, tst2A.TimeStamp);
        }

        [Test]
        public async Task ReadSingleObjectAsync()
        {
            byte[] data = HexToBytes("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00");
            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Blah", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadGuid_TextAsync()
        {
            byte[] data = HexToBytes("31-00-00-00-02-30-00-25-00-00-00-64-38-32-31-65-65-64-37-2D-34-62-35-63-2D-34-33-63-39-2D-38-61-63-32-2D-36-39-32-38-65-35-37-39-62-37-30-35-00-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("d821eed7-4b5c-43c9-8ac2-6928e579b705", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            ms = new MemoryStream(data);
            reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            JsonSerializer serializer = new JsonSerializer();
            IList<Guid> l = await serializer.DeserializeAsync<IList<Guid>>(reader);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705"), l[0]);
        }

        [Test]
        public async Task ReadGuid_BytesAsync()
        {
            byte[] data = HexToBytes("1D-00-00-00-05-30-00-10-00-00-00-04-D7-EE-21-D8-5C-4B-C9-43-8A-C2-69-28-E5-79-B7-05-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Guid g = new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(g, reader.Value);
            Assert.AreEqual(typeof(Guid), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            ms = new MemoryStream(data);
            reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            JsonSerializer serializer = new JsonSerializer();
            IList<Guid> l = await serializer.DeserializeAsync<IList<Guid>>(reader);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(g, l[0]);
        }

        [Test]
        public async Task ReadDoubleAsync()
        {
            byte[] data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(99.99d, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadDouble_DecimalAsync()
        {
            byte[] data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.FloatParseHandling = FloatParseHandling.Decimal;
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(99.99m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadValuesAsync()
        {
            byte[] data = HexToBytes("8C-00-00-00-12-30-00-FF-FF-FF-FF-FF-FF-FF-7F-12-31-00-FF-FF-FF-FF-FF-FF-FF-7F-10-32-00-FF-FF-FF-7F-10-33-00-FF-FF-FF-7F-10-34-00-FF-00-00-00-10-35-00-7F-00-00-00-02-36-00-02-00-00-00-61-00-01-37-00-00-00-00-00-00-00-F0-45-01-38-00-FF-FF-FF-FF-FF-FF-EF-7F-01-39-00-00-00-00-E0-FF-FF-EF-47-08-31-30-00-01-05-31-31-00-05-00-00-00-02-00-01-02-03-04-09-31-32-00-40-C5-E2-BA-E3-00-00-00-09-31-33-00-40-C5-E2-BA-E3-00-00-00-00");
            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
#pragma warning disable 612,618
            reader.JsonNet35BinaryCompatibility = true;
#pragma warning restore 612,618
            reader.ReadRootValueAsArray = true;
            reader.DateTimeKindHandling = DateTimeKind.Utc;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)int.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)int.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)byte.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)sbyte.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual((double)decimal.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual((double)float.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);
            Assert.AreEqual(typeof(bool), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(new byte[] { 0, 1, 2, 3, 4 }, (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadObjectBsonFromSiteAsync()
        {
            byte[] data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("0", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("1", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("2", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("c", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadArrayBsonFromSiteAsync()
        {
            byte[] data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.AreEqual(false, reader.ReadRootValueAsArray);
            Assert.AreEqual(DateTimeKind.Local, reader.DateTimeKindHandling);

            reader.ReadRootValueAsArray = true;
            reader.DateTimeKindHandling = DateTimeKind.Utc;

            Assert.AreEqual(true, reader.ReadRootValueAsArray);
            Assert.AreEqual(DateTimeKind.Utc, reader.DateTimeKindHandling);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("c", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadAsInt32BadStringAsync()
        {
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                byte[] data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

                MemoryStream ms = new MemoryStream(data);
                BsonReader reader = new BsonReader(ms);

                Assert.AreEqual(false, reader.ReadRootValueAsArray);
                Assert.AreEqual(DateTimeKind.Local, reader.DateTimeKindHandling);

                reader.ReadRootValueAsArray = true;
                reader.DateTimeKindHandling = DateTimeKind.Utc;

                Assert.AreEqual(true, reader.ReadRootValueAsArray);
                Assert.AreEqual(DateTimeKind.Utc, reader.DateTimeKindHandling);

                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

                await reader.ReadAsInt32Async();
            }, "Could not convert string to integer: a. Path '[0]'.");
        }

        [Test]
        public async Task ReadBytesAsync()
        {
            byte[] data = HexToBytes("2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-02-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms, true, DateTimeKind.Utc);
#pragma warning disable 612,618
            reader.JsonNet35BinaryCompatibility = true;
#pragma warning restore 612,618

            Assert.AreEqual(true, reader.ReadRootValueAsArray);
            Assert.AreEqual(DateTimeKind.Utc, reader.DateTimeKindHandling);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            byte[] encodedStringData = await reader.ReadAsBytesAsync();
            Assert.IsNotNull(encodedStringData);
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(encodedStringData, reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            string decodedString = Encoding.UTF8.GetString(encodedStringData, 0, encodedStringData.Length);
            Assert.AreEqual("Hello world!", decodedString);
        }

        [Test]
        public async Task ReadOidAsync()
        {
            byte[] data = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("_id", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(HexToBytes("4ABBED9D1D8B0F0218000001"), (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("test", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("1234£56", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadNestedArrayAsync()
        {
            string hexdoc = "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("_id", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"), (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("a", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            for (int i = 1; i <= 8; i++)
            {
                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.Float, reader.TokenType);

                double value = (i != 5)
                    ? Convert.ToDouble(i)
                    : 5.78960446186581E+77d;

                Assert.AreEqual(value, reader.Value);
            }

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("b", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("test", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadNestedArrayIntoLinqAsync()
        {
            string hexdoc = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

            byte[] data = HexToBytes(hexdoc);

            BsonReader reader = new BsonReader(new MemoryStream(data));
#pragma warning disable 612,618
            reader.JsonNet35BinaryCompatibility = true;
#pragma warning restore 612,618

            JObject o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(3, o.Count);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            await o.WriteToAsync(writer);
            await writer.FlushAsync();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual(hexdoc, bson);
        }

        [Test]
        public async Task OidAndBytesAreEqualAsync()
        {
            byte[] data1 = HexToBytes(
                "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

            BsonReader reader1 = new BsonReader(new MemoryStream(data1));
#pragma warning disable 612,618
            reader1.JsonNet35BinaryCompatibility = true;
#pragma warning restore 612,618

            // oid
            JObject o1 = (JObject)await JToken.ReadFromAsync(reader1);

            byte[] data2 = HexToBytes(
                "87-00-00-00-05-5F-69-64-00-0C-00-00-00-02-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

            BsonReader reader2 = new BsonReader(new MemoryStream(data2));
#pragma warning disable 612,618
            reader2.JsonNet35BinaryCompatibility = true;
#pragma warning restore 612,618

            // bytes
            JObject o2 = (JObject)await JToken.ReadFromAsync(reader2);

            Assert.IsTrue(o1.DeepEquals(o2));
        }

        [Test]
        public async Task ReadRegexAsync()
        {
            string hexdoc = "15-00-00-00-0B-72-65-67-65-78-00-74-65-73-74-00-67-69-6D-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("regex", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"/test/gim", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadCodeAsync()
        {
            string hexdoc = "1A-00-00-00-0D-63-6F-64-65-00-0B-00-00-00-49-20-61-6D-20-63-6F-64-65-21-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("code", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"I am code!", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadUndefinedAsync()
        {
            string hexdoc = "10-00-00-00-06-75-6E-64-65-66-69-6E-65-64-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("undefined", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);
            Assert.AreEqual(null, reader.Value);
            Assert.AreEqual(null, reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadLongAsync()
        {
            string hexdoc = "13-00-00-00-12-6C-6F-6E-67-00-FF-FF-FF-FF-FF-FF-FF-7F-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("long", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadReferenceAsync()
        {
            string hexdoc = "1E-00-00-00-0C-6F-69-64-00-04-00-00-00-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("oid", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$ref", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("oid", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$id", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadCodeWScopeAsync()
        {
            string hexdoc = "75-00-00-00-0F-63-6F-64-65-57-69-74-68-53-63-6F-70-65-00-61-00-00-00-35-00-00-00-66-6F-72-20-28-69-6E-74-20-69-20-3D-20-30-3B-20-69-20-3C-20-31-30-30-30-3B-20-69-2B-2B-29-0D-0A-7B-0D-0A-20-20-61-6C-65-72-74-28-61-72-67-31-29-3B-0D-0A-7D-00-24-00-00-00-02-61-72-67-31-00-15-00-00-00-4A-73-6F-6E-2E-4E-45-54-20-69-73-20-61-77-65-73-6F-6D-65-2E-00-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("codeWithScope", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$code", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("for (int i = 0; i < 1000; i++)\r\n{\r\n  alert(arg1);\r\n}", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$scope", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("arg1", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("Json.NET is awesome.", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadEndOfStreamAsync()
        {
            BsonReader reader = new BsonReader(new MemoryStream());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadLargeStringsAsync()
        {
            string bson =
                "4E-02-00-00-02-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-22-01-00-00-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-00";

            BsonReader reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

            StringBuilder largeStringBuilder = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                if (i > 0)
                {
                    largeStringBuilder.Append("-");
                }

                largeStringBuilder.Append(i.ToString(CultureInfo.InvariantCulture));
            }
            string largeString = largeStringBuilder.ToString();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(largeString, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(largeString, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadEmptyStringsAsync()
        {
            string bson = "0C-00-00-00-02-00-01-00-00-00-00-00";

            BsonReader reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task WriteAndReadEmptyListsAndDictionariesAsync()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("Arguments");
            await writer.WriteStartObjectAsync();
            await writer.WriteEndObjectAsync();
            await writer.WritePropertyNameAsync("List");
            await writer.WriteStartArrayAsync();
            await writer.WriteEndArrayAsync();
            await writer.WriteEndObjectAsync();

            string bson = BitConverter.ToString(ms.ToArray());

            Assert.AreEqual("20-00-00-00-03-41-72-67-75-6D-65-6E-74-73-00-05-00-00-00-00-04-4C-69-73-74-00-05-00-00-00-00-00", bson);

            BsonReader reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Arguments", reader.Value.ToString());

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("List", reader.Value.ToString());

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task DateTimeKindHandlingAsync()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("DateTime");
            await writer.WriteValueAsync(value);
            await writer.WriteEndObjectAsync();

            byte[] bson = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Utc);
            JObject o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Local);
            o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(value.ToLocalTime(), (DateTime)o["DateTime"]);

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Unspecified);
            o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), (DateTime)o["DateTime"]);
        }

        [Test]
        public async Task UnspecifiedDateTimeKindHandlingAsync()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.DateTimeKindHandling = DateTimeKind.Unspecified;

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("DateTime");
            await writer.WriteValueAsync(value);
            await writer.WriteEndObjectAsync();

            byte[] bson = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Unspecified);
            JObject o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);
        }

        [Test]
        public async Task LocalDateTimeKindHandlingAsync()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("DateTime");
            await writer.WriteValueAsync(value);
            await writer.WriteEndObjectAsync();

            byte[] bson = ms.ToArray();

            JObject o;
            BsonReader reader;

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Local);
            o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);
        }

        private async Task<string> WriteAndReadStringValueAsync(string val)
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter bs = new BsonWriter(ms);
            await bs.WriteStartObjectAsync();
            await bs.WritePropertyNameAsync("StringValue");
            await bs.WriteValueAsync(val);
            await bs.WriteEndAsync();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            // object
            await reader.ReadAsync();
            // property name
            await reader.ReadAsync();
            // string
            await reader.ReadAsync();
            return (string)reader.Value;
        }

        private async Task<string> WriteAndReadStringPropertyNameAsync(string val)
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter bs = new BsonWriter(ms);
            await bs.WriteStartObjectAsync();
            await bs.WritePropertyNameAsync(val);
            await bs.WriteValueAsync("Dummy");
            await bs.WriteEndAsync();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            // object
            await reader.ReadAsync();
            // property name
            await reader.ReadAsync();
            return (string)reader.Value;
        }

        [Test]
        public async Task TestReadLenStringValueShortTripleByteAsync()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append('1',127); //first char of euro at the end of the boundry.
            //sb.Append(euro, 5);
            //sb.Append('1',128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadLenStringValueTripleByteCharBufferBoundry0Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 127); //first char of euro at the end of the boundry.
            sb.Append(Euro, 5);
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await  WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadLenStringValueTripleByteCharBufferBoundry1Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 126);
            sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            string result = await  WriteAndReadStringValueAsync(expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task TestReadLenStringValueTripleByteCharOneAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Euro, 1); //Just one triple byte char in the string.

            string expected = sb.ToString();
            Assert.AreEqual(expected, await  WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadLenStringValueTripleByteCharBufferBoundry2Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 125);
            sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await  WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadStringValueAsync()
        {
            string expected = "test";
            Assert.AreEqual(expected, await  WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadStringValueLongAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('t', 150);
            string expected = sb.ToString();
            Assert.AreEqual(expected, await  WriteAndReadStringValueAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameShortTripleByteAsync()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append('1',127); //first char of euro at the end of the boundry.
            //sb.Append(euro, 5);
            //sb.Append('1',128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry0Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 127); //first char of euro at the end of the boundry.
            sb.Append(Euro, 5);
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            string result = await WriteAndReadStringPropertyNameAsync(expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry1Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 126);
            sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameTripleByteCharOneAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Euro, 1); //Just one triple byte char in the string.

            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry2Async()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 125);
            sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameAsync()
        {
            string expected = "test";
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task TestReadStringPropertyNameLongAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('t', 150);
            string expected = sb.ToString();
            Assert.AreEqual(expected, await WriteAndReadStringPropertyNameAsync(expected));
        }

        [Test]
        public async Task ReadRegexWithOptionsAsync()
        {
            string hexdoc = "1A-00-00-00-0B-72-65-67-65-78-00-61-62-63-00-69-00-0B-74-65-73-74-00-00-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("/abc/i", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("//", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task CanRoundTripStackOverflowDataAsync()
        {
            var doc =
                @"{
""AboutMe"": ""<p>I'm the Director for Research and Development for <a href=\""http://www.prophoenix.com\"" rel=\""nofollow\"">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\""http://blog.usepowershell.com\"" rel=\""nofollow\"">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\""http://powershellcommunity.org\"" rel=\""nofollow\"">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n"",
""WebsiteUrl"": ""http://blog.usepowershell.com""
}";
            JObject parsed = JObject.Parse(doc);
            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            await parsed.WriteToAsync(bsonWriter);
            await bsonWriter.FlushAsync();
            memoryStream.Position = 0;

            BsonReader reader = new BsonReader(memoryStream);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("AboutMe", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("<p>I'm the Director for Research and Development for <a href=\"http://www.prophoenix.com\" rel=\"nofollow\">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\"http://blog.usepowershell.com\" rel=\"nofollow\">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\"http://powershellcommunity.org\" rel=\"nofollow\">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("WebsiteUrl", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("http://blog.usepowershell.com", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task MultibyteCharacterPropertyNamesAndStringsAsync()
        {
            string json = @"{
  ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ"": ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ""
}";
            JObject parsed = JObject.Parse(json);
            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            await parsed.WriteToAsync(bsonWriter);
            await bsonWriter.FlushAsync();
            memoryStream.Position = 0;

            BsonReader reader = new BsonReader(memoryStream);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task UriGuidTimeSpanTestClassEmptyTestAsync()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass();

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            JsonSerializer serializer = new JsonSerializer();
            await serializer.SerializeAsync(bsonWriter, c1);
            await bsonWriter.FlushAsync();
            memoryStream.Position = 0;

            var bsonReader = new BsonReader(memoryStream);

            UriGuidTimeSpanTestClass c2 = await serializer.DeserializeAsync<UriGuidTimeSpanTestClass>(bsonReader);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public async Task UriGuidTimeSpanTestClassValuesTestAsync()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass
            {
                Guid = new Guid("1924129C-F7E0-40F3-9607-9939C531395A"),
                NullableGuid = new Guid("9E9F3ADF-E017-4F72-91E0-617EBE85967D"),
                TimeSpan = TimeSpan.FromDays(1),
                NullableTimeSpan = TimeSpan.FromHours(1),
                Uri = new Uri("http://testuri.com")
            };

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            JsonSerializer serializer = new JsonSerializer();
            await serializer.SerializeAsync(bsonWriter, c1);
            await bsonWriter.FlushAsync();
            memoryStream.Position = 0;

            var bsonReader = new BsonReader(memoryStream);

            UriGuidTimeSpanTestClass c2 = await serializer.DeserializeAsync<UriGuidTimeSpanTestClass>(bsonReader);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public async Task DeserializeByteArrayWithTypeNameHandlingAsync()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            byte[] objectBytes;
            using (MemoryStream bsonStream = new MemoryStream())
            using (JsonWriter bsonWriter = new BsonWriter(bsonStream))
            {
                await serializer.SerializeAsync(bsonWriter, test);
                await bsonWriter.FlushAsync();

                objectBytes = bsonStream.ToArray();
            }

            using (MemoryStream bsonStream = new MemoryStream(objectBytes))
            using (JsonReader bsonReader = new BsonReader(bsonStream))
            {
                // Get exception here
                TestObject newObject = (TestObject)await serializer.DeserializeAsync(bsonReader);

                Assert.AreEqual("Test", newObject.Name);
                CollectionAssert.AreEquivalent(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }
        
        [Test]
        public async Task Utf8TextAsync()
        {
            string badText = System.IO.File.ReadAllText(@"PoisonText.txt");
            var j = new JObject();
            j["test"] = badText;

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            await j.WriteToAsync(bsonWriter);
            await bsonWriter.FlushAsync();

            memoryStream.Position = 0;
            JObject o = await JObject.LoadAsync(new BsonReader(memoryStream));

            Assert.AreEqual(badText, (string)o["test"]);
        }

#if !PORTABLE || NETSTANDARD1_1
        public class BigIntegerTestClass
        {
            public BigInteger Blah { get; set; }
        }

        [Test]
        public async Task WriteBigIntegerAsync()
        {
            BigInteger i = BigInteger.Parse("1999999999999999999999999999999999999999999999999999999999990");

            byte[] data = HexToBytes("2A-00-00-00-05-42-6C-61-68-00-1A-00-00-00-00-F6-FF-FF-FF-FF-FF-FF-1F-B2-21-CB-28-59-84-C4-AE-03-8A-44-34-2F-4C-4E-9E-3E-01-00");
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            BigIntegerTestClass c = await serializer.DeserializeAsync<BigIntegerTestClass>(reader);

            Assert.AreEqual(i, c.Blah);
        }
#endif

        public class RegexTestClass
        {
            public Regex Regex { get; set; }
        }

        [Test]
        public async Task DeserializeRegexNonConverterBsonAsync()
        {
            string hex = "46-00-00-00-03-52-65-67-65-78-00-3A-00-00-00-02-70-61-74-74-65-72-6E-00-05-00-00-00-28-68-69-29-00-10-6F-70-74-69-6F-6E-73-00-05-00-00-00-12-6D-61-74-63-68-54-69-6D-65-6F-75-74-00-F0-D8-FF-FF-FF-FF-FF-FF-00-00";
            byte[] data = HexToBytes(hex);
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            RegexTestClass c = await serializer.DeserializeAsync<RegexTestClass>(reader);

            Assert.AreEqual("(hi)", c.Regex.ToString());
            Assert.AreEqual(RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase, c.Regex.Options);
        }

        [Test]
        public async Task DeserializeRegexBsonAsync()
        {
            string hex = "15-00-00-00-0B-52-65-67-65-78-00-28-68-69-29-00-69-75-78-00-00";
            byte[] data = HexToBytes(hex);
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            RegexTestClass c = await serializer.DeserializeAsync<RegexTestClass>(reader);

            Assert.AreEqual("(hi)", c.Regex.ToString());
            Assert.AreEqual(RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase, c.Regex.Options);
        }

        class Zoo
        {
            public List<Animal> Animals { get; set; }
        }

        class Animal
        {
            public Animal(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        class Dog : Animal
        {
            public Dog(string name)
                : base(name)
            {
            }
        }

        class Cat : Animal
        {
            public Cat(string name)
                : base(name)
            {
            }
        }

        public class MyBinder : DefaultSerializationBinder
        {
            public bool BindToTypeCalled { get; set; }

#if !(NET20 || NET35)
            public bool BindToNameCalled { get; set; }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                BindToNameCalled = true;
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
#endif

            public override Type BindToType(string assemblyName, string typeName)
            {
                BindToTypeCalled = true;
                return base.BindToType(assemblyName, typeName);
            }
        }

        [Test]
        public async Task TypeNameHandlingAutoAsync()
        {
            var binder = new MyBinder();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
#pragma warning disable CS0618 // Type or member is obsolete
                Binder = binder
#pragma warning restore CS0618 // Type or member is obsolete
            };

            Zoo zoo = new Zoo
            {
                Animals = new List<Animal>
                {
                    new Dog("Dog!")
                }
            };

            JsonSerializer serializer = JsonSerializer.Create(settings);

            MemoryStream ms = new MemoryStream();
            BsonWriter bsonWriter = new BsonWriter(ms);
            await serializer.SerializeAsync(bsonWriter, zoo);

            ms.Seek(0, SeekOrigin.Begin);

            var deserialized = await serializer.DeserializeAsync<Zoo>(new BsonReader(ms));

            Assert.AreEqual(1, deserialized.Animals.Count);
            Assert.AreEqual("Dog!", deserialized.Animals[0].Name);
            Assert.IsTrue(deserialized.Animals[0] is Dog);

#if !(NET20 || NET35)
            Assert.IsTrue(binder.BindToNameCalled);
#endif
            Assert.IsTrue(binder.BindToTypeCalled);
        }

        [Test]
        public async Task GuidsShouldBeProperlyDeserialisedAsync()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("TheGuid");
            await writer.WriteValueAsync(g);
            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(Guid), reader.ValueType);
            Assert.AreEqual(g, (Guid)reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());

            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            ObjectTestClass b = await serializer.DeserializeAsync<ObjectTestClass>(new BsonReader(new MemoryStream(bytes)));
            Assert.AreEqual(typeof(Guid), b.TheGuid.GetType());
            Assert.AreEqual(g, (Guid)b.TheGuid);
        }

        [Test]
        public async Task GuidsShouldBeProperlyDeserialised_AsBytesAsync()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("TheGuid");
            await writer.WriteValueAsync(g);
            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            CollectionAssert.AreEquivalent(g.ToByteArray(), reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);
            CollectionAssert.AreEquivalent(g.ToByteArray(), (byte[])reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());

            JsonSerializer serializer = new JsonSerializer();
            BytesTestClass b = await serializer.DeserializeAsync<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
            CollectionAssert.AreEquivalent(g.ToByteArray(), b.TheGuid);
        }

        [Test]
        public async Task GuidsShouldBeProperlyDeserialised_AsBytes_ReadAheadAsync()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("TheGuid");
            await writer.WriteValueAsync(g);
            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            CollectionAssert.AreEquivalent(g.ToByteArray(), reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);
            CollectionAssert.AreEquivalent(g.ToByteArray(), (byte[])reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());

            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
            BytesTestClass b = await serializer.DeserializeAsync<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
            CollectionAssert.AreEquivalent(g.ToByteArray(), b.TheGuid);
        }

        [Test]
        public async Task DeserializeBsonDocumentWithStringAsync()
        {
            byte[] data = HexToBytes("10-00-00-00-02-62-00-04-00-00-00-61-62-63-00-00");
            JsonSerializer serializer = new JsonSerializer();
            JObject jObj = (JObject) await serializer.DeserializeAsync(new BsonReader(new MemoryStream(data)));
            string stringValue = jObj.Value<string>("b");
            Assert.AreEqual("abc", stringValue);
        }

        [Test]
        public async Task DeserializeBsonDocumentWithGuidAsync()
        {
            byte[] data = HexToBytes("1D-00-00-00-05-62-00-10-00-00-00-04-DF-41-E3-E2-39-EE-BB-4C-86-C0-06-A7-64-33-61-E1-00");
            JsonSerializer serializer = new JsonSerializer();
            JObject jObj = (JObject) await serializer.DeserializeAsync(new BsonReader(new MemoryStream(data)));
            Guid guidValue = jObj.Value<Guid>("b");
            Assert.AreEqual(new Guid("e2e341df-ee39-4cbb-86c0-06a7643361e1"), guidValue);
        }

        public class BytesTestClass
        {
            public byte[] TheGuid { get; set; }
        }

        public class ObjectTestClass
        {
            public object TheGuid { get; set; }
        }
    }
}

#endif
