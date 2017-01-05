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
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class ExtensionDataAsyncTests : TestFixtureBase
    {
        public class Item
        {
            [JsonExtensionData]
            public IDictionary<string, JToken> ExtensionData;

            public IEnumerable<string> Foo
            {
                get { yield return "foo"; yield return "bar"; }
            }
        }

        [Test]
        public async Task Deserialize_WriteJsonDirectlyToJTokenAsync()
        {
            JsonSerializer jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            StringWriter stringWriter = new StringWriter();
            await jsonSerializer.SerializeAsync(stringWriter, new Item());
            string str = stringWriter.GetStringBuilder().ToString();
            Item deserialize = await jsonSerializer.DeserializeAsync<Item>(new JsonTextReader(new StringReader(str)));

            JToken value = deserialize.ExtensionData["Foo"]["$type"];
            Assert.AreEqual(JTokenType.String, value.Type);
            Assert.AreEqual("foo", (string)deserialize.ExtensionData["Foo"]["$values"][0]);
            Assert.AreEqual("bar", (string)deserialize.ExtensionData["Foo"]["$values"][1]);
        }

        public class ItemWithConstructor
        {
            [JsonExtensionData]
            public IDictionary<string, JToken> ExtensionData;

            public ItemWithConstructor(string temp)
            {
            }

            public IEnumerable<string> Foo
            {
                get { yield return "foo"; yield return "bar"; }
            }
        }

        [Test]
        public async Task DeserializeWithConstructor_WriteJsonDirectlyToJToken()
        {
            JsonSerializer jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            StringWriter stringWriter = new StringWriter();
            await jsonSerializer.SerializeAsync(stringWriter, new ItemWithConstructor(null));
            string str = stringWriter.GetStringBuilder().ToString();
            Item deserialize = await jsonSerializer.DeserializeAsync<Item>(new JsonTextReader(new StringReader(str)));

            JToken value = deserialize.ExtensionData["Foo"]["$type"];
            Assert.AreEqual(JTokenType.String, value.Type);
            Assert.AreEqual("foo", (string)deserialize.ExtensionData["Foo"]["$values"][0]);
            Assert.AreEqual("bar", (string)deserialize.ExtensionData["Foo"]["$values"][1]);
        }
    }
}

#endif