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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Utilities
{
    [TestFixture]
    public class StringUtilsTests : TestFixtureBase
    {
        [Test]
        public void ToCamelCaseTest()
        {
            Assert.AreEqual("urlValue", StringUtils.ToCamelCase("URLValue"));
            Assert.AreEqual("url", StringUtils.ToCamelCase("URL"));
            Assert.AreEqual("id", StringUtils.ToCamelCase("ID"));
            Assert.AreEqual("i", StringUtils.ToCamelCase("I"));
            Assert.AreEqual("", StringUtils.ToCamelCase(""));
            Assert.AreEqual(null, StringUtils.ToCamelCase(null));
            Assert.AreEqual("iPhone", StringUtils.ToCamelCase("iPhone"));
            Assert.AreEqual("person", StringUtils.ToCamelCase("Person"));
            Assert.AreEqual("iPhone", StringUtils.ToCamelCase("IPhone"));
            Assert.AreEqual("i Phone", StringUtils.ToCamelCase("I Phone"));
            Assert.AreEqual(" IPhone", StringUtils.ToCamelCase(" IPhone"));
        }
    }
}