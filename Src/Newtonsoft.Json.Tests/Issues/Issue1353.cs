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

#if !(NET20 || NET35)
using System;
using System.Collections.Concurrent;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1353 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            ConcurrentDictionary<string, string> d1 = new ConcurrentDictionary<string, string>();
            d1.TryAdd("key!", "value!");

            string json = JsonConvert.SerializeObject(d1, Formatting.Indented);

            ConcurrentDictionary<string, string> d2 = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json);

            Assert.AreEqual(1, d2.Count);
            Assert.AreEqual("value!", d2["key!"]);
        }
    }
}
#endif