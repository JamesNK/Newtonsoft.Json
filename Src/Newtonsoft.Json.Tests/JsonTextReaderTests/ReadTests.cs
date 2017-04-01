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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3
using System.Numerics;
#endif
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Xml;
using Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class ReadTests : TestFixtureBase
    {
#if !(PORTABLE || PORTABLE40 || NET35 || NET20)
        [Test]
        public void ReadAsBoolean()
        {
            string json = @"[
  1,
  0,
  1.1,
  0.0,
  0.000000000001,
  9999999999,
  -9999999999,
  9999999999999999999999999999999999999999999999999999999999999999999999,
  -9999999999999999999999999999999999999999999999999999999999999999999999,
  'true',
  'TRUE',
  'false',
  'FALSE',
  // comment!
  /* comment! */
  '',
  null
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[10]);
#endif

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[0]", reader.Path);

            Assert.AreEqual(false, reader.ReadAsBoolean());
            Assert.AreEqual("[1]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[2]", reader.Path);

            Assert.AreEqual(false, reader.ReadAsBoolean());
            Assert.AreEqual("[3]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[4]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[5]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[6]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[7]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[8]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[9]", reader.Path);

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual("[10]", reader.Path);

            Assert.AreEqual(false, reader.ReadAsBoolean());
            Assert.AreEqual("[11]", reader.Path);

            Assert.AreEqual(false, reader.ReadAsBoolean());
            Assert.AreEqual("[12]", reader.Path);

            Assert.AreEqual(null, reader.ReadAsBoolean());
            Assert.AreEqual("[13]", reader.Path);

            Assert.AreEqual(null, reader.ReadAsBoolean());
            Assert.AreEqual("[14]", reader.Path);

            Assert.AreEqual(null, reader.ReadAsBoolean());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("", reader.Path);

            Assert.AreEqual(null, reader.ReadAsBoolean());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
            Assert.AreEqual("", reader.Path);
        }
#endif

        [Test]
        public void ReadAsBoolean_NullChar()
        {
            string json = '\0' + @"true" + '\0' + '\0';

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.AreEqual(true, reader.ReadAsBoolean());
            Assert.AreEqual(null, reader.ReadAsBoolean());
        }

        [Test]
        public void ReadAsBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world");

            string json = @"""" + Convert.ToBase64String(data) + @"""";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            byte[] result = reader.ReadAsBytes();

            CollectionAssert.AreEquivalent(data, result);
        }

        [Test]
        public void ReadAsBooleanNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(reader.ReadAsBoolean());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

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

#if !NET20
        [Test]
        public void ReadAsDateTimeOffsetNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(reader.ReadAsDateTimeOffset());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }
#endif

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
#endif

#if !NET20
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
#endif

#if !NET20
        [Test]
        public void ReadAsDateTimeOffsetBadString()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }
#endif

#if !NET20
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
#endif

#if !NET20
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

#if !NET20
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
#endif

#if !NET20
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
#endif

#if !NET20
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

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
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
        public void ReadAsDouble_Null()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("null"));
            Assert.AreEqual(null, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_Success()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("'12.34'"));
            Assert.AreEqual(12.34d, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_Hex()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("0XCAFEBABE"));
            Assert.AreEqual(3405691582d, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_AllowThousands()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("'1,112.34'"));
            Assert.AreEqual(1112.34d, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_Failure()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['Trump',1]"));

            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsDouble(); },
                "Could not convert string to double: Trump. Path '[0]', line 1, position 8.");

            Assert.AreEqual(1d, reader.ReadAsDouble());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadAsString_Boolean()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false}"));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            string s = reader.ReadAsString();
            Assert.AreEqual("false", s);

            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void Read_Boolean_Failure()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.Read(); },
                "Error parsing boolean value. Path 'Test1', line 1, position 14.");

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void ReadAsString_Boolean_Failure()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsString(); },
                "Unexpected character encountered while parsing value: 1. Path 'Test1', line 1, position 14.");

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

