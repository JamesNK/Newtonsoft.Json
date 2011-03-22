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
using System.Xml;

namespace Newtonsoft.Json.Tests
{
  public class JsonTextReaderTest : TestFixtureBase
  {
    [Test]
    public void CloseInput()
    {
      MemoryStream ms = new MemoryStream();
      JsonTextReader reader = new JsonTextReader(new StreamReader(ms));

      Assert.IsTrue(ms.CanRead);
      reader.Close();
      Assert.IsFalse(ms.CanRead);

      ms = new MemoryStream();
      reader = new JsonTextReader(new StreamReader(ms)) { CloseInput = false };

      Assert.IsTrue(ms.CanRead);
      reader.Close();
      Assert.IsTrue(ms.CanRead);
    }
    
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
,{""t"":""JPM"", ""n"":""JPMorgan Chase \\x26 Co."", ""e"":""NYSE"", ""id"":""665639""}
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

      using (JsonTextReader jsonReader = new JsonTextReader(sr))
      {
        Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
        Assert.AreEqual(0, jsonReader.LineNumber);
        Assert.AreEqual(0, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
        Assert.AreEqual(1, jsonReader.LineNumber);
        Assert.AreEqual(1, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "CPU");
        Assert.AreEqual(2, jsonReader.LineNumber);
        Assert.AreEqual(6, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "Intel");
        Assert.AreEqual(2, jsonReader.LineNumber);
        Assert.AreEqual(14, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "Drives");
        Assert.AreEqual(3, jsonReader.LineNumber);
        Assert.AreEqual(9, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
        Assert.AreEqual(3, jsonReader.LineNumber);
        Assert.AreEqual(11, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "DVD read/writer");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');
        Assert.AreEqual(4, jsonReader.LineNumber);
        Assert.AreEqual(21, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
        Assert.AreEqual(jsonReader.QuoteChar, '"');
        Assert.AreEqual(5, jsonReader.LineNumber);
        Assert.AreEqual(29, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
        Assert.AreEqual(6, jsonReader.LineNumber);
        Assert.AreEqual(3, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
        Assert.AreEqual(7, jsonReader.LineNumber);
        Assert.AreEqual(1, jsonReader.LinePosition);

        Assert.IsFalse(jsonReader.Read());
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
        Assert.AreEqual(1, jsonReader.Value);
        Assert.AreEqual(3, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(2, jsonReader.Value);
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
    [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = @"Value cannot be null.
Parameter name: reader")]
    public void NullTextReader()
    {
      new JsonTextReader(null);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unterminated string. Expected delimiter: '. Line 1, position 3.")]
    public void UnexpectedEndOfString()
    {
      JsonReader reader = new JsonTextReader(new StringReader("'hi"));
      reader.Read();
    }

    [Test]
    public void ReadNullTerminatorStrings()
    {
      JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
      Assert.IsTrue(reader.Read());

      Assert.AreEqual("h\0i", reader.Value);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end while parsing unicode character. Line 1, position 7.")]
    public void UnexpectedEndOfHex()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"'h\u006"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unterminated string. Expected delimiter: '. Line 1, position 3.")]
    public void UnexpectedEndOfControlCharacter()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected token when reading bytes: Boolean. Line 1, position 4.")]
    public void ReadBytesWithBadCharacter()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"true"));
      reader.ReadAsBytes();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unterminated string. Expected delimiter: '. Line 1, position 17.")]
    public void ReadBytesWithUnexpectedEnd()
    {
      string helloWorld = "Hello world!";
      byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

      JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));
      reader.ReadAsBytes();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading bytes: Line 1, position 3.")]
    public void ReadBytesNoStartWithUnexpectedEnd()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
      Assert.IsTrue(reader.Read());
      reader.ReadAsBytes();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when parsing unquoted property name. Line 1, position 4.")]
    public void UnexpectedEndWhenParsingUnquotedProperty()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
      Assert.IsTrue(reader.Read());
      reader.Read();
    }

    [Test]
    public void ParsingQuotedPropertyWithControlCharacters()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"{'hi\r\nbye':1}"));
      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual(@"hi
bye", reader.Value);
      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ReadBytesFollowingNumberInArray()
    {
      string helloWorld = "Hello world!";
      byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

      JsonReader reader = new JsonTextReader(new StringReader(@"[1,'" + Convert.ToBase64String(helloWorldData) + @"']"));
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      byte[] data = reader.ReadAsBytes();
      Assert.AreEqual(helloWorldData, data);
      Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ReadBytesFollowingNumberInObject()
    {
      string helloWorld = "Hello world!";
      byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

      JsonReader reader = new JsonTextReader(new StringReader(@"{num:1,data:'" + Convert.ToBase64String(helloWorldData) + @"'}"));
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.IsTrue(reader.Read());
      byte[] data = reader.ReadAsBytes();
      Assert.AreEqual(helloWorldData, data);
      Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
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

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = @"Invalid character after parsing property name. Expected ':' but got: "". Line 3, position 9.")]
    public void MissingColon()
    {
      string json = @"{
    ""A"" : true,
    ""B"" ""hello"", // Notice the colon is missing
    ""C"" : ""bye""
}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.Read();
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

      reader.Read();
    }

    [Test]
    public void ReadSingleBytes()
    {
      StringReader s = new StringReader(@"""SGVsbG8gd29ybGQu""");
      JsonTextReader reader = new JsonTextReader(s);

      byte[] data = reader.ReadAsBytes();
      Assert.IsNotNull(data);

      string text = Encoding.UTF8.GetString(data, 0, data.Length);
      Assert.AreEqual("Hello world.", text);
    }

    [Test]
    public void ReadOctalNumber()
    {
      StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
      JsonTextReader jsonReader = new JsonTextReader(s);

      Assert.IsTrue(jsonReader.Read());
      Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

      Assert.IsTrue(jsonReader.Read());
      Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
      Assert.AreEqual(250, jsonReader.Value);

      Assert.IsTrue(jsonReader.Read());
      Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
      Assert.AreEqual(250, jsonReader.Value);

      Assert.IsTrue(jsonReader.Read());
      Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
      Assert.AreEqual(250, jsonReader.Value);

      Assert.IsTrue(jsonReader.Read());
      Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

      Assert.IsFalse(jsonReader.Read());
    }

    [Test]
    public void ReadUnicode()
    {
      string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("Message", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(@"Hi,I" + '\u0092' + "ve send you smth", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ReadHexidecimalWithAllLetters()
    {
      string json = @"{""text"":0xabcdef12345}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual(11806310474565, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

#if !NET20
    [Test]
    public void ReadAsDateTimeOffset()
    {
      string json = "{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
      Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadAsDateTimeOffsetNegative()
    {
      string json = @"{""Offset"":""\/Date(946706400000-0600)\/""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
      Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadAsDateTimeOffsetHoursOnly()
    {
      string json = "{\"Offset\":\"\\/Date(946663200000+06)\\/\"}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
      Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadAsDateTimeOffsetWithMinutes()
    {
      string json = @"{""Offset"":""\/Date(946708260000-0631)\/""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
      Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }
#endif

    [Test]
    public void ReadAsDecimal()
    {
      string json = @"{""decimal"":-7.92281625142643E+28}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      decimal? d = reader.ReadAsDecimal();
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(typeof(decimal), reader.ValueType);
      Assert.AreEqual(-79228162514264300000000000000m, d);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }
  }
}