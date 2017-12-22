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

#pragma warning disable 618
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if !(NET20 || NET35 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
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
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Bson
{
    [TestFixture]
    public class BsonReaderTests : TestFixtureBase
    {
        private const char Euro = '\u20ac';

        [Test]
        public void DeserializeLargeBsonObject()
        {
            byte[] data = System.IO.File.ReadAllBytes(ResolvePath(@"SpaceShipV2.bson"));

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            JObject o = (JObject)JToken.ReadFrom(reader);

            Assert.AreEqual("1", (string)o["$id"]);
        }

        [Test]
        public void CloseInput()
        {
            MemoryStream ms = new MemoryStream();
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(ms.CanRead);
            reader.Close();
            Assert.IsFalse(ms.CanRead);

            ms = new MemoryStream();
            reader = new BsonReader(ms) { CloseInput = false };

            Assert.IsTrue(ms.CanRead);
            reader.Close();
            Assert.IsTrue(ms.CanRead);
        }

        [Test]
        public void ReadSingleObject()
        {
            byte[] data = HexToBytes("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00");
            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Blah", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadGuid_Text()
        {
            byte[] data = HexToBytes("31-00-00-00-02-30-00-25-00-00-00-64-38-32-31-65-65-64-37-2D-34-62-35-63-2D-34-33-63-39-2D-38-61-63-32-2D-36-39-32-38-65-35-37-39-62-37-30-35-00-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("d821eed7-4b5c-43c9-8ac2-6928e579b705", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            ms = new MemoryStream(data);
            reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            JsonSerializer serializer = new JsonSerializer();
            IList<Guid> l = serializer.Deserialize<IList<Guid>>(reader);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705"), l[0]);
        }

        [Test]
        public void ReadGuid_Bytes()
        {
            byte[] data = HexToBytes("1D-00-00-00-05-30-00-10-00-00-00-04-D7-EE-21-D8-5C-4B-C9-43-8A-C2-69-28-E5-79-B7-05-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Guid g = new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705");

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(g, reader.Value);
            Assert.AreEqual(typeof(Guid), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            ms = new MemoryStream(data);
            reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            JsonSerializer serializer = new JsonSerializer();
            IList<Guid> l = serializer.Deserialize<IList<Guid>>(reader);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(g, l[0]);
        }

        [Test]
        public void ReadDouble()
        {
            byte[] data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(99.99d, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadDouble_Decimal()
        {
            byte[] data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.FloatParseHandling = FloatParseHandling.Decimal;
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(99.99m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadValues()
        {
            byte[] data = HexToBytes("8C-00-00-00-12-30-00-FF-FF-FF-FF-FF-FF-FF-7F-12-31-00-FF-FF-FF-FF-FF-FF-FF-7F-10-32-00-FF-FF-FF-7F-10-33-00-FF-FF-FF-7F-10-34-00-FF-00-00-00-10-35-00-7F-00-00-00-02-36-00-02-00-00-00-61-00-01-37-00-00-00-00-00-00-00-F0-45-01-38-00-FF-FF-FF-FF-FF-FF-EF-7F-01-39-00-00-00-00-E0-FF-FF-EF-47-08-31-30-00-01-05-31-31-00-05-00-00-00-02-00-01-02-03-04-09-31-32-00-40-C5-E2-BA-E3-00-00-00-09-31-33-00-40-C5-E2-BA-E3-00-00-00-00");
            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);
            reader.JsonNet35BinaryCompatibility = true;
            reader.ReadRootValueAsArray = true;
            reader.DateTimeKindHandling = DateTimeKind.Utc;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)int.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)int.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)byte.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual((long)sbyte.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual((double)decimal.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual((double)double.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual((double)float.MaxValue, reader.Value);
            Assert.AreEqual(typeof(double), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);
            Assert.AreEqual(typeof(bool), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(new byte[] { 0, 1, 2, 3, 4 }, (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadObjectBsonFromSite()
        {
            byte[] data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("0", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("1", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("2", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("c", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadArrayBsonFromSite()
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

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("c", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadAsInt32BadString()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
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

                Assert.IsTrue(reader.Read());
                Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

                reader.ReadAsInt32();
            }, "Could not convert string to integer: a. Path '[0]'.");
        }

        [Test]
        public void ReadBytes()
        {
            byte[] data = HexToBytes("2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-02-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms, true, DateTimeKind.Utc);
            reader.JsonNet35BinaryCompatibility = true;

            Assert.AreEqual(true, reader.ReadRootValueAsArray);
            Assert.AreEqual(DateTimeKind.Utc, reader.DateTimeKindHandling);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("a", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("b", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            byte[] encodedStringData = reader.ReadAsBytes();
            Assert.IsNotNull(encodedStringData);
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(encodedStringData, reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            string decodedString = Encoding.UTF8.GetString(encodedStringData, 0, encodedStringData.Length);
            Assert.AreEqual("Hello world!", decodedString);
        }

        [Test]
        public void ReadOid()
        {
            byte[] data = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("_id", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(HexToBytes("4ABBED9D1D8B0F0218000001"), (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("test", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("1234£56", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadNestedArray()
        {
            string hexdoc = "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("_id", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"), (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("a", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            for (int i = 1; i <= 8; i++)
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(JsonToken.Float, reader.TokenType);

                double value = (i != 5)
                    ? Convert.ToDouble(i)
                    : 5.78960446186581E+77d;

                Assert.AreEqual(value, reader.Value);
            }

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("b", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("test", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadNestedArrayIntoLinq()
        {
            string hexdoc = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

            byte[] data = HexToBytes(hexdoc);

            BsonReader reader = new BsonReader(new MemoryStream(data));
            reader.JsonNet35BinaryCompatibility = true;

            JObject o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(3, o.Count);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            o.WriteTo(writer);
            writer.Flush();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual(hexdoc, bson);
        }

        [Test]
        public void OidAndBytesAreEqual()
        {
            byte[] data1 = HexToBytes(
                "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

            BsonReader reader1 = new BsonReader(new MemoryStream(data1));
            reader1.JsonNet35BinaryCompatibility = true;

            // oid
            JObject o1 = (JObject)JToken.ReadFrom(reader1);

            byte[] data2 = HexToBytes(
                "87-00-00-00-05-5F-69-64-00-0C-00-00-00-02-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

            BsonReader reader2 = new BsonReader(new MemoryStream(data2));
            reader2.JsonNet35BinaryCompatibility = true;

            // bytes
            JObject o2 = (JObject)JToken.ReadFrom(reader2);

            Assert.IsTrue(o1.DeepEquals(o2));
        }

        [Test]
        public void ReadRegex()
        {
            string hexdoc = "15-00-00-00-0B-72-65-67-65-78-00-74-65-73-74-00-67-69-6D-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("regex", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"/test/gim", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadCode()
        {
            string hexdoc = "1A-00-00-00-0D-63-6F-64-65-00-0B-00-00-00-49-20-61-6D-20-63-6F-64-65-21-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("code", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"I am code!", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadUndefined()
        {
            string hexdoc = "10-00-00-00-06-75-6E-64-65-66-69-6E-65-64-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("undefined", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);
            Assert.AreEqual(null, reader.Value);
            Assert.AreEqual(null, reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadLong()
        {
            string hexdoc = "13-00-00-00-12-6C-6F-6E-67-00-FF-FF-FF-FF-FF-FF-FF-7F-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("long", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(long.MaxValue, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadReference()
        {
            string hexdoc = "1E-00-00-00-0C-6F-69-64-00-04-00-00-00-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("oid", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$ref", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("oid", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$id", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, (byte[])reader.Value);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadCodeWScope()
        {
            string hexdoc = "75-00-00-00-0F-63-6F-64-65-57-69-74-68-53-63-6F-70-65-00-61-00-00-00-35-00-00-00-66-6F-72-20-28-69-6E-74-20-69-20-3D-20-30-3B-20-69-20-3C-20-31-30-30-30-3B-20-69-2B-2B-29-0D-0A-7B-0D-0A-20-20-61-6C-65-72-74-28-61-72-67-31-29-3B-0D-0A-7D-00-24-00-00-00-02-61-72-67-31-00-15-00-00-00-4A-73-6F-6E-2E-4E-45-54-20-69-73-20-61-77-65-73-6F-6D-65-2E-00-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("codeWithScope", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$code", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("for (int i = 0; i < 1000; i++)\r\n{\r\n  alert(arg1);\r\n}", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("$scope", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("arg1", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("Json.NET is awesome.", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadEndOfStream()
        {
            BsonReader reader = new BsonReader(new MemoryStream());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void ReadLargeStrings()
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

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(largeString, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(largeString, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadEmptyStrings()
        {
            string bson = "0C-00-00-00-02-00-01-00-00-00-00-00";

            BsonReader reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void WriteAndReadEmptyListsAndDictionaries()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("Arguments");
            writer.WriteStartObject();
            writer.WriteEndObject();
            writer.WritePropertyName("List");
            writer.WriteStartArray();
            writer.WriteEndArray();
            writer.WriteEndObject();

            string bson = BitConverter.ToString(ms.ToArray());

            Assert.AreEqual("20-00-00-00-03-41-72-67-75-6D-65-6E-74-73-00-05-00-00-00-00-04-4C-69-73-74-00-05-00-00-00-00-00", bson);

            BsonReader reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Arguments", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("List", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void DateTimeKindHandling()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("DateTime");
            writer.WriteValue(value);
            writer.WriteEndObject();

            byte[] bson = ms.ToArray();

            JObject o;
            BsonReader reader;

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Utc);
            o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Local);
            o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(value.ToLocalTime(), (DateTime)o["DateTime"]);

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Unspecified);
            o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), (DateTime)o["DateTime"]);
        }

        [Test]
        public void UnspecifiedDateTimeKindHandling()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.DateTimeKindHandling = DateTimeKind.Unspecified;

            writer.WriteStartObject();
            writer.WritePropertyName("DateTime");
            writer.WriteValue(value);
            writer.WriteEndObject();

            byte[] bson = ms.ToArray();

            JObject o;
            BsonReader reader;

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Unspecified);
            o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);
        }

        [Test]
        public void LocalDateTimeKindHandling()
        {
            DateTime value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("DateTime");
            writer.WriteValue(value);
            writer.WriteEndObject();

            byte[] bson = ms.ToArray();

            JObject o;
            BsonReader reader;

            reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Local);
            o = (JObject)JToken.ReadFrom(reader);
            Assert.AreEqual(value, (DateTime)o["DateTime"]);
        }

        private string WriteAndReadStringValue(string val)
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter bs = new BsonWriter(ms);
            bs.WriteStartObject();
            bs.WritePropertyName("StringValue");
            bs.WriteValue(val);
            bs.WriteEnd();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            // object
            reader.Read();
            // property name
            reader.Read();
            // string
            reader.Read();
            return (string)reader.Value;
        }

        private string WriteAndReadStringPropertyName(string val)
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter bs = new BsonWriter(ms);
            bs.WriteStartObject();
            bs.WritePropertyName(val);
            bs.WriteValue("Dummy");
            bs.WriteEnd();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            // object
            reader.Read();
            // property name
            reader.Read();
            return (string)reader.Value;
        }

        [Test]
        public void TestReadLenStringValueShortTripleByte()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append('1',127); //first char of euro at the end of the boundry.
            //sb.Append(euro, 5);
            //sb.Append('1',128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadLenStringValueTripleByteCharBufferBoundry0()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 127); //first char of euro at the end of the boundry.
            sb.Append(Euro, 5);
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadLenStringValueTripleByteCharBufferBoundry1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 126);
            sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            string result = WriteAndReadStringValue(expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestReadLenStringValueTripleByteCharOne()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Euro, 1); //Just one triple byte char in the string.

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadLenStringValueTripleByteCharBufferBoundry2()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 125);
            sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadStringValue()
        {
            string expected = "test";
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadStringValueLong()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('t', 150);
            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringValue(expected));
        }

        [Test]
        public void TestReadStringPropertyNameShortTripleByte()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append('1',127); //first char of euro at the end of the boundry.
            //sb.Append(euro, 5);
            //sb.Append('1',128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void TestReadStringPropertyNameTripleByteCharBufferBoundry0()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 127); //first char of euro at the end of the boundry.
            sb.Append(Euro, 5);
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            string result = WriteAndReadStringPropertyName(expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestReadStringPropertyNameTripleByteCharBufferBoundry1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 126);
            sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void TestReadStringPropertyNameTripleByteCharOne()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Euro, 1); //Just one triple byte char in the string.

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void TestReadStringPropertyNameTripleByteCharBufferBoundry2()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1', 125);
            sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
            sb.Append('1', 128);
            sb.Append(Euro);

            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void TestReadStringPropertyName()
        {
            string expected = "test";
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void TestReadStringPropertyNameLong()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('t', 150);
            string expected = sb.ToString();
            Assert.AreEqual(expected, WriteAndReadStringPropertyName(expected));
        }

        [Test]
        public void ReadRegexWithOptions()
        {
            string hexdoc = "1A-00-00-00-0B-72-65-67-65-78-00-61-62-63-00-69-00-0B-74-65-73-74-00-00-00-00";

            byte[] data = HexToBytes(hexdoc);

            MemoryStream ms = new MemoryStream(data);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("/abc/i", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("//", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void CanRoundTripStackOverflowData()
        {
            var doc =
                @"{
""AboutMe"": ""<p>I'm the Director for Research and Development for <a href=\""http://www.prophoenix.com\"" rel=\""nofollow\"">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\""http://blog.usepowershell.com\"" rel=\""nofollow\"">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\""http://powershellcommunity.org\"" rel=\""nofollow\"">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n"",
""WebsiteUrl"": ""http://blog.usepowershell.com""
}";
            JObject parsed = JObject.Parse(doc);
            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            parsed.WriteTo(bsonWriter);
            bsonWriter.Flush();
            memoryStream.Position = 0;

            BsonReader reader = new BsonReader(memoryStream);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("AboutMe", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("<p>I'm the Director for Research and Development for <a href=\"http://www.prophoenix.com\" rel=\"nofollow\">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\"http://blog.usepowershell.com\" rel=\"nofollow\">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\"http://powershellcommunity.org\" rel=\"nofollow\">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("WebsiteUrl", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("http://blog.usepowershell.com", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void MultibyteCharacterPropertyNamesAndStrings()
        {
            string json = @"{
  ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ"": ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ""
}";
            JObject parsed = JObject.Parse(json);
            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            parsed.WriteTo(bsonWriter);
            bsonWriter.Flush();
            memoryStream.Position = 0;

            BsonReader reader = new BsonReader(memoryStream);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public void UriGuidTimeSpanTestClassEmptyTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass();

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(bsonWriter, c1);
            bsonWriter.Flush();
            memoryStream.Position = 0;

            var bsonReader = new BsonReader(memoryStream);

            UriGuidTimeSpanTestClass c2 = serializer.Deserialize<UriGuidTimeSpanTestClass>(bsonReader);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public void UriGuidTimeSpanTestClassValuesTest()
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
            serializer.Serialize(bsonWriter, c1);
            bsonWriter.Flush();
            memoryStream.Position = 0;

            var bsonReader = new BsonReader(memoryStream);

            UriGuidTimeSpanTestClass c2 = serializer.Deserialize<UriGuidTimeSpanTestClass>(bsonReader);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public void DeserializeByteArrayWithTypeNameHandling()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            byte[] objectBytes;
            using (MemoryStream bsonStream = new MemoryStream())
            using (JsonWriter bsonWriter = new BsonWriter(bsonStream))
            {
                serializer.Serialize(bsonWriter, test);
                bsonWriter.Flush();

                objectBytes = bsonStream.ToArray();
            }

            using (MemoryStream bsonStream = new MemoryStream(objectBytes))
            using (JsonReader bsonReader = new BsonReader(bsonStream))
            {
                // Get exception here
                TestObject newObject = (TestObject)serializer.Deserialize(bsonReader);

                Assert.AreEqual("Test", newObject.Name);
                CollectionAssert.AreEquivalent(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }

        [Test]
        public void Utf8Text()
        {
            string badText = System.IO.File.ReadAllText(TestFixtureBase.ResolvePath(@"PoisonText.txt"));
            var j = new JObject();
            j["test"] = badText;

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            j.WriteTo(bsonWriter);
            bsonWriter.Flush();

            memoryStream.Position = 0;
            JObject o = JObject.Load(new BsonReader(memoryStream));

            Assert.AreEqual(badText, (string)o["test"]);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        public class BigIntegerTestClass
        {
            public BigInteger Blah { get; set; }
        }

        [Test]
        public void WriteBigInteger()
        {
            BigInteger i = BigInteger.Parse("1999999999999999999999999999999999999999999999999999999999990");

            byte[] data = HexToBytes("2A-00-00-00-05-42-6C-61-68-00-1A-00-00-00-00-F6-FF-FF-FF-FF-FF-FF-1F-B2-21-CB-28-59-84-C4-AE-03-8A-44-34-2F-4C-4E-9E-3E-01-00");
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            BigIntegerTestClass c = serializer.Deserialize<BigIntegerTestClass>(reader);

            Assert.AreEqual(i, c.Blah);
        }
#endif

        public class RegexTestClass
        {
            public Regex Regex { get; set; }
        }

        [Test]
        public void DeserializeRegexNonConverterBson()
        {
            string hex = "46-00-00-00-03-52-65-67-65-78-00-3A-00-00-00-02-70-61-74-74-65-72-6E-00-05-00-00-00-28-68-69-29-00-10-6F-70-74-69-6F-6E-73-00-05-00-00-00-12-6D-61-74-63-68-54-69-6D-65-6F-75-74-00-F0-D8-FF-FF-FF-FF-FF-FF-00-00";
            byte[] data = HexToBytes(hex);
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

            Assert.AreEqual("(hi)", c.Regex.ToString());
            Assert.AreEqual(RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase, c.Regex.Options);
        }

        [Test]
        public void DeserializeRegexBson()
        {
            string hex = "15-00-00-00-0B-52-65-67-65-78-00-28-68-69-29-00-69-75-78-00-00";
            byte[] data = HexToBytes(hex);
            MemoryStream ms = new MemoryStream(data);

            BsonReader reader = new BsonReader(ms);

            JsonSerializer serializer = new JsonSerializer();

            RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

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
        public void TypeNameHandlingAuto()
        {
            var binder = new MyBinder();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = binder
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
            serializer.Serialize(bsonWriter, zoo);

            ms.Seek(0, SeekOrigin.Begin);

            var deserialized = serializer.Deserialize<Zoo>(new BsonReader(ms));

            Assert.AreEqual(1, deserialized.Animals.Count);
            Assert.AreEqual("Dog!", deserialized.Animals[0].Name);
            Assert.IsTrue(deserialized.Animals[0] is Dog);

#if !(NET20 || NET35)
            Assert.IsTrue(binder.BindToNameCalled);
#endif
            Assert.IsTrue(binder.BindToTypeCalled);
        }

        [Test]
        public void GuidsShouldBeProperlyDeserialised()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("TheGuid");
            writer.WriteValue(g);
            writer.WriteEndObject();
            writer.Flush();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(Guid), reader.ValueType);
            Assert.AreEqual(g, (Guid)reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());

            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            ObjectTestClass b = serializer.Deserialize<ObjectTestClass>(new BsonReader(new MemoryStream(bytes)));
            Assert.AreEqual(typeof(Guid), b.TheGuid.GetType());
            Assert.AreEqual(g, (Guid)b.TheGuid);
        }

        [Test]
        public void GuidsShouldBeProperlyDeserialised_AsBytes()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("TheGuid");
            writer.WriteValue(g);
            writer.WriteEndObject();
            writer.Flush();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            CollectionAssert.AreEquivalent(g.ToByteArray(), reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);
            CollectionAssert.AreEquivalent(g.ToByteArray(), (byte[])reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());

            JsonSerializer serializer = new JsonSerializer();
            BytesTestClass b = serializer.Deserialize<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
            CollectionAssert.AreEquivalent(g.ToByteArray(), b.TheGuid);
        }

        [Test]
        public void GuidsShouldBeProperlyDeserialised_AsBytes_ReadAhead()
        {
            Guid g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("TheGuid");
            writer.WriteValue(g);
            writer.WriteEndObject();
            writer.Flush();

            byte[] bytes = ms.ToArray();

            BsonReader reader = new BsonReader(new MemoryStream(bytes));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            CollectionAssert.AreEquivalent(g.ToByteArray(), reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.AreEqual(typeof(byte[]), reader.ValueType);
            CollectionAssert.AreEquivalent(g.ToByteArray(), (byte[])reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());

            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
            BytesTestClass b = serializer.Deserialize<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
            CollectionAssert.AreEquivalent(g.ToByteArray(), b.TheGuid);
        }

        [Test]
        public void DeserializeBsonDocumentWithString()
        {
            byte[] data = HexToBytes("10-00-00-00-02-62-00-04-00-00-00-61-62-63-00-00");
            JsonSerializer serializer = new JsonSerializer();
            JObject jObj = (JObject)serializer.Deserialize(new BsonReader(new MemoryStream(data)));
            string stringValue = jObj.Value<string>("b");
            Assert.AreEqual("abc", stringValue);
        }

        [Test]
        public void DeserializeBsonDocumentWithGuid()
        {
            byte[] data = HexToBytes("1D-00-00-00-05-62-00-10-00-00-00-04-DF-41-E3-E2-39-EE-BB-4C-86-C0-06-A7-64-33-61-E1-00");
            JsonSerializer serializer = new JsonSerializer();
            JObject jObj = (JObject)serializer.Deserialize(new BsonReader(new MemoryStream(data)));
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
#pragma warning restore 618