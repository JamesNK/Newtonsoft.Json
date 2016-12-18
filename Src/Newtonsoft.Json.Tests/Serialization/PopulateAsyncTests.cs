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
using Newtonsoft.Json.Tests.TestObjects.Organization;
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
    public class PopulateAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task PopulateListOfPeopleAsync()
        {
            List<Person> p = new List<Person>();

            JsonSerializer serializer = new JsonSerializer();
            await serializer.PopulateAsync(new StringReader(@"[{""Name"":""James""},{""Name"":""Jim""}]"), p);

            Assert.AreEqual(2, p.Count);
            Assert.AreEqual("James", p[0].Name);
            Assert.AreEqual("Jim", p[1].Name);
        }

        [Test]
        public async Task PopulateDictionaryAsync()
        {
            Dictionary<string, string> p = new Dictionary<string, string>();

            JsonSerializer serializer = new JsonSerializer();
            await serializer.PopulateAsync(new StringReader(@"{""Name"":""James""}"), p);

            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("James", p["Name"]);
        }
    }
}

#endif
