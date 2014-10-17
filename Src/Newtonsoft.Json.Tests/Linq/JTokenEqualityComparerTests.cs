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

using System.Collections.Generic;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JTokenEqualityComparerTests : TestFixtureBase
    {
        [Test]
        public void CompareEmptyProperties()
        {
            JObject o1 = JObject.Parse("{}");
            o1.Add(new JProperty("hi"));

            JObject o2 = JObject.Parse("{}");
            o2.Add(new JProperty("hi"));

            JTokenEqualityComparer c = new JTokenEqualityComparer();
            Assert.IsTrue(c.Equals(o1, o2));

            o1["hi"] = 10;
            Assert.IsFalse(c.Equals(o1, o2));
        }

        [Test]
        public void JValueDictionary()
        {
            Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
            JValue v11 = new JValue(1);
            JValue v12 = new JValue(1);

            dic[v11] = 1;
            dic[v12] += 1;
            Assert.AreEqual(2, dic[v11]);
        }

        [Test]
        public void JArrayDictionary()
        {
            Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
            JArray v11 = new JArray();
            JArray v12 = new JArray();

            dic[v11] = 1;
            dic[v12] += 1;
            Assert.AreEqual(2, dic[v11]);
        }

        [Test]
        public void JObjectDictionary()
        {
            Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
            JObject v11 = new JObject() { { "Test", new JValue(1) }, { "Test1", new JValue(1) } };
            JObject v12 = new JObject() { { "Test", new JValue(1) }, { "Test1", new JValue(1) } };

            dic[v11] = 1;
            dic[v12] += 1;
            Assert.AreEqual(2, dic[v11]);
        }

        [Test]
        public void JConstructorDictionary()
        {
            Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
            JConstructor v11 = new JConstructor("ConstructorValue");
            JConstructor v12 = new JConstructor("ConstructorValue");

            dic[v11] = 1;
            dic[v12] += 1;
            Assert.AreEqual(2, dic[v11]);
        }
    }
}