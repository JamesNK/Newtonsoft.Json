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
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.Serialization;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JTokenReaderTest : TestFixtureBase
  {
#if !PocketPC && !NET20
    [Test]
    public void YahooFinance()
    {
      JObject o =
        new JObject(
          new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
          new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
          new JProperty("Test3", "Test3Value"),
          new JProperty("Test4", null)
        );

      using (JTokenReader jsonReader = new JTokenReader(o))
      {
        IJsonLineInfo lineInfo = jsonReader;

        jsonReader.Read();
        Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
        Assert.AreEqual(false, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test1", jsonReader.Value);
        Assert.AreEqual(false, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
        Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
        Assert.AreEqual(false, lineInfo.HasLineInfo());
        Assert.AreEqual(0, lineInfo.LinePosition);
        Assert.AreEqual(0, lineInfo.LineNumber);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test2", jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
        Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test3", jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
        Assert.AreEqual("Test3Value", jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test4", jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Null, jsonReader.TokenType);
        Assert.AreEqual(null, jsonReader.Value);

        Assert.IsTrue(jsonReader.Read());
        Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

        Assert.IsFalse(jsonReader.Read());
      }

      using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
      {
        Assert.IsTrue(jsonReader.Read());
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test2", jsonReader.Value);

        Assert.IsTrue(jsonReader.Read());
        Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
        Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

        Assert.IsFalse(jsonReader.Read());
      }
    }
#endif

    [Test]
    public void ReadLineInfo()
    {
      string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

      StringReader sr = new StringReader(input);

      JObject o = JObject.Parse(input);

      using (JTokenReader jsonReader = new JTokenReader(o))
      {
        IJsonLineInfo lineInfo = jsonReader;

        Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
        Assert.AreEqual(0, lineInfo.LineNumber);
        Assert.AreEqual(0, lineInfo.LinePosition);
        Assert.AreEqual(false, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
        Assert.AreEqual(1, lineInfo.LineNumber);
        Assert.AreEqual(1, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "CPU");
        Assert.AreEqual(2, lineInfo.LineNumber);
        Assert.AreEqual(6, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Intel");
        Assert.AreEqual(2, lineInfo.LineNumber);
        Assert.AreEqual(14, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "Drives");
        Assert.AreEqual(3, lineInfo.LineNumber);
        Assert.AreEqual(9, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
        Assert.AreEqual(3, lineInfo.LineNumber);
        Assert.AreEqual(11, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "DVD read/writer");
        Assert.AreEqual(4, lineInfo.LineNumber);
        Assert.AreEqual(21, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
        Assert.AreEqual(5, lineInfo.LineNumber);
        Assert.AreEqual(29, lineInfo.LinePosition);
        Assert.AreEqual(true, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
        Assert.AreEqual(0, lineInfo.LineNumber);
        Assert.AreEqual(0, lineInfo.LinePosition);
        Assert.AreEqual(false, lineInfo.HasLineInfo());

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
        Assert.AreEqual(0, lineInfo.LineNumber);
        Assert.AreEqual(0, lineInfo.LinePosition);
        Assert.AreEqual(false, lineInfo.HasLineInfo());
      }
    }

    [Test]
    public void ReadBytes()
    {
      byte[] data = Encoding.UTF8.GetBytes("Hello world!");

      JObject o =
        new JObject(
          new JProperty("Test1", data)
        );

      using (JTokenReader jsonReader = new JTokenReader(o))
      {
        jsonReader.Read();
        Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test1", jsonReader.Value);

        byte[] readBytes = jsonReader.ReadAsBytes();
        Assert.AreEqual(data, readBytes);

        Assert.IsTrue(jsonReader.Read());
        Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

        Assert.IsFalse(jsonReader.Read());
      }
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error reading bytes. Expected bytes but got Integer.")]
    public void ReadBytesFailure()
    {
      JObject o =
        new JObject(
          new JProperty("Test1", 1)
        );

      using (JTokenReader jsonReader = new JTokenReader(o))
      {
        jsonReader.Read();
        Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test1", jsonReader.Value);

        jsonReader.ReadAsBytes();
      }
    }

    public class HasBytes
    {
      public byte[] Bytes { get; set; }
    }

    [Test]
    public void ReadBytesFromString()
    {
      var bytes = new HasBytes { Bytes = new byte[] { 1, 2, 3, 4 } };
      var json = JsonConvert.SerializeObject(bytes);

      TextReader textReader = new StringReader(json);
      JsonReader jsonReader = new JsonTextReader(textReader);

      var jToken = JToken.ReadFrom(jsonReader);

      jsonReader = new JTokenReader(jToken);

      var result2 = (HasBytes)JsonSerializer.Create(null)
                 .Deserialize(jsonReader, typeof(HasBytes));

      Assert.AreEqual(new byte[] { 1, 2, 3, 4 }, result2.Bytes);
    }

    [Test]
    public void ReadBytesFromEmptyString()
    {
      var bytes = new HasBytes { Bytes = new byte[0] };
      var json = JsonConvert.SerializeObject(bytes);

      TextReader textReader = new StringReader(json);
      JsonReader jsonReader = new JsonTextReader(textReader);

      var jToken = JToken.ReadFrom(jsonReader);

      jsonReader = new JTokenReader(jToken);

      var result2 = (HasBytes)JsonSerializer.Create(null)
                 .Deserialize(jsonReader, typeof(HasBytes));

      Assert.AreEqual(new byte[0], result2.Bytes);
    }

    public class ReadAsBytesTestObject
    {
      public byte[] Data;
    }

    [Test]
    public void ReadAsBytesNull()
    {
      JsonSerializer s = new JsonSerializer();

      JToken nullToken = JToken.ReadFrom(new JsonTextReader(new StringReader("{ Data: null }")));
      ReadAsBytesTestObject x = s.Deserialize<ReadAsBytesTestObject>(new JTokenReader(nullToken));
      Assert.IsNull(x.Data);
    }

    [Test]
    public void DeserializeByteArrayWithTypeNameHandling()
    {
      TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

      string json = JsonConvert.SerializeObject(test, Formatting.Indented, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All
        });

      JObject o = JObject.Parse(json);

      JsonSerializer serializer = new JsonSerializer();
      serializer.TypeNameHandling = TypeNameHandling.All;

      using (JsonReader nodeReader = o.CreateReader())
      {
        // Get exception here
        TestObject newObject = (TestObject)serializer.Deserialize(nodeReader);

        Assert.AreEqual("Test", newObject.Name);
        Assert.AreEqual(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
      }
    }
  }
}