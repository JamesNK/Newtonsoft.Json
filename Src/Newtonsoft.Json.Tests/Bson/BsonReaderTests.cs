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
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Bson
{
  public class BsonReaderTests : TestFixtureBase
  {
    [Test]
    public void ReadSingleObject()
    {
      byte[] data = MiscellaneousUtils.HexToBytes("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00");
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
      Assert.AreEqual(1, reader.Value);
      Assert.AreEqual(typeof(long), reader.ValueType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ReadObjectBsonFromSite()
    {
      byte[] data = MiscellaneousUtils.HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

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
    }

    [Test]
    public void ReadArrayBsonFromSite()
    {
      byte[] data = MiscellaneousUtils.HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

      MemoryStream ms = new MemoryStream(data);
      BsonReader reader = new BsonReader(ms, true);

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
    }

    [Test]
    public void ReadBytes()
    {
      byte[] data = MiscellaneousUtils.HexToBytes("2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-02-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00");

      MemoryStream ms = new MemoryStream(data);
      BsonReader reader = new BsonReader(ms, true);

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

      string decodedString = Encoding.UTF8.GetString(encodedStringData);
      Assert.AreEqual("Hello world!", decodedString);
    }

    [Test]
    public void ReadOid()
    {
      byte[] data = MiscellaneousUtils.HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

      MemoryStream ms = new MemoryStream(data);
      BsonReader reader = new BsonReader(ms);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("_id", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
      Assert.AreEqual(MiscellaneousUtils.HexToBytes("4ABBED9D1D8B0F0218000001"), reader.Value);
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
    }

    [Test]
    public void ReadNestedArray()
    {
      string hexdoc = "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

      MemoryStream ms = new MemoryStream(data);
      BsonReader reader = new BsonReader(ms);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("_id", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
      Assert.AreEqual(MiscellaneousUtils.HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"), reader.Value);
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
    }

    [Test]
    public void ReadNestedArrayIntoLinq()
    {
      string hexdoc = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-02-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

      BsonReader reader = new BsonReader(new MemoryStream(data));

      JObject o = (JObject)JToken.ReadFrom(reader);
      Assert.AreEqual(3, o.Count);

      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);
      o.WriteTo(writer);
      writer.Flush();

      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());
      Assert.AreEqual(hexdoc, bson);
    }

    [Test]
    public void OidAndBytesAreEqual()
    {
      byte[] data1 = MiscellaneousUtils.HexToBytes(
        "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

      BsonReader reader1 = new BsonReader(new MemoryStream(data1));

      // oid
      JObject o1 = (JObject)JToken.ReadFrom(reader1);

      byte[] data2 = MiscellaneousUtils.HexToBytes(
        "87-00-00-00-05-5F-69-64-00-0C-00-00-00-02-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00");

      BsonReader reader2 = new BsonReader(new MemoryStream(data2));

      // bytes
      JObject o2 = (JObject)JToken.ReadFrom(reader2);

      Assert.IsTrue(o1.DeepEquals(o2));
    }

    [Test]
    public void ReadRegex()
    {
      string hexdoc = "15-00-00-00-0B-72-65-67-65-78-00-74-65-73-74-00-67-69-6D-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
    }

    [Test]
    public void ReadCode()
    {
      string hexdoc = "1A-00-00-00-0D-63-6F-64-65-00-0B-00-00-00-49-20-61-6D-20-63-6F-64-65-21-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
    }

    [Test]
    public void ReadUndefined()
    {
      string hexdoc = "10-00-00-00-06-75-6E-64-65-66-69-6E-65-64-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
    }

    [Test]
    public void ReadLong()
    {
      string hexdoc = "13-00-00-00-12-6C-6F-6E-67-00-FF-FF-FF-FF-FF-FF-FF-7F-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
    }

    [Test]
    public void ReadReference()
    {
      string hexdoc = "1E-00-00-00-0C-6F-69-64-00-04-00-00-00-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
      Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, reader.Value);
      Assert.AreEqual(typeof(byte[]), reader.ValueType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ReadCodeWScope()
    {
      string hexdoc = "75-00-00-00-0F-63-6F-64-65-57-69-74-68-53-63-6F-70-65-00-61-00-00-00-35-00-00-00-66-6F-72-20-28-69-6E-74-20-69-20-3D-20-30-3B-20-69-20-3C-20-31-30-30-30-3B-20-69-2B-2B-29-0D-0A-7B-0D-0A-20-20-61-6C-65-72-74-28-61-72-67-31-29-3B-0D-0A-7D-00-24-00-00-00-02-61-72-67-31-00-15-00-00-00-4A-73-6F-6E-2E-4E-45-54-20-69-73-20-61-77-65-73-6F-6D-65-2E-00-00-00";

      byte[] data = MiscellaneousUtils.HexToBytes(hexdoc);

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
      Assert.AreEqual(@"for (int i = 0; i < 1000; i++)
{
  alert(arg1);
}", reader.Value);

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

      BsonReader reader = new BsonReader(new MemoryStream(MiscellaneousUtils.HexToBytes(bson)));

      StringBuilder largeStringBuilder = new StringBuilder();
      for (int i = 0; i < 100; i++)
      {
        if (i > 0)
          largeStringBuilder.Append("-");

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
    }
  }
}