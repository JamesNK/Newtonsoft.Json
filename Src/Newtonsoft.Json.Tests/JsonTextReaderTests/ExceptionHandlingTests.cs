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
    public class ExceptionHandlingTests : TestFixtureBase
    {
        [Test]
        public void ReadAsBytes_MissingComma()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world");

            string json = @"['" + Convert.ToBase64String(data) + "' '" + Convert.ToBase64String(data) + @"']";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            CollectionAssert.AreEquivalent(data, reader.ReadAsBytes());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsBytes(),
                "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 20.");
        }

        [Test]
        public void ReadAsInt32_MissingComma()
        {
            string json = "[0 1 2]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(0, (int)reader.ReadAsInt32());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsInt32(),
                "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
        }

        [Test]
        public void ReadAsBoolean_MissingComma()
        {
            string json = "[true false true]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(true, (bool)reader.ReadAsBoolean());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsBoolean(),
                "After parsing a value an unexpected character was encountered: f. Path '[0]', line 1, position 6.");
        }

        [Test]
        public void ReadAsDateTime_MissingComma()
        {
            string json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTime(2017, 2, 4, 0, 0, 0, DateTimeKind.Utc), (DateTime)reader.ReadAsDateTime());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsDateTime(),
                "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffset_MissingComma()
        {
            string json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTimeOffset(2017, 2, 4, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)reader.ReadAsDateTimeOffset());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsDateTimeOffset(),
                "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
        }
