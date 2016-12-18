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

using System.IO;
using System.Threading.Tasks;
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
    public class ContractResolverAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task SerializeWithEscapedPropertyNameAsync()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertySuffix = @"-'-""-"
                    }
                });

            Assert.AreEqual(@"{""AddressLine1-'-\""-"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            await reader.ReadAsync();
            await reader.ReadAsync();

            Assert.AreEqual(@"AddressLine1-'-""-", reader.Value);
        }

        [Test]
        public async Task SerializeWithHtmlEscapedPropertyNameAsync()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertyPrefix = "<b>",
                        PropertySuffix = "</b>"
                    },
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                });

            Assert.AreEqual(@"{""\u003cb\u003eAddressLine1\u003c/b\u003e"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            await reader.ReadAsync();
            await reader.ReadAsync();

            Assert.AreEqual(@"<b>AddressLine1</b>", reader.Value);
        }
    }
}

#endif