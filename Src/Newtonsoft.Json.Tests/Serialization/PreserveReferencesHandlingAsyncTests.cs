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
using System.Text;
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
    public class PreserveReferencesHandlingAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task PreserveReferencesHandlingWithReusedJsonSerializerAsync()
        {
            MyClass c = new MyClass();

            IList<MyClass> myClasses1 = new List<MyClass>
            {
                c,
                c
            };

            var ser = new JsonSerializer()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };

            MemoryStream ms = new MemoryStream();

            using (var sw = new StreamWriter(ms))
            using (var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                await ser.SerializeAsync(writer, myClasses1);
            }

            byte[] data = ms.ToArray();
            string json = Encoding.UTF8.GetString(data, 0, data.Length);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""$id"": ""2"",
      ""PreProperty"": 0,
      ""PostProperty"": 0
    },
    {
      ""$ref"": ""2""
    }
  ]
}", json);

            ms = new MemoryStream(data);
            IList<MyClass> myClasses2;

            using (var sr = new StreamReader(ms))
            using (var reader = new JsonTextReader(sr))
            {
                myClasses2 = await ser.DeserializeAsync<IList<MyClass>>(reader);
            }

            Assert.AreEqual(2, myClasses2.Count);
            Assert.AreEqual(myClasses2[0], myClasses2[1]);

            Assert.AreNotEqual(myClasses1[0], myClasses2[0]);
        }
    }
}

#endif