#endif

        [Test]
        public void ReadAsString_MissingComma()
        {
            string json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("2017-02-04T00:00:00Z", reader.ReadAsString());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.ReadAsString(),
                "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
        }

        [Test]
        public void Read_MissingComma()
        {
            string json = "[0 1 2]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(
                () => reader.Read(),
                "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
        }

        [Test]
        public void UnexpectedEndAfterReadingN()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("n"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 1.");
        }

        [Test]
        public void UnexpectedEndAfterReadingNu()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("nu"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
        }

        [Test]
        public void UnexpectedEndAfterReadingNe()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("ne"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
        }

        [Test]
        public void UnexpectedEndOfHex()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing Unicode escape sequence. Path '', line 1, position 4.");
        }

        [Test]
        public void UnexpectedEndOfControlCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Test]
        public void ReadInvalidNonBase10Number()
        {
            string json = "0aq2dun13.hod";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
        }

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

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected content while parsing JSON. Path 'u', line 1, position 29.");
        }

        [Test]
        public void UnexpectedEndOfString()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'hi"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Test]
        public void UnexpectedEndTokenWhenParsingOddEndToken()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
        }

        [Test]
        public void ResetJsonTextReaderErrorCount()
        {
            ToggleReaderError toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
            JsonTextReader jsonTextReader = new JsonTextReader(toggleReaderError);

            Assert.IsTrue(jsonTextReader.Read());

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual("first", jsonTextReader.Value);

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(1L, jsonTextReader.Value);

            toggleReaderError.Error = true;

            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;
        }

        [Test]
        public void MatchWithInsufficentCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nul"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public void MatchWithWrongCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nulz"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 1, position 3.");
        }

        [Test]
        public void MatchWithNoTrailingSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nullz"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 1, position 4.");
        }

        [Test]
        public void UnclosedComment()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/* sdf"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 6.");
        }

        [Test]
        public void BadCommentStart()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/sdf"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
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

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 8.");
        }

        [Test]
        public void NullTextReader()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                () => { new JsonTextReader(null); },
                new string[]
                {
                    "Value cannot be null." + Environment.NewLine + "Parameter name: reader",
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: reader" // Mono
                });
        }

        [Test]
        public void ParseConstructorWithBadCharacter()
        {
            string json = "new Date,()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { Assert.IsTrue(reader.Read()); }, "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.");
        }

        [Test]
        public void ParseConstructorWithUnexpectedEnd()
        {
            string json = "new Dat";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing constructor. Path '', line 1, position 7.");
        }

        [Test]
        public void ParseConstructorWithUnexpectedCharacter()
        {
            string json = "new Date !";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character while parsing constructor: !. Path '', line 1, position 9.");
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

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                while (reader.Read())
                {
                }
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
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

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 1.");
        }

        [Test]
        public void ParseAdditionalContent_WhitespaceThenText()
        {
            string json = @"'hi' a";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                while (reader.Read())
                {
                }
            }, "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
        }

        [Test]
        public void ParseIncompleteCommentSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("true/"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing boolean value. Path '', line 1, position 4.");
        }

        [Test]
        public void ReadBadCharInArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[}"));

            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsBytesNoContentWrappedObject()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading JSON. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadBytesEmptyWrappedObject()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{}"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Error reading bytes. Unexpected token: StartObject. Path '', line 1, position 2." );
        }

        [Test]
        public void ReadIntegerWithError()
        {
            string json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 52.");

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(jsonTextReader.Read());
        }

        [Test]
        public void ReadIntegerWithErrorInArray()
        {
            string json = @"[
  333333333333333333333333333333333333333,
  3.3,
  ,
  0f
]";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 41.");

            ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 5.");

            ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Unexpected character encountered while parsing value: ,. Path '[2]', line 4, position 3.");

            ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 4.");

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

            Assert.IsFalse(jsonTextReader.Read());
        }

        [Test]
        public void ReadBytesWithError()
        {
            string json = @"{
    ChildId: '123'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            try
            {
                jsonTextReader.ReadAsBytes();
            }
            catch (FormatException)
            {
            }

            Assert.IsTrue(jsonTextReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(jsonTextReader.Read());
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
                ExceptionAssert.Throws<JsonReaderException>(() =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    reader.ReadAsInt32();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.");
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
                ExceptionAssert.Throws<JsonReaderException>(() =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    reader.ReadAsInt32();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.");
            }
        }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3
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
#endif

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3
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
        public void ReadAsString_Null_AdditionalBadData()
        {
            string json = @"nullllll";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Error parsing null value. Path '', line 1, position 4.");
        }

        [Test]
        public void ReadAsBoolean_AdditionalBadData()
        {
            string json = @"falseeeee";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
        }

        [Test]
        public void ReadAsString_AdditionalBadData()
        {
            string json = @"falseeeee";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
        }

        [Test]
        public void ReadAsBoolean_UnexpectedEnd()
        {
            string json = @"tru";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public void ReadAsBoolean_BadData()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsString_BadData()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsDouble_BadData()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDouble(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsDouble_Boolean()
        {
            string json = @"true";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDouble(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsBytes_BadData()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsBytesIntegerArrayWithNoEnd()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
        }

        [Test]
        public void ReadAsBytesArrayWithBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1.0]"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
        }

        [Test]
        public void ReadAsBytesBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }

        [Test]
        public void ReadAsBytes_CommaErrors()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,'']"));
            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsBytes();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            CollectionAssert.AreEquivalent(new byte[0], reader.ReadAsBytes());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadAsBytes_InvalidEndArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsBytes();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsBytes_CommaErrors_Multiple()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['',,'']"));
            reader.Read();
            CollectionAssert.AreEquivalent(new byte[0], reader.ReadAsBytes());

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsBytes();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

            CollectionAssert.AreEquivalent(new byte[0], reader.ReadAsBytes());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadBytesWithBadCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadBytesWithUnexpectedEnd()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
        }

        [Test]
        public void ReadAsDateTime_BadData()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTime(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadAsDateTime_Boolean()
        {
            string json = @"true";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTime(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffsetBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }
#endif

        [Test]
        public void ReadAsDecimalBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }

        [Test]
        public void ReadAsDecimalBadContent_SecondLine()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"
new Date()"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing value: e. Path '', line 2, position 2.");
        }

        [Test]
        public void ReadInt32WithBadCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadNumberValue_CommaErrors()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,1]"));
            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsInt32();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            Assert.AreEqual(1, reader.ReadAsInt32());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadNumberValue_InvalidEndArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsInt32();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadNumberValue_CommaErrors_Multiple()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[1,,1]"));
            reader.Read();
            reader.ReadAsInt32();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsInt32();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 4.");

            Assert.AreEqual(1, reader.ReadAsInt32());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadAsString_UnexpectedEnd()
        {
            string json = @"tru";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public void ReadAsString_Null_UnexpectedEnd()
        {
            string json = @"nul";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public void ReadStringValue_InvalidEndArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsDateTime();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadStringValue_CommaErrors()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,'']"));
            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsString();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            Assert.AreEqual(string.Empty, reader.ReadAsString());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadStringValue_CommaErrors_Multiple()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['',,'']"));
            reader.Read();
            reader.ReadAsInt32();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsString();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

            Assert.AreEqual(string.Empty, reader.ReadAsString());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ReadStringValue_Numbers_NotString()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[56,56]"));
            reader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsDateTime();
            }, "Unexpected character encountered while parsing value: 5. Path '', line 1, position 2.");

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsDateTime();
            }, "Unexpected character encountered while parsing value: 6. Path '', line 1, position 3.");

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader.ReadAsDateTime();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 4.");

            Assert.AreEqual(56, reader.ReadAsInt32());
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void ErrorReadingComment()
        {
            string json = @"/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 1.");
        }

        [Test]
        public void EscapedPathInExceptionMessage()
        {
            string json = @"{
  ""frameworks"": {
    ""dnxcore50"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": !!! !!!
        }
      }
    }
  }
}";

            ExceptionAssert.Throws<JsonReaderException>(
                () =>
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader(json));
                    while (reader.Read())
                    {
                    }
                },
                "Unexpected character encountered while parsing value: !. Path 'frameworks.dnxcore50.dependencies['System.Xml.ReaderWriter'].source', line 6, position 20.");
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

            ExceptionAssert.Throws<JsonReaderException>(() => { Assert.IsTrue(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
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

            ExceptionAssert.Throws<JsonReaderException>(() => { Assert.IsTrue(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
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

            ExceptionAssert.Throws<JsonReaderException>(() => { Assert.IsTrue(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
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
        public void UnexpectedEndWhenParsingUnquotedProperty()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
            Assert.IsTrue(reader.Read());

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
        }
    }
}
