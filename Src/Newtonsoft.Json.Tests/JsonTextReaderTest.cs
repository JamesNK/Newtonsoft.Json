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
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class JsonTextReaderTest : TestFixtureBase
    {
        [Test]
        public void ThrowErrorWhenParsingUnquoteStringThatStartsWithNE()
        {
            const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>("Unexpected content while parsing JSON. Path 'u', line 1, position 27.",
                () => { reader.Read(); });
        }

        [Test]
        public void FloatParseHandling()
        {
            string json = "[1.0,1,9.9,1E-06]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1.0m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(9.9m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(Convert.ToDecimal(1E-06), reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public void FloatParseHandling_NaN()
        {
            string json = "[NaN]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(
                "Cannot read NaN as a decimal.",
                () => reader.Read());
        }

        [Test]
        public void UnescapeDoubleQuotes()
        {
            string json = @"{""recipe_id"":""12"",""recipe_name"":""Apocalypse Leather Armors"",""recipe_text"":""#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \""Enhanced Leather Armor Boots\"" \""85644\"" )\r\n<img src=rdb:\/\/13264>\r\n\r\n#L \""Hacker Tool\"" \""87814\""\r\n<img src=rdb:\/\/99282>\r\n\r\n#L \""Clanalizer\"" \""208313\""\r\n<img src=rdb:\/\/156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \""Hacked Leather Armor Boots\"" \""245979\"" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \""Apocalypse Leather Armor Boots\"" \""245966\"" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \""Apocalypse Leather Armor Boots\"" \""245967\""\r\n#L \""Apocalypse Leather Armor Gloves\"" \""245969\""\r\n#L \""Apocalypse Leather Armor Helmet\"" \""245975\""\r\n#L \""Apocalypse Leather Armor Pants\"" \""245971\""\r\n#L \""Apocalypse Leather Armor Sleeves\"" \""245973\""\r\n#L \""Apocalypse Leather Body Armor\"" \""245965\""\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n"",""recipe_author"":null}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("recipe_text", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.AreEqual(@"#C16------------------------------
#C12Ingredients #C20
#C16------------------------------

a piece of Leather Armor
( ie #L ""Enhanced Leather Armor Boots"" ""85644"" )
<img src=rdb://13264>

#L ""Hacker Tool"" ""87814""
<img src=rdb://99282>

#L ""Clanalizer"" ""208313""
<img src=rdb://156479>

#C16------------------------------
#C12Recipe #C16
#C16------------------------------#C20

Hacker Tool
#C15+#C20
a piece of Leather Armor
#C15=#C20
<img src=rdb://13264>
a piece of Hacked Leather Armor
( ie : #L ""Hacked Leather Armor Boots"" ""245979"" )
#C16Skills: |  BE  |#C20

#C14------------------------------#C20

Clanalizer
#C15+#C20
a piece of Hacked Leather Armor
#C15=#C20
<img src=rdb://13264>
a piece of Apocalypse Leather Armor
( ie : #L ""Apocalypse Leather Armor Boots"" ""245966"" )
#C16Skills: |  ??  |#C20

#C16------------------------------
#C12Details#C16
#C16------------------------------#C20

#L ""Apocalypse Leather Armor Boots"" ""245967""
#L ""Apocalypse Leather Armor Gloves"" ""245969""
#L ""Apocalypse Leather Armor Helmet"" ""245975""
#L ""Apocalypse Leather Armor Pants"" ""245971""
#L ""Apocalypse Leather Armor Sleeves"" ""245973""
#L ""Apocalypse Leather Body Armor"" ""245965""

#C16------------------------------
#C12Comments#C16
#C16------------------------------#C20

Nice froob armor.. but ugleh!

", reader.Value);
        }

        [Test]
        public void SurrogatePairValid()
        {
            string json = @"{ ""MATHEMATICAL ITALIC CAPITAL ALPHA"": ""\uD835\uDEE2"" }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            string s = reader.Value.ToString();
            Assert.AreEqual(2, s.Length);

            StringInfo stringInfo = new StringInfo(s);
            Assert.AreEqual(1, stringInfo.LengthInTextElements);
        }

        [Test]
        public void SurrogatePairReplacement()
        {
            // existing good surrogate pair
            Assert.AreEqual("ABC \ud800\udc00 DEF", ReadString("ABC \\ud800\\udc00 DEF"));

            // invalid surrogates (two high back-to-back)
            Assert.AreEqual("ABC \ufffd\ufffd DEF", ReadString("ABC \\ud800\\ud800 DEF"));

            // invalid surrogates (two high back-to-back)
            Assert.AreEqual("ABC \ufffd\ufffd\u1234 DEF", ReadString("ABC \\ud800\\ud800\\u1234 DEF"));

            // invalid surrogates (three high back-to-back)
            Assert.AreEqual("ABC \ufffd\ufffd\ufffd DEF", ReadString("ABC \\ud800\\ud800\\ud800 DEF"));

            // invalid surrogates (high followed by a good surrogate pair)
            Assert.AreEqual("ABC \ufffd\ud800\udc00 DEF", ReadString("ABC \\ud800\\ud800\\udc00 DEF"));

            // invalid high surrogate at end of string
            Assert.AreEqual("ABC \ufffd", ReadString("ABC \\ud800"));

            // high surrogate not followed by low surrogate
            Assert.AreEqual("ABC \ufffd DEF", ReadString("ABC \\ud800 DEF"));

            // low surrogate not preceded by high surrogate
            Assert.AreEqual("ABC \ufffd\ufffd DEF", ReadString("ABC \\udc00\\ud800 DEF"));

            // make sure unencoded invalid surrogate characters don't make it through
            Assert.AreEqual("\ufffd\ufffd\ufffd", ReadString("\udc00\ud800\ud800"));

            Assert.AreEqual("ABC \ufffd\b", ReadString("ABC \\ud800\\b"));
            Assert.AreEqual("ABC \ufffd ", ReadString("ABC \\ud800 "));
            Assert.AreEqual("ABC \b\ufffd", ReadString("ABC \\b\\ud800"));
        }

        private string ReadString(string input)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(@"""" + input + @""""));

            JsonTextReader reader = new JsonTextReader(new StreamReader(ms));
            reader.Read();

            string s = (string)reader.Value;

            return s;
        }

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

            JsonReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
            Assert.AreEqual("Date", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(0L, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("hi", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("MemberConverter", reader.Value);
        }

        [Test]
        public void ParseAdditionalContent_Comma()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
],";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(
                "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 2.",
                () =>
                {
                    while (reader.Read())
                    {
                    }
                });
        }

        [Test]
        public void ParseAdditionalContent_Text()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
]content";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[2]);
#endif

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

            ExceptionAssert.Throws<JsonReaderException>(
                "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 2.",
                () => { reader.Read(); });
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
        public void ParseAdditionalContent_WhitespaceThenText()
        {
            string json = @"'hi' a";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(
                "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.",
                () =>
                {
                    while (reader.Read())
                    {
                    }
                });
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
#if DEBUG
                jsonReader.SetCharBuffer(new char[5]);
#endif

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
  array:[1,2,new Date(1)],
  subobject:{prop:1,proparray:[1]}
}";

            StringReader sr = new StringReader(input);

            using (JsonReader reader = new JsonTextReader(sr))
            {
                Assert.AreEqual(0, reader.Depth);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.StartObject);
                Assert.AreEqual(0, reader.Depth);
                Assert.AreEqual("", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("value", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.String);
                Assert.AreEqual(reader.Value, @"Purple");
                Assert.AreEqual(reader.QuoteChar, '\'');
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("value", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("array", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.StartArray);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("array", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.Integer);
                Assert.AreEqual(1L, reader.Value);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("array[0]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.Integer);
                Assert.AreEqual(2L, reader.Value);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("array[1]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.StartConstructor);
                Assert.AreEqual("Date", reader.Value);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("array[2]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.Integer);
                Assert.AreEqual(1L, reader.Value);
                Assert.AreEqual(3, reader.Depth);
                Assert.AreEqual("array[2][0]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.EndConstructor);
                Assert.AreEqual(null, reader.Value);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("array[2]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.EndArray);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("array", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("subobject", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.StartObject);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("subobject", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("subobject.prop", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.Integer);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("subobject.prop", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("subobject.proparray", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.StartArray);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("subobject.proparray", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.Integer);
                Assert.AreEqual(3, reader.Depth);
                Assert.AreEqual("subobject.proparray[0]", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.EndArray);
                Assert.AreEqual(2, reader.Depth);
                Assert.AreEqual("subobject.proparray", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.EndObject);
                Assert.AreEqual(1, reader.Depth);
                Assert.AreEqual("subobject", reader.Path);

                reader.Read();
                Assert.AreEqual(reader.TokenType, JsonToken.EndObject);
                Assert.AreEqual(0, reader.Depth);
                Assert.AreEqual("", reader.Path);
            }
        }

        [Test]
        public void NullTextReader()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                @"Value cannot be null.
Parameter name: reader",
                () => { new JsonTextReader(null); });
        }

        [Test]
        public void UnexpectedEndOfString()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'hi"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.",
                () => { reader.Read(); });
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
                Assert.AreEqual((long)i, reader.Value);
            }
            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void NullCharReading()
        {
            string json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

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
#if DEBUG
            reader.SetCharBuffer(new char[129]);
#endif

            for (int i = 0; i < 15; i++)
            {
                reader.Read();
            }

            reader.Read();
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
        }

        [Test]
        public void ReadInt32Overflow()
        {
            long i = int.MaxValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                long total = j + i;
                ExceptionAssert.Throws<JsonReaderException>(
                    "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.",
                    () =>
                    {
                        reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                        reader.ReadAsInt32();
                    });
            }
        }

        [Test]
        public void ReadInt32Overflow_Negative()
        {
            long i = int.MinValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.AreEqual(typeof(long), reader.ValueType);
            Assert.AreEqual(i, reader.Value);

            for (int j = 1; j < 1000; j++)
            {
                long total = -j + i;
                ExceptionAssert.Throws<JsonReaderException>(
                    "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.",
                    () =>
                    {
                        reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                        reader.ReadAsInt32();
                    });
            }
        }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
        [Test]
        public void ReadInt64Overflow()
        {
            BigInteger i = new BigInteger(long.MaxValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.Read();

                Assert.AreEqual(typeof(BigInteger), reader.ValueType);
            }
        }

        [Test]
        public void ReadInt64Overflow_Negative()
        {
            BigInteger i = new BigInteger(long.MinValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + -j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.Read();

                Assert.AreEqual(typeof(BigInteger), reader.ValueType);
            }
        }
#endif

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
#if DEBUG
            reader.SetCharBuffer(new char[129]);
#endif

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
        public void UnexpectedEndOfHex()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end while parsing unicode character. Path '', line 1, position 4.",
                () => { reader.Read(); });
        }

        [Test]
        public void UnexpectedEndOfControlCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.",
                () => { reader.Read(); });
        }

        [Test]
        public void ReadBytesWithBadCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error reading bytes. Unexpected token: Boolean. Path '', line 1, position 4.",
                () => { reader.ReadAsBytes(); });
        }

        [Test]
        public void ReadBytesWithUnexpectedEnd()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.",
                () => { reader.ReadAsBytes(); });
        }

        [Test]
        public void ReadBytesNoStartWithUnexpectedEnd()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
            Assert.IsTrue(reader.Read());

            Assert.IsNull(reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void UnexpectedEndWhenParsingUnquotedProperty()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.",
                () => { reader.Read(); });
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

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
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
            Assert.AreEqual(1L, reader.Value);

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
            Assert.AreEqual(1L, reader.Value);
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
            CollectionAssert.AreEquivalent(helloWorldData, data);
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
            CollectionAssert.AreEquivalent(helloWorldData, data);
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
        public void ParseIntegers()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1"));
            Assert.AreEqual(1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-1"));
            Assert.AreEqual(-1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("0"));
            Assert.AreEqual(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-0"));
            Assert.AreEqual(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
            Assert.AreEqual(int.MaxValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
            Assert.AreEqual(int.MinValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
            ExceptionAssert.Throws<JsonReaderException>("JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.", () => reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '9999999999999999999999999999999999999999999999999999999999999999999999999999a' is not a valid integer. Path '', line 1, position 77.", () => reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("1E-06"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '1E-06' is not a valid integer. Path '', line 1, position 5.", () => reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("1.1"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '1.1' is not a valid integer. Path '', line 1, position 3.", () => reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '-' is not a valid integer. Path '', line 1, position 1.", () => reader.ReadAsInt32());
        }

        [Test]
        public void ParseDecimals()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.AreEqual(1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.AreEqual(-1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.AreEqual(0.0m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.AreEqual(0, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '9999999999999999999999999999999999999999999999999999999999999999999999999999a' is not a valid decimal. Path '', line 1, position 77.", () => reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;
            ExceptionAssert.Throws<JsonReaderException>("Input string '9999999999999999999999999999999999999999999999999999999999999999999999999999a' is not a valid decimal. Path '', line 1, position 77.", () => reader.Read());

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.AreEqual(0.000001m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '-' is not a valid decimal. Path '', line 1, position 1.", () => reader.ReadAsDecimal());
        }

        [Test]
        public void ParseDoubles()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '9999999999999999999999999999999999999999999999999999999999999999999999999999a' is not a valid number. Path '', line 1, position 77.", () => reader.Read());

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.000001d, reader.Value);

            reader = new JsonTextReader(new StringReader(""));
            Assert.IsFalse(reader.Read());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>("Input string '-' is not a valid number. Path '', line 1, position 1.", () => reader.Read());
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

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void EscapedUnicodeText()
        {
            string json = @"[""\u003c"",""\u5f20""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[2]);
#endif

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
                @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN,0e-10,0.25e-5,0.3e10]";

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
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0000025d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(3000000000d, jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);
            }
        }

        [Test]
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

            ExceptionAssert.Throws<JsonReaderException>(
                @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 9.",
                () => { reader.Read(); });
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
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(jsonReader.Read());
        }

        [Test]
        public void ReadOctalNumberAsInt64()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual((long)250, (long)jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual((long)250, (long)jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual((long)250, (long)jsonReader.Value);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(jsonReader.Read());
        }

        [Test]
        public void ReadOctalNumberAsInt32()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

            jsonReader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            jsonReader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            jsonReader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            Assert.IsTrue(jsonReader.Read());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(jsonReader.Read());
        }

        [Test]
        public void ReadBadCharInArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[}"));

            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.",
                () => { reader.Read(); });
        }

        [Test]
        public void ReadAsDecimalNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(reader.ReadAsDecimal());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadAsBytesNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public void ReadAsBytesNoContentWrappedObject()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end when reading bytes. Path '', line 1, position 1.",
                () => { reader.ReadAsBytes(); });
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffsetNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(reader.ReadAsDateTimeOffset());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }
#endif

        [Test]
        public void ReadAsDecimalBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error reading decimal. Unexpected token: StartConstructor. Path '', line 1, position 9.",
                () => { reader.ReadAsDecimal(); });
        }

        [Test]
        public void ReadAsBytesBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error reading bytes. Unexpected token: StartConstructor. Path '', line 1, position 9.",
                () => { reader.ReadAsBytes(); });
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffsetBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error reading date. Unexpected token: StartConstructor. Path '', line 1, position 9.",
                () => { reader.ReadAsDateTimeOffset(); });
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
        public void ReadAsBytesIntegerArrayWithNoEnd()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end when reading bytes. Path '[0]', line 1, position 2.",
                () => { reader.ReadAsBytes(); });
        }

        [Test]
        public void ReadAsBytesArrayWithBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1.0]"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.",
                () => { reader.ReadAsBytes(); });
        }

        [Test]
        public void ReadUnicode()
        {
            string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

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
        public void ReadAsDateTimeOffsetBadString()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(
                "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.",
                () => { reader.ReadAsDateTimeOffset(); });
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
        public void ReadAsDecimalInt()
        {
            string json = @"{""Name"":1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(1m, reader.Value);
        }

        [Test]
        public void ReadAsIntDecimal()
        {
            string json = @"{""Name"": 1.1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(
                "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.",
                () => { reader.ReadAsInt32(); });
        }

        [Test]
        public void MatchWithInsufficentCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nul"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error parsing null value. Path '', line 0, position 0.",
                () => { reader.Read(); });
        }

        [Test]
        public void MatchWithWrongCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nulz"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error parsing null value. Path '', line 0, position 0.",
                () => { reader.Read(); });
        }

        [Test]
        public void MatchWithNoTrailingSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nullz"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error parsing null value. Path '', line 1, position 4.",
                () => { reader.Read(); });
        }

        [Test]
        public void UnclosedComment()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/* sdf"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end while parsing comment. Path '', line 1, position 6.",
                () => { reader.Read(); });
        }

        [Test]
        public void BadCommentStart()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/sdf"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.",
                () => { reader.Read(); });
        }

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
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

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
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

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
#if DEBUG
            reader.SetCharBuffer(new char[7]);
#endif

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
        public void SupportMultipleContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"1 2 ""name"" [][]null {}{} 1.1"));
            reader.SupportMultipleContent = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsFalse(reader.Read());
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
        public void ParseConstructorWithUnexpectedEnd()
        {
            string json = "new Dat";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end while parsing constructor. Path '', line 1, position 7.",
                () => { reader.Read(); });
        }

        [Test]
        public void ParseConstructorWithUnexpectedCharacter()
        {
            string json = "new Date !";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected character while parsing constructor: !. Path '', line 1, position 9.",
                () => { reader.Read(); });
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
        public void ParseIncompleteCommentSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("true/"));

            ExceptionAssert.Throws<JsonReaderException>(
                "Error parsing boolean value. Path '', line 1, position 4.",
                () => { reader.Read(); });
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
        public void ParseConstructorWithBadCharacter()
        {
            string json = "new Date,()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.",
                () => { Assert.IsTrue(reader.Read()); });
        }

        [Test]
        public void ParseContentDelimitedByNonStandardWhitespace()
        {
            string json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

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

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

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
        public void SingleLineComments()
        {
            string json = @"//comment*//*hi*/
{//comment
Name://comment
true//comment after true" + StringUtils.CarriageReturn + @"
,//comment after comma" + StringUtils.CarriageReturnLineFeed + @"
""ExpiryDate""://comment"  + StringUtils.LineFeed + @"
new
" + StringUtils.LineFeed +
                          @"Date
(//comment
null//comment
),
        ""Price"": 3.99,
        ""Sizes"": //comment
[//comment

          ""Small""//comment
]//comment
}//comment 
//comment 1 ";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment*//*hi*/", reader.Value);
            Assert.AreEqual(2, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.LineNumber);
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual(3, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Name", reader.Value);
            Assert.AreEqual(3, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual(4, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);
            Assert.AreEqual(4, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment after true", reader.Value);
            Assert.AreEqual(5, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment after comma", reader.Value);
            Assert.AreEqual(7, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("ExpiryDate", reader.Value);
            Assert.AreEqual(8, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual(9, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
            Assert.AreEqual(13, reader.LineNumber);
            Assert.AreEqual("Date", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(14, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual(15, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
            Assert.AreEqual(15, reader.LineNumber);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Price", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Sizes", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment ", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment 1 ", reader.Value);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void JustSinglelineComment()
        {
            string json = @"//comment";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("comment", reader.Value);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void ErrorReadingComment()
        {
            string json = @"/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            ExceptionAssert.Throws<JsonReaderException>(
                "Unexpected end while parsing comment. Path '', line 1, position 1.",
                () => { reader.Read(); });
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

        [Test]
        public void UnexpectedEndTokenWhenParsingOddEndToken()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.",
                () => { reader.Read(); });
        }

        [Test]
        public void ScientificNotation()
        {
            double d;

            d = Convert.ToDouble("6.0221418e23", CultureInfo.InvariantCulture);
            Console.WriteLine(d.ToString(new CultureInfo("fr-FR")));
            Console.WriteLine(d.ToString("0.#############################################################################"));

            //CultureInfo info = CultureInfo.GetCultureInfo("fr-FR");
            //Console.WriteLine(info.NumberFormat.NumberDecimalSeparator);

            string json = @"[0e-10,0E-10,0.25e-5,0.3e10,6.0221418e23]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();

            reader.Read();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            reader.Read();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            reader.Read();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0.0000025d, reader.Value);

            reader.Read();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(3000000000d, reader.Value);

            reader.Read();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(602214180000000000000000d, reader.Value);

            reader.Read();


            reader = new JsonTextReader(new StringReader(json));

            reader.Read();

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0m, reader.Value);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0m, reader.Value);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0.0000025m, reader.Value);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(3000000000m, reader.Value);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(602214180000000000000000m, reader.Value);

            reader.Read();
        }

        [Test]
        public void MaxDepth()
        {
            string json = "[[]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.",
                () => { Assert.IsTrue(reader.Read()); });
        }

        [Test]
        public void MaxDepthDoesNotRecursivelyError()
        {
            string json = "[[[[]]],[[]]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(0, reader.Depth);

            ExceptionAssert.Throws<JsonReaderException>(
                "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.",
                () => { Assert.IsTrue(reader.Read()); });
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(3, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(3, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1, reader.Depth);

            ExceptionAssert.Throws<JsonReaderException>(
                "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.",
                () => { Assert.IsTrue(reader.Read()); });
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(0, reader.Depth);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void ReadingFromSlowStream()
        {
            string json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(false, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(false, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("test!", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(1.11d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0.0000025d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(3000000000d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(602214180000000000000000d, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(reader.Value, "Purple\r \n monkey's:\tdishwasher");

            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void DateParseHandling()
        {
            string json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTime;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());

#if !NET20
            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTimeOffset;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());
#endif

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.None;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(@"1970-01-01T00:00:00Z", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(@"/Date(0)/", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(reader.Read());

#if !NET20
            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTime;

            Assert.IsTrue(reader.Read());
            reader.ReadAsDateTimeOffset();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            reader.ReadAsDateTimeOffset();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());


            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTimeOffset;

            Assert.IsTrue(reader.Read());
            reader.ReadAsDateTime();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            reader.ReadAsDateTime();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());
#endif
        }

        [Test]
        public void ResetJsonTextReaderErrorCount()
        {
            ToggleReaderError toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
            JsonTextReader jsonTextReader = new JsonTextReader(toggleReaderError);

            Assert.IsTrue(jsonTextReader.Read());

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());
            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());

            toggleReaderError.Error = false;

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual("first", jsonTextReader.Value);

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());

            toggleReaderError.Error = false;

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(1L, jsonTextReader.Value);

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());
            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());
            ExceptionAssert.Throws<Exception>(
                "Read error",
                () => jsonTextReader.Read());

            toggleReaderError.Error = false;

            //a reader use to skip to the end after 3 errors in a row
            //Assert.IsFalse(jsonTextReader.Read());
        }

        [Test]
        public void WriteReadBoundaryDecimals()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartArray();
            writer.WriteValue(decimal.MaxValue);
            writer.WriteValue(decimal.MinValue);
            writer.WriteEndArray();

            string json = sw.ToString();

            StringReader sr = new StringReader(json);
            JsonTextReader reader = new JsonTextReader(sr);

            Assert.IsTrue(reader.Read());

            decimal? max = reader.ReadAsDecimal();
            Assert.AreEqual(decimal.MaxValue, max);

            decimal? min = reader.ReadAsDecimal();
            Assert.AreEqual(decimal.MinValue, min);

            Assert.IsTrue(reader.Read());
        }
    }

    public class ToggleReaderError : TextReader
    {
        private readonly TextReader _inner;

        public bool Error { get; set; }

        public ToggleReaderError(TextReader inner)
        {
            _inner = inner;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (Error)
                throw new Exception("Read error");

            return _inner.Read(buffer, index, 1);
        }
    }

    public class SlowStream : Stream
    {
        private byte[] bytes;
        private int totalBytesRead;
        private int bytesPerRead;

        public SlowStream(byte[] content, int bytesPerRead)
        {
            bytes = content;
            totalBytesRead = 0;
            this.bytesPerRead = bytesPerRead;
        }

        public SlowStream(string content, Encoding encoding, int bytesPerRead)
            : this(encoding.GetBytes(content), bytesPerRead)
        {
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toReturn = Math.Min(count, bytesPerRead);
            toReturn = Math.Min(toReturn, bytes.Length - totalBytesRead);
            if (toReturn > 0)
            {
                Array.Copy(bytes, totalBytesRead, buffer, offset, toReturn);
            }

            totalBytesRead += toReturn;
            return toReturn;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}