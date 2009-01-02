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

namespace Newtonsoft.Json.Tests
{
  public class JsonTextReaderTest : TestFixtureBase
  {
    [Test]
    public void YahooFinance()
    {
      string input = @"{
""matches"" : [
{""t"":""C"", ""n"":""Citigroup Inc."", ""e"":""NYSE"", ""id"":""662713""}
,{""t"":""CHL"", ""n"":""China Mobile Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""660998""}
,{""t"":""PTR"", ""n"":""PetroChina Company Limited (ADR)"", ""e"":""NYSE"", ""id"":""664536""}
,{""t"":""RIO"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""671472""}
,{""t"":""RIOPR"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""3512643""}
,{""t"":""CSCO"", ""n"":""Cisco Systems, Inc."", ""e"":""NASDAQ"", ""id"":""99624""}
,{""t"":""CVX"", ""n"":""Chevron Corporation"", ""e"":""NYSE"", ""id"":""667226""}
,{""t"":""TM"", ""n"":""Toyota Motor Corporation (ADR)"", ""e"":""NYSE"", ""id"":""655880""}
,{""t"":""JPM"", ""n"":""JPMorgan Chase \x26 Co."", ""e"":""NYSE"", ""id"":""665639""}
,{""t"":""COP"", ""n"":""ConocoPhillips"", ""e"":""NYSE"", ""id"":""1691168""}
,{""t"":""LFC"", ""n"":""China Life Insurance Company Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""688679""}
,{""t"":""NOK"", ""n"":""Nokia Corporation (ADR)"", ""e"":""NYSE"", ""id"":""657729""}
,{""t"":""KO"", ""n"":""The Coca-Cola Company"", ""e"":""NYSE"", ""id"":""6550""}
,{""t"":""VZ"", ""n"":""Verizon Communications Inc."", ""e"":""NYSE"", ""id"":""664887""}
,{""t"":""AMX"", ""n"":""America Movil S.A.B de C.V. (ADR)"", ""e"":""NYSE"", ""id"":""665834""}],
""all"" : false
}
";

      using (JsonReader jsonReader = new JsonTextReader(new StringReader(input)))
      {
        while (jsonReader.Read())
        {
          Console.WriteLine(jsonReader.Value);
        }
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
    public void Depth()
    {
      string input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher',array:[1,2]}";

      StringReader sr = new StringReader(input);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        Assert.AreEqual(0, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
        Assert.AreEqual(0, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Purple\r \n monkey's:\tdishwasher");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
        Assert.AreEqual(2, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(3, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(3, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
        Assert.AreEqual(0, jsonReader.Depth);
      }
    }

    [Test]
    public void ReadingEscapedStrings()
    {
      string input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

      StringReader sr = new StringReader(input);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        Assert.AreEqual(0, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
        Assert.AreEqual(0, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Purple\r \n monkey's:\tdishwasher");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
        Assert.AreEqual(0, jsonReader.Depth);
      }
    }

    [Test]
    public void ReadNewlineLastCharacter()
    {
      string input = @"{
  CPU: 'Intel',
  Drives: [ /* Com*ment */
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
    public void LongStringTest()
    {
      int length = 20000;
      string json = @"[""" + new string(' ', length) + @"""]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.Read();
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(typeof(string), reader.ValueType);
      Assert.AreEqual(20000, reader.Value.ToString().Length);

      reader.Read();
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }

    [Test]
    public void EscapedUnicodeText()
    {
      string json = @"[""\u003c"",""\u5f20""]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
  
      reader.Read();
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
      
      reader.Read();
      Assert.AreEqual("<", reader.Value);

      reader.Read();
      Assert.AreEqual(24352, Convert.ToInt32(Convert.ToChar((string)reader.Value)));

      reader.Read();
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }

    [Test]
    public void ReadFloatingPointNumber()
    {
      string json =
        @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN]";

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
        Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);
      }
    }
  }
}