#if !NET20
        [Test]
        public void ReadValue_EmptyString_Position()
        {
            string json = @"['','','','','','','']";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();
            reader.ReadAsInt32();
            Assert.AreEqual("[0]", reader.Path);
            reader.ReadAsDecimal();
            Assert.AreEqual("[1]", reader.Path);
            reader.ReadAsDateTime();
            Assert.AreEqual("[2]", reader.Path);
            reader.ReadAsDateTimeOffset();
            Assert.AreEqual("[3]", reader.Path);
            reader.ReadAsString();
            Assert.AreEqual("[4]", reader.Path);
            reader.ReadAsBytes();
            Assert.AreEqual("[5]", reader.Path);
            reader.ReadAsDouble();
            Assert.AreEqual("[6]", reader.Path);

            Assert.IsNull(reader.ReadAsString());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNull(reader.ReadAsString());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            Assert.IsNull(reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }
#endif

        [Test]
        public void ReadValueComments()
        {
            string json = @"/*comment*/[/*comment*/1/*comment*/,2,/*comment*//*comment*/""three""/*comment*/,""four""/*comment*/,null,/*comment*/null,3.99,1.1/*comment*/,''/*comment*/]/*comment*/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.AreEqual(1, reader.ReadAsInt32());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.AreEqual(2, reader.ReadAsInt32());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.AreEqual("three", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.AreEqual("four", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.AreEqual(null, reader.ReadAsString());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.AreEqual(null, reader.ReadAsInt32());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.AreEqual(3.99m, reader.ReadAsDecimal());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.AreEqual(1.1m, reader.ReadAsDecimal());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            CollectionAssert.AreEquivalent(new byte[0], reader.ReadAsBytes());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.AreEqual(null, reader.ReadAsInt32());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
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
        public void ReadNullIntLineNumberAndPosition()
        {
            string json = @"[
  1,
  2,
  3,
  null
]";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            reader.Read();
            Assert.AreEqual(1, reader.LineNumber);

            reader.ReadAsInt32();
            Assert.AreEqual(2, reader.LineNumber);
            Assert.AreEqual("[0]", reader.Path);

            reader.ReadAsInt32();
            Assert.AreEqual(3, reader.LineNumber);
            Assert.AreEqual("[1]", reader.Path);

            reader.ReadAsInt32();
            Assert.AreEqual(4, reader.LineNumber);
            Assert.AreEqual("[2]", reader.Path);

            reader.ReadAsInt32();
            Assert.AreEqual(5, reader.LineNumber);
            Assert.AreEqual("[3]", reader.Path);

            reader.Read();
            Assert.AreEqual(6, reader.LineNumber);
            Assert.AreEqual(string.Empty, reader.Path);

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

#if !DNXCORE50
        [Test]
        [Ignore("Probably not a Json.NET issue")]
        public void ReadFromNetworkStream()
        {
            const int port = 11999;
            const int jsonArrayElementsCount = 193;

            var serverStartedEvent = new ManualResetEvent(false);
            var clientReceivedEvent = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(work =>
            {
                var server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
                server.Start();

                serverStartedEvent.Set();

                var serverSocket = server.AcceptSocket();

                var jsonString = "[\r\n" + String.Join(",", Enumerable.Repeat("  \"testdata\"\r\n", jsonArrayElementsCount).ToArray()) + "]";
                var bytes = new UTF8Encoding().GetBytes(jsonString);
                serverSocket.Send(bytes);
                Console.WriteLine("server send: " + bytes.Length);


                clientReceivedEvent.WaitOne();

            });

            serverStartedEvent.WaitOne();


            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = false;
            socket.Connect("127.0.0.1", port);

            var stream = new NetworkStream(socket);
            var serializer = new JsonSerializer();

            int i = 0;
            using (var sr = new StreamReader(stream, new UTF8Encoding(), false))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                while (jsonTextReader.Read())
                {
                    i++;

                    if (i == 193)
                    {
                        string s = string.Empty;
                    }

                    Console.WriteLine($"{i} - {jsonTextReader.TokenType} - {jsonTextReader.Value}");
                }
                //var result = serializer.Deserialize(jsonTextReader).ToString();
                //Console.WriteLine("client receive: " + new UTF8Encoding().GetBytes(result).Length);
            }

            clientReceivedEvent.Set();

            Console.WriteLine("Done");
        }
#endif

        [Test]
        public void ReadCommentInsideArray()
        {
            string json = @"{
    ""projects"": [
        ""src"",
        //""
        ""test""
    ]
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual("src", jsonTextReader.Value);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.Comment, jsonTextReader.TokenType);
            Assert.AreEqual(@"""", jsonTextReader.Value);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual("test", jsonTextReader.Value);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
        }

        [Test]
        public void ReadAsBytes_Base64AndGuid()
        {
            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'"));
            byte[] data = jsonTextReader.ReadAsBytes();
            byte[] expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

            CollectionAssert.AreEqual(expected, data);

            jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'"));
            data = jsonTextReader.ReadAsBytes();
            expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

            CollectionAssert.AreEqual(expected, data);
        }

        [Test]
        public void ReadSingleQuoteInsideDoubleQuoteString()
        {
            string json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
            jsonTextReader.Read();
            jsonTextReader.Read();
            jsonTextReader.Read();

            Assert.AreEqual(@"Forest's Bakery And Cafe", jsonTextReader.Value);
        }

        [Test]
        public void ReadMultilineString()
        {
            string json = @"""first line
second line
third line""";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);

            Assert.AreEqual(@"first line
second line
third line", jsonTextReader.Value);
        }



#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3
        [Test]
        public void ReadBigInteger()
        {
            string json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333,
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);
            Assert.AreEqual(typeof(BigInteger), jsonTextReader.ValueType);
            Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(jsonTextReader.Read());

            JObject o = JObject.Parse(json);
            var i = (BigInteger)((JValue)o["ChildId"]).Value;
            Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), i);
        }
#endif


        [Test]
        public void ReadBadMSDateAsString()
        {
            string json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(jsonTextReader.Read());
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
                Assert.AreEqual(6, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
                Assert.AreEqual("Intel", jsonReader.Value);
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
        public void ReadNullTerminatorStrings()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
            Assert.IsTrue(reader.Read());

            Assert.AreEqual("h\0i", reader.Value);
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
    }
}
