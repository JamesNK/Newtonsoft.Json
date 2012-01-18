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
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Utilities;

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
    public void ReadConstructor()
    {
      string json = @"{""DefaultConverter"":new Date(0, ""hi""),""MemberConverter"":""1970-01-01T00:00:00Z""}";

      JsonReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
      Assert.AreEqual("Date", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(0, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual("hi", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual("MemberConverter", reader.Value);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Additional text encountered after finished reading JSON content: ,. Line 5, position 2.")]
    public void ParseAdditionalContent_Comma()
    {
      string json = @"[
""Small"",
""Medium"",
""Large""
],";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      while (reader.Read())
      {
      }
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Additional text encountered after finished reading JSON content: c. Line 5, position 2.")]
    public void ParseAdditionalContent_Text()
    {
      string json = @"[
""Small"",
""Medium"",
""Large""
]content";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[2]);

      reader.Read();
      Assert.AreEqual(1, reader.LineNumber);

      reader.Read();
      Assert.AreEqual(2, reader.LineNumber);

      reader.Read();
      Assert.AreEqual(3, reader.LineNumber);

      reader.Read();
      Assert.AreEqual(4, reader.LineNumber);

      reader.Read();
      Assert.AreEqual(5, reader.LineNumber);

      reader.Read();
    }

    [Test]
    public void ParseAdditionalContent_Whitespace()
    {
      string json = @"[
""Small"",
""Medium"",
""Large""
]   

";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      while (reader.Read())
      {
      }
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Additional text encountered after finished reading JSON content: a. Line 1, position 5.")]
    public void ParseAdditionalContent_WhitespaceThenText()
    {
      string json = @"'hi' a";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      while (reader.Read())
      {
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
        jsonReader.SetCharBuffer(new char[5]);

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
        Assert.AreEqual(7, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
        Assert.AreEqual("Intel", jsonReader.Value);
        Assert.AreEqual(2, jsonReader.LineNumber);
        Assert.AreEqual(15, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(jsonReader.Value, "Drives");
        Assert.AreEqual(3, jsonReader.LineNumber);
        Assert.AreEqual(10, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
        Assert.AreEqual(3, jsonReader.LineNumber);
        Assert.AreEqual(12, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "DVD read/writer");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');
        Assert.AreEqual(4, jsonReader.LineNumber);
        Assert.AreEqual(22, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
        Assert.AreEqual(jsonReader.QuoteChar, '"');
        Assert.AreEqual(5, jsonReader.LineNumber);
        Assert.AreEqual(30, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
        Assert.AreEqual(6, jsonReader.LineNumber);
        Assert.AreEqual(4, jsonReader.LinePosition);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
        Assert.AreEqual(7, jsonReader.LineNumber);
        Assert.AreEqual(2, jsonReader.LinePosition);

        Assert.IsFalse(jsonReader.Read());
      }
    }

    [Test]
    public void Depth()
    {
      string input = @"{
  value:'Purple',
  array:[1,2],
  subobject:{prop:1}
}";

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
        Assert.AreEqual(jsonReader.Value, @"Purple");
        Assert.AreEqual(jsonReader.QuoteChar, '\'');
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(1, jsonReader.Value);
        Assert.AreEqual(2, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(2, jsonReader.Value);
        Assert.AreEqual(2, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.AreEqual(2, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.Integer);
        Assert.AreEqual(2, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
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
    public void ReadLongString()
    {
      string s = new string('a', 10000);
      JsonReader reader = new JsonTextReader(new StringReader("'" + s + "'"));
      reader.Read();

      Assert.AreEqual(s, reader.Value);
    }

    [Test]
    public void ReadLongJsonArray()
    {
      int valueCount = 10000;
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.WriteStartArray();
      for (int i = 0; i < valueCount; i++)
      {
        writer.WriteValue(i);
      }
      writer.WriteEndArray();

      string json = sw.ToString();

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      Assert.IsTrue(reader.Read());
      for (int i = 0; i < valueCount; i++)
      {
        Assert.IsTrue(reader.Read());
        Assert.AreEqual(i, reader.Value);
      }
      Assert.IsTrue(reader.Read());
      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void NullCharReading()
    {
      string json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);
      Assert.AreEqual("\0sd\0f\0", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);
      Assert.AreEqual("\0sd\0f\0", reader.Value);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void AppendCharsWhileReadingNull()
    {
      string json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[129]);

      for (int i = 0; i < 15; i++)
      {
        reader.Read();
      }

      reader.Read();
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
    }

    [Test]
    public void AppendCharsWhileReadingNewLine()
    {
      string json = @"
{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"":
  {
    ""name"": {""type"":""string""},
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {""type"":""string""}
    }
  }
}
";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[129]);

      for (int i = 0; i < 14; i++)
      {
        Assert.IsTrue(reader.Read());
      }

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("type", reader.Value);
    }

    [Test]
    public void ReadNullTerminatorStrings()
    {
      JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
      Assert.IsTrue(reader.Read());

      Assert.AreEqual("h\0i", reader.Value);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end while parsing unicode character. Line 1, position 4.")]
    public void UnexpectedEndOfHex()
    {
      JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));
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
    public void ReadNewLines()
    {
      string newLinesText = StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed + StringUtils.LineFeed + StringUtils.CarriageReturnLineFeed + " " + StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed;

      string json =
        newLinesText
        + "{" + newLinesText
        + "'" + newLinesText
        + "name1" + newLinesText
        + "'" + newLinesText
        + ":" + newLinesText
        + "[" + newLinesText
        + "new" + newLinesText
        + "Date" + newLinesText
        + "(" + newLinesText
        + "1" + newLinesText
        + "," + newLinesText
        + "null" + newLinesText
        + "/*" + newLinesText
        + "blah comment" + newLinesText
        + "*/" + newLinesText
        + ")" + newLinesText
        + "," + newLinesText
        + "1.1111" + newLinesText
        + "]" + newLinesText
        + "," + newLinesText
        + "name2" + newLinesText
        + ":" + newLinesText
        + "{" + newLinesText
        + "}" + newLinesText
        + "}" + newLinesText;

      int count = 0;
      StringReader sr = new StringReader(newLinesText);
      while (sr.ReadLine() != null)
      {
        count++;
      }

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(7, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(31, reader.LineNumber);
      Assert.AreEqual(newLinesText + "name1" + newLinesText, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(37, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(55, reader.LineNumber);
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
      Assert.AreEqual("Date", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(61, reader.LineNumber);
      Assert.AreEqual(1, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(73, reader.LineNumber);
      Assert.AreEqual(null, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(91, reader.LineNumber);
      Assert.AreEqual(newLinesText + "blah comment" + newLinesText, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(97, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(109, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(115, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(133, reader.LineNumber);
      Assert.AreEqual("name2", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(139, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(145, reader.LineNumber);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(151, reader.LineNumber);
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
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual(1, reader.Value);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
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
        Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
        Assert.AreEqual(0, jsonReader.Depth);
        
        jsonReader.Read();
        Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
        Assert.AreEqual("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
        Assert.AreEqual('\'', jsonReader.QuoteChar);
        Assert.AreEqual(1, jsonReader.Depth);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);
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
    public void ReadRandomJson()
    {
      string json = @"[
  true,
  {
    ""integer"": 99,
    ""string"": ""how now brown cow?"",
    ""array"": [
      0,
      1,
      2,
      3,
      4,
      {
        ""decimal"": 990.00990099
      },
      5
    ]
  },
  ""This is a string."",
  null,
  null
]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      while (reader.Read())
      {
        
      }
    }

    [Test]
    public void WriteReadWrite()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw)
        {
          Formatting = Formatting.Indented
        })
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

      using (JsonWriter jsonWriter = new JsonTextWriter(sw)
        {
          Formatting = Formatting.Indented
        })
      {
        serializer.Serialize(jsonWriter, jsonObject);
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
      reader.SetCharBuffer(new char[2]);

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
    ""B"" """;

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
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected character encountered while parsing value: }. Line 1, position 1.")]
    public void ReadBadCharInArray()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"[}"));

      reader.Read();
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading decimal: Line 0, position 0.")]
    public void ReadAsDecimalNoContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@""));

      reader.ReadAsDecimal();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading bytes: Line 0, position 0.")]
    public void ReadAsBytesNoContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@""));

      reader.ReadAsBytes();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading bytes: Line 1, position 1.")]
    public void ReadAsBytesNoContentWrappedObject()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"{"));

      reader.ReadAsBytes();
    }

#if !NET20
    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading date: Line 0, position 0.")]
    public void ReadAsDateTimeOffsetNoContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@""));

      reader.ReadAsDateTimeOffset();
    }
#endif

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected token when reading decimal: StartConstructor. Line 1, position 9.")]
    public void ReadAsDecimalBadContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

      reader.ReadAsDecimal();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected token when reading bytes: StartConstructor. Line 1, position 9.")]
    public void ReadAsBytesBadContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

      reader.ReadAsBytes();
    }

#if !NET20
    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected token when reading date: StartConstructor. Line 1, position 9.")]
    public void ReadAsDateTimeOffsetBadContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

      reader.ReadAsDateTimeOffset();
    }
#endif

    [Test]
    public void ReadAsBytesIntegerArrayWithComments()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

      byte[] data = reader.ReadAsBytes();
      Assert.AreEqual(2, data.Length);
      Assert.AreEqual(1, data[0]);
      Assert.AreEqual(2, data[1]);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end when reading bytes: Line 1, position 2.")]
    public void ReadAsBytesIntegerArrayWithNoEnd()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"[1"));

      reader.ReadAsBytes();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected token when reading bytes: Float. Line 1, position 4.")]
    public void ReadAsBytesArrayWithBadContent()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"[1.0]"));

      reader.ReadAsBytes();
    }

    [Test]
    public void ReadUnicode()
    {
      string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[5]);

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

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error parsing null value. Line 0, position 0.")]
    public void MatchWithInsufficentCharacters()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"nul"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error parsing null value. Line 0, position 0.")]
    public void MatchWithWrongCharacters()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"nulz"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error parsing null value. Line 1, position 4.")]
    public void MatchWithNoTrailingSeperator()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"nullz"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end while parsing comment.")]
    public void UnclosedComment()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"/* sdf"));
      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error parsing comment. Expected: *, got s. Line 1, position 1.")]
    public void BadCommentStart()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(@"/sdf"));
      reader.Read();
    }

    [Test]
    public void ReadAsDateTimeOffsetIsoDate()
    {
      string json = @"{""Offset"":""2011-08-01T21:25Z""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
      Assert.AreEqual(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadAsDateTimeOffsetUnitedStatesDate()
    {
      string json = @"{""Offset"":""1/30/2011""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.Culture = new CultureInfo("en-US");

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

      DateTimeOffset dt = (DateTimeOffset)reader.Value;
      Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadAsDateTimeOffsetNewZealandDate()
    {
      string json = @"{""Offset"":""30/1/2011""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.Culture = new CultureInfo("en-NZ");

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      reader.ReadAsDateTimeOffset();
      Assert.AreEqual(JsonToken.Date, reader.TokenType);
      Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

      DateTimeOffset dt = (DateTimeOffset)reader.Value;
      Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

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

    [Test]
    public void ReadAsDecimalFrench()
    {
      string json = @"{""decimal"":""9,99""}";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.Culture = new CultureInfo("fr-FR");

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      decimal? d = reader.ReadAsDecimal();
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(typeof(decimal), reader.ValueType);
      Assert.AreEqual(9.99m, d);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void ReadBufferOnControlChar()
    {
      string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[5]);
      for (int i = 0; i < 13; i++)
      {
        reader.Read();
      }

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(new DateTime(631136448000000000), reader.Value);
    }

    [Test]
    public void ReadBufferOnEndComment()
    {
      string json = @"/*comment*/ { /*comment*/
        ""Name"": /*comment*/ ""Apple"" /*comment*/, /*comment*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment*/ [ /*comment*/
          ""Small"", /*comment*/
          ""Medium"" /*comment*/,
          /*comment*/ ""Large""
        /*comment*/ ] /*comment*/
      } /*comment*/";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[5]);

      for (int i = 0; i < 26; i++)
      {
        Assert.IsTrue(reader.Read());
      }

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ParseNullStringConstructor()
    {
      string json = "new Date\0()";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      reader.SetCharBuffer(new char[7]);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual("Date", reader.Value);
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
    }

    [Test]
    public void ParseLineFeedDelimitedConstructor()
    {
      string json = "new Date\n()";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual("Date", reader.Value);
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
    }

    [Test]
    public void ParseArrayWithMissingValues()
    {
      string json = "[,,, \n\r\n \0   \r  , ,    ]";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }

    [Test]
    public void ParseBooleanWithNoExtraContent()
    {
      string json = "[true ";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.IsFalse(reader.Read());
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected end while parsing constructor.")]
    public void ParseConstructorWithUnexpectedEnd()
    {
      string json = "new Dat";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.Read();
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected character while parsing constructor: !. Line 1, position 9.")]
    public void ParseConstructorWithUnexpectedCharacter()
    {
      string json = "new Date !";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.Read();
    }

    [Test]
    public void ParseObjectWithNoEnd()
    {
      string json = "{hi:1, ";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.IsTrue(reader.Read());
      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ParseEmptyArray()
    {
      string json = "[]";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }

    [Test]
    public void ParseEmptyObject()
    {
      string json = "{}";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Error parsing boolean value. Line 1, position 4.")]
    public void ParseIncompleteCommentSeperator()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader("true/"));

      reader.Read();
    }

    [Test]
    public void ParseEmptyConstructor()
    {
      string json = "new Date()";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Unexpected character while parsing constructor: ,. Line 1, position 8.")]
    public void ParseConstructorWithBadCharacter()
    {
      string json = "new Date,()";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
    }

    [Test]
    public void ParseContentDelimitedByNonStandardWhitespace()
    {
      string json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
      
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
      
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }
    
    [Test]
    public void ReadContentDelimitedByComments()
    {
      string json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ""ExpiryDate"":/*comment*/new
" + StringUtils.LineFeed +
@"Date
(/*comment*/null/*comment*/),
        ""Price"": 3.99,
        ""Sizes"":/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("Name", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
      Assert.AreEqual(true, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("ExpiryDate", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
      Assert.AreEqual(5, reader.LineNumber);
      Assert.AreEqual("Date", reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Comment, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
    }

    [Test]
    public void ParseOctalNumber()
    {
      string json = @"010";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.ReadAsDecimal();
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(8m, reader.Value);
    }

    [Test]
    public void ParseHexNumber()
    {
      string json = @"0x20";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.ReadAsDecimal();
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(32m, reader.Value);
    }

    [Test]
    public void ParseNumbers()
    {
      string json = @"[0,1,2 , 3]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      reader.Read();
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      reader.Read();
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }
  }
}