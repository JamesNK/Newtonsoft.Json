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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Globalization;
#if !PORTABLE || NETSTANDARD1_1
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
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests;

namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class ExceptionHandlingAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task UnexpectedEndAfterReadingNAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("n"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 1.");
        }

        [Test]
        public async Task UnexpectedEndAfterReadingNuAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("nu"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
        }

        [Test]
        public async Task UnexpectedEndAfterReadingNeAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("ne"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
        }

        [Test]
        public async Task UnexpectedEndOfHexAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing Unicode escape sequence. Path '', line 1, position 4.");
        }

        [Test]
        public async Task UnexpectedEndOfControlCharacterAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Test]
        public async Task ReadInvalidNonBase10NumberAsync()
        {
            string json = "0aq2dun13.hod";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
        }

        [Test]
        public async Task ThrowErrorWhenParsingUnquoteStringThatStartsWithNEAsync()
        {
            const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected content while parsing JSON. Path 'u', line 1, position 29.");
        }

        [Test]
        public async Task UnexpectedEndOfStringAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'hi"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Test]
        public async Task UnexpectedEndTokenWhenParsingOddEndTokenAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
        }

        [Test]
        public async Task ResetJsonTextReaderErrorCountAsync()
        {
            ToggleReaderError toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
            JsonTextReader jsonTextReader = new JsonTextReader(toggleReaderError);

            Assert.IsTrue(await jsonTextReader.ReadAsync());

            toggleReaderError.Error = true;

            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

            toggleReaderError.Error = false;

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual("first", jsonTextReader.Value);

            toggleReaderError.Error = true;

            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

            toggleReaderError.Error = false;

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(1L, jsonTextReader.Value);

            toggleReaderError.Error = true;

            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
            await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

            toggleReaderError.Error = false;
        }

        [Test]
        public async Task MatchWithInsufficentCharactersAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nul"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public async Task MatchWithWrongCharactersAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nulz"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing null value. Path '', line 1, position 3.");
        }

        [Test]
        public async Task MatchWithNoTrailingSeparatorAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nullz"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing null value. Path '', line 1, position 4.");
        }

        [Test]
        public async Task UnclosedCommentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/* sdf"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing comment. Path '', line 1, position 6.");
        }

        [Test]
        public async Task BadCommentStartAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/sdf"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
        }

        [Test]
        public async Task MissingColonAsync()
        {
            string json = @"{
    ""A"" : true,
    ""B"" """;

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 8.");
        }

        [Test]
        public async Task ParseConstructorWithBadCharacterAsync()
        {
            string json = "new Date,()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.");
        }

        [Test]
        public async Task ParseConstructorWithUnexpectedEndAsync()
        {
            string json = "new Dat";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing constructor. Path '', line 1, position 7.");
        }

        [Test]
        public async Task ParseConstructorWithUnexpectedCharacterAsync()
        {
            string json = "new Date !";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character while parsing constructor: !. Path '', line 1, position 9.");
        }

        [Test]
        public async Task ParseAdditionalContent_CommaAsync()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
],";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                while (await reader.ReadAsync())
                {
                }
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
        }

        [Test]
        public async Task ParseAdditionalContent_TextAsync()
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

            await reader.ReadAsync();
            Assert.AreEqual(1, reader.LineNumber);

            await reader.ReadAsync();
            Assert.AreEqual(2, reader.LineNumber);

            await reader.ReadAsync();
            Assert.AreEqual(3, reader.LineNumber);

            await reader.ReadAsync();
            Assert.AreEqual(4, reader.LineNumber);

            await reader.ReadAsync();
            Assert.AreEqual(5, reader.LineNumber);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 1.");
        }

        [Test]
        public async Task ParseAdditionalContent_WhitespaceThenTextAsync()
        {
            string json = @"'hi' a";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                while (await reader.ReadAsync())
                {
                }
            }, "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
        }

        [Test]
        public async Task ParseIncompleteCommentSeparatorAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("true/"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing boolean value. Path '', line 1, position 4.");
        }

        [Test]
        public async Task ReadBadCharInArrayAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[}"));

            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsBytesNoContentWrappedObjectAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadBytesEmptyWrappedObjectAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{}"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Error reading bytes. Unexpected token: StartObject. Path '', line 1, position 2." );
        }

        [Test]
        public async Task ReadIntegerWithErrorAsync()
        {
            string json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 52.");

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(await jsonTextReader.ReadAsync());
        }

        [Test]
        public async Task ReadIntegerWithErrorInArrayAsync()
        {
            string json = @"[
  333333333333333333333333333333333333333,
  3.3,
  ,
  0f
]";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 41.");

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 5.");

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Unexpected character encountered while parsing value: ,. Path '[2]', line 4, position 3.");

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 4.");

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

            Assert.IsFalse(await jsonTextReader.ReadAsync());
        }

        [Test]
        public async Task ReadBytesWithErrorAsync()
        {
            string json = @"{
    ChildId: '123'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            try
            {
                await jsonTextReader.ReadAsBytesAsync();
            }
            catch (FormatException)
            {
            }

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(await jsonTextReader.ReadAsync());
        }

        [Test]
        public async Task ReadInt32OverflowAsync()
        {
            long i = int.MaxValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                long total = j + i;
                await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    await reader.ReadAsInt32Async();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.");
            }
        }

        [Test]
        public async Task ReadInt32Overflow_NegativeAsync()
        {
            long i = int.MinValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();
            Assert.AreEqual(typeof(long), reader.ValueType);
            Assert.AreEqual(i, reader.Value);

            for (int j = 1; j < 1000; j++)
            {
                long total = -j + i;
                await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    await reader.ReadAsInt32Async();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.");
            }
        }

#if !PORTABLE || NETSTANDARD1_1
        [Test]
        public async Task ReadInt64OverflowAsync()
        {
            BigInteger i = new BigInteger(long.MaxValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                await reader.ReadAsync();

                Assert.AreEqual(typeof(BigInteger), reader.ValueType);
            }
        }

        [Test]
        public async Task ReadInt64Overflow_NegativeAsync()
        {
            BigInteger i = new BigInteger(long.MinValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();
            Assert.AreEqual(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + -j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                await reader.ReadAsync();

                Assert.AreEqual(typeof(BigInteger), reader.ValueType);
            }
        }
#endif

        [Test]
        public async Task ReadAsString_Null_AdditionalBadDataAsync()
        {
            string json = @"nullllll";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Error parsing null value. Path '', line 1, position 4.");
        }

        [Test]
        public async Task ReadAsBoolean_AdditionalBadDataAsync()
        {
            string json = @"falseeeee";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
        }

        [Test]
        public async Task ReadAsString_AdditionalBadDataAsync()
        {
            string json = @"falseeeee";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
        }

        [Test]
        public async Task ReadAsBoolean_UnexpectedEndAsync()
        {
            string json = @"tru";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public async Task ReadAsBoolean_BadDataAsync()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsString_BadDataAsync()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsDouble_BadDataAsync()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDoubleAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsDouble_BooleanAsync()
        {
            string json = @"true";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDoubleAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsBytes_BadDataAsync()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsBytesIntegerArrayWithNoEndAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
        }

        [Test]
        public async Task ReadAsBytesArrayWithBadContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1.0]"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
        }

        [Test]
        public async Task ReadAsBytesBadContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }

        [Test]
        public async Task ReadAsBytes_CommaErrorsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,'']"));
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsBytesAsync();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            CollectionAssert.AreEquivalent(new byte[0], await reader.ReadAsBytesAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsBytes_InvalidEndArrayAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsBytesAsync();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsBytes_CommaErrors_MultipleAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['',,'']"));
            await reader.ReadAsync();
            CollectionAssert.AreEquivalent(new byte[0], await reader.ReadAsBytesAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsBytesAsync();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

            CollectionAssert.AreEquivalent(new byte[0], await reader.ReadAsBytesAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadBytesWithBadCharacterAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadBytesWithUnexpectedEndAsync()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
        }

        [Test]
        public async Task ReadAsDateTime_BadDataAsync()
        {
            string json = @"pie";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadAsDateTime_BooleanAsync()
        {
            string json = @"true";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

#if !NET20
        [Test]
        public async Task ReadAsDateTimeOffsetBadContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }
#endif

        [Test]
        public async Task ReadAsDecimalBadContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
        }

        [Test]
        public async Task ReadAsDecimalBadContent_SecondLineAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"
new Date()"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 2, position 2.");
        }

        [Test]
        public async Task ReadInt32WithBadCharacterAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadNumberValue_CommaErrorsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,1]"));
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsInt32Async();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            Assert.AreEqual(1, await reader.ReadAsInt32Async());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadNumberValue_InvalidEndArrayAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsInt32Async();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadNumberValue_CommaErrors_MultipleAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[1,,1]"));
            await reader.ReadAsync();
            await reader.ReadAsInt32Async();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsInt32Async();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 4.");

            Assert.AreEqual(1, await reader.ReadAsInt32Async());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsString_UnexpectedEndAsync()
        {
            string json = @"tru";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public async Task ReadAsString_Null_UnexpectedEndAsync()
        {
            string json = @"nul";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
        }

        [Test]
        public async Task ReadStringValue_InvalidEndArrayAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("]"));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDateTimeAsync();
            }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ReadStringValue_CommaErrorsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[,'']"));
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsStringAsync();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

            Assert.AreEqual(string.Empty, await reader.ReadAsStringAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadStringValue_CommaErrors_MultipleAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['',,'']"));
            await reader.ReadAsync();
            await reader.ReadAsInt32Async();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsStringAsync();
            }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

            Assert.AreEqual(string.Empty, await reader.ReadAsStringAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadStringValue_Numbers_NotStringAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[56,56]"));
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDateTimeAsync();
            }, "Unexpected character encountered while parsing value: 5. Path '', line 1, position 2.");

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDateTimeAsync();
            }, "Unexpected character encountered while parsing value: 6. Path '', line 1, position 3.");

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDateTimeAsync();
            }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 4.");

            Assert.AreEqual(56, await reader.ReadAsInt32Async());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ErrorReadingCommentAsync()
        {
            string json = @"/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing comment. Path '', line 1, position 1.");
        }

        [Test]
        public async Task EscapedPathInExceptionMessageAsync()
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

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                async () =>
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader(json));
                    while (await reader.ReadAsync())
                    {
                    }
                },
                "Unexpected character encountered while parsing value: !. Path 'frameworks.dnxcore50.dependencies['System.Xml.ReaderWriter'].source', line 6, position 20.");
        }

        [Test]
        public async Task MaxDepthAsync()
        {
            string json = "[[]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
        }

        [Test]
        public async Task MaxDepthDoesNotRecursivelyErrorAsync()
        {
            string json = "[[[[]]],[[]]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(0, reader.Depth);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(3, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(3, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(1, reader.Depth);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(2, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(0, reader.Depth);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task UnexpectedEndWhenParsingUnquotedPropertyAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
        }
    }
}

#endif
