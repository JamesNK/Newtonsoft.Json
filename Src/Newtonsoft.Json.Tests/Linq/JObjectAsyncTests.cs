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

using System.Collections.Generic;
using Newtonsoft.Json.Tests.TestObjects;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JObjectAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task ReadWithSupportMultipleContentAsync()
        {
            string json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";

            IList<JObject> roles = new List<JObject>();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.SupportMultipleContent = true;

            while (true)
            {
                JObject role = (JObject)await JToken.ReadFromAsync(reader);

                roles.Add(role);

                if (!await reader.ReadAsync())
                {
                    break;
                }
            }

            Assert.AreEqual(2, roles.Count);
            Assert.AreEqual("Admin", (string)roles[0]["name"]);
            Assert.AreEqual("Publisher", (string)roles[1]["name"]);
        }

        [Test]
        public async Task JTokenReaderAsync()
        {
            PersonRaw raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            JObject o = JObject.FromObject(raw);

            JsonReader reader = new JTokenReader(o);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Raw, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task LoadFromNestedObjectAsync()
        {
            string jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

            JsonReader reader = new JsonTextReader(new StringReader(jsonText));
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();

            JObject o = (JObject)await JToken.ReadFromAsync(reader);
            Assert.IsNotNull(o);
            StringAssert.AreEqual(@"{
  ""code"": 0,
  ""msg"": ""No action taken""
}", o.ToString(Formatting.Indented));
        }

        [Test]
        public async Task LoadFromNestedObjectIncompleteAsync()
        {
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                string jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0";

                JsonReader reader = new JsonTextReader(new StringReader(jsonText));
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();

                await JToken.ReadFromAsync(reader);
            }, "Unexpected end of content while loading JObject. Path 'short.error.code', line 6, position 14.");
        }
    }
}

#endif