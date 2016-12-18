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

using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class CamelCasePropertyNamesContractResolverAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task JTokenWriterAsync()
        {
            JsonIgnoreAttributeOnClassTestClass ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass();
            ignoreAttributeOnClassTestClass.Field = int.MinValue;

            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

            JTokenWriter writer = new JTokenWriter();

            await serializer.SerializeAsync(writer, ignoreAttributeOnClassTestClass);

            JObject o = (JObject)writer.Token;
            JProperty p = o.Property("theField");

            Assert.IsNotNull(p);
            Assert.AreEqual(int.MinValue, (int)p.Value);

            string json = o.ToString();
        }
    }
}

#endif