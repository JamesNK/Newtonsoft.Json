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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json.Utilities;

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
  }
}