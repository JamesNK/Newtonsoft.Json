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

#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class JsonArrayAttributeTests : TestFixtureBase
    {
        [Test]
        public void IsReferenceTest()
        {
            JsonPropertyAttribute attribute = new JsonPropertyAttribute();
            Assert.AreEqual(null, attribute._isReference);
            Assert.AreEqual(false, attribute.IsReference);

            attribute.IsReference = false;
            Assert.AreEqual(false, attribute._isReference);
            Assert.AreEqual(false, attribute.IsReference);

            attribute.IsReference = true;
            Assert.AreEqual(true, attribute._isReference);
            Assert.AreEqual(true, attribute.IsReference);
        }

        [Test]
        public void NullValueHandlingTest()
        {
            JsonPropertyAttribute attribute = new JsonPropertyAttribute();
            Assert.AreEqual(null, attribute._nullValueHandling);
            Assert.AreEqual(NullValueHandling.Include, attribute.NullValueHandling);

            attribute.NullValueHandling = NullValueHandling.Ignore;
            Assert.AreEqual(NullValueHandling.Ignore, attribute._nullValueHandling);
            Assert.AreEqual(NullValueHandling.Ignore, attribute.NullValueHandling);
        }

        [Test]
        public void DefaultValueHandlingTest()
        {
            JsonPropertyAttribute attribute = new JsonPropertyAttribute();
            Assert.AreEqual(null, attribute._defaultValueHandling);
            Assert.AreEqual(DefaultValueHandling.Include, attribute.DefaultValueHandling);

            attribute.DefaultValueHandling = DefaultValueHandling.Ignore;
            Assert.AreEqual(DefaultValueHandling.Ignore, attribute._defaultValueHandling);
            Assert.AreEqual(DefaultValueHandling.Ignore, attribute.DefaultValueHandling);
        }
    }
}