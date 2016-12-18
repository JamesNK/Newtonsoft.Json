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
using Newtonsoft.Json.Tests.TestObjects;
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
    public class JsonSerializerCollectionsAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task CustomCollectionSerializationAsync()
        {
            ProductCollection collection = new ProductCollection
            {
                new Product { Name = "Test1" },
                new Product { Name = "Test2" },
                new Product { Name = "Test3" }
            };

            JsonSerializer jsonSerializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            StringWriter sw = new StringWriter();

            await jsonSerializer.SerializeAsync(sw, collection);

            Assert.AreEqual(@"[{""Name"":""Test1"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test2"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test3"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null}]",
                sw.GetStringBuilder().ToString());

            ProductCollection collectionNew = (ProductCollection)await jsonSerializer.DeserializeAsync(new JsonTextReader(new StringReader(sw.GetStringBuilder().ToString())), typeof(ProductCollection));

            CollectionAssert.AreEqual(collection, collectionNew);
        }
    }
}

#endif