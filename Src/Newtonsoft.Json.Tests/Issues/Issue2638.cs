#region License
// Copyright (c) 2022 James Newton-King
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

using Newtonsoft.Json.Linq;
using System.Globalization;
using Newtonsoft.Json.Tests.Documentation.Samples.Linq;

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
    public class Issue2638
    {
        [Test]
        public void DeserilizeUsesSharedBooleans()
        {
            Test(true);
            Test(false);
            void Test(bool value)
            {
                var obj = (JObject)JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value ? "true" : "false"));
                var x = ((JValue)obj["x"]).Value;
                var y = ((JValue)obj["y"]).Value;

                Assert.AreEqual(value, (bool)x);
                Assert.AreEqual(value, (bool)y);
                Assert.AreSame(x, y);
            }
        }

        [Test]
        public void DeserilizeUsesSharedDoubleZeros()
        {
            Test(0, true);
            Test(double.NaN, true);
            Test(double.NegativeInfinity, true);
            Test(double.PositiveInfinity, true);
            Test(1, false);
            Test(42.42, false);

            void Test(double value, bool expectSame)
            {
                var obj = (JObject)JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value.ToString("0.0###", CultureInfo.InvariantCulture)));
                var x = ((JValue)obj["x"]).Value;
                var y = ((JValue)obj["y"]).Value;

                Assert.AreEqual(value, (double)x);
                Assert.AreEqual(value, (double)y);
                if (expectSame)
                {
                    Assert.AreSame(x, y);
                }
                else
                {
                    Assert.AreNotSame(x, y);
                }
                var unboxed = (double)x;
                Assert.AreEqual(double.IsNaN(value), double.IsNaN(unboxed));
                Assert.AreEqual(double.IsPositiveInfinity(value), double.IsPositiveInfinity(unboxed));
                Assert.AreEqual(double.IsNegativeInfinity(value), double.IsNegativeInfinity(unboxed));
            }
        }

        [Test]
        public void DeserilizeUsesSharedSmallInt64()
        {
            Test(-2, false);
            Test(-1, true);
            Test(0, true);
            Test(1, true);
            Test(2, true);
            Test(3, true);
            Test(4, true);
            Test(5, true);
            Test(6, true);
            Test(7, true);
            Test(8, true);
            Test(9, false);

            void Test(long value, bool expectSame)
            {
                var obj = (JObject)JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value.ToString(CultureInfo.InvariantCulture)));
                var x = ((JValue)obj["x"]).Value;
                var y = ((JValue)obj["y"]).Value;

                Assert.AreEqual(value, (long)x);
                Assert.AreEqual(value, (long)y);
                if (expectSame)
                {
                    Assert.AreSame(x, y);
                }
                else
                {
                    Assert.AreNotSame(x, y);
                }
            }
        }
    }
}
