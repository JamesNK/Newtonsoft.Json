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
    public class JRawTests : TestFixtureBase
    {
        [Test]
        public void RawEquals()
        {
            JRaw r1 = new JRaw("raw1");
            JRaw r2 = new JRaw("raw1");
            JRaw r3 = new JRaw("raw2");

            Assert.IsTrue(JToken.DeepEquals(r1, r2));
            Assert.IsFalse(JToken.DeepEquals(r1, r3));
        }

        [Test]
        public void RawClone()
        {
            JRaw r1 = new JRaw("raw1");
            JToken r2 = r1.CloneToken();

            CustomAssert.IsInstanceOfType(typeof(JRaw), r2);
        }

        [Test]
        public void RawToObject()
        {
            JRaw r1 = new JRaw("1");
            int i = r1.ToObject<int>();

            Assert.AreEqual(1, i);
        }
    }
}