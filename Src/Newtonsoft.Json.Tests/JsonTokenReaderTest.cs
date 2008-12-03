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

namespace Newtonsoft.Json.Tests
{
  public class JsonTokenReaderTest : TestFixtureBase
  {
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

      using (JsonReader jsonReader = new JsonTokenReader(o))
      {
        jsonReader.Read();
        Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual("Test1", jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
        Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);

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

      using (JsonReader jsonReader = new JsonTokenReader(o.Property("Test2")))
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

    [Test]
    public void ReadingIndented()
    {
      string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

      StringReader sr = new StringReader(input);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        Assert.AreEqual(jsonReader.TokenType, JsonToken.None);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "CPU");

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Intel");

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "Drives");

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "DVD read/writer");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
        Assert.AreEqual(jsonReader.QuoteChar, '"');

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
      }
    }

    [Test]
    public void ReadingEscapedStrings()
    {
      string input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

      StringReader sr = new StringReader(input);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Purple\r \n monkey's:\tdishwasher");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
      }
    }

    [Test]
    public void ReadNewlineLastCharacter()
    {
      string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}" + '\n';

      object o = JsonConvert.DeserializeObject(input);
    }

    [Test]
    public void WriteReadWrite()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(true);

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("integer");
        jsonWriter.WriteValue(99);
        jsonWriter.WritePropertyName("string");
        jsonWriter.WriteValue("how now brown cow?");
        jsonWriter.WritePropertyName("array");

        jsonWriter.WriteStartArray();
        for (int i = 0; i < 5; i++)
        {
          jsonWriter.WriteValue(i);
        }

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("decimal");
        jsonWriter.WriteValue(990.00990099m);
        jsonWriter.WriteEndObject();

        jsonWriter.WriteValue(5);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        jsonWriter.WriteValue("This is a string.");
        jsonWriter.WriteNull();
        jsonWriter.WriteNull();
        jsonWriter.WriteEndArray();
      }

      string json = sb.ToString();

      JsonSerializer serializer = new JsonSerializer();

      object jsonObject = serializer.Deserialize(new JsonTextReader(new StringReader(json)));

      sb = new StringBuilder();
      sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        serializer.Serialize(sw, jsonObject);
      }

      Assert.AreEqual(json, sb.ToString());
    }
  }
}