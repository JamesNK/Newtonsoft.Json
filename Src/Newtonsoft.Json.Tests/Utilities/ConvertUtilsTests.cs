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

using System;
using System.Globalization;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
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
    public class ConvertUtilsTests : TestFixtureBase
    {
        [Test]
        public void Int64TryParse()
        {
            long l;
            char[] c = "43443333222211111117".ToCharArray();
            ParseResult result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.AreEqual(ParseResult.Overflow, result);

            c = "9223372036854775807".ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.AreEqual(ParseResult.Success, result);
            Assert.AreEqual(9223372036854775807L, l);

            c = "9223372036854775808".ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.AreEqual(ParseResult.Overflow, result);

            for (int i = 3; i < 10; i++)
            {
                c = ("9" + i + "23372036854775807").ToCharArray();
                result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
                Assert.AreEqual(ParseResult.Overflow, result);
            }

            c = "-9223372036854775808".ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.AreEqual(ParseResult.Success, result);
            Assert.AreEqual(-9223372036854775808L, l);

            c = "-9223372036854775809".ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.AreEqual(ParseResult.Overflow, result);

            for (int i = 3; i < 10; i++)
            {
                c = ("-9" + i + "23372036854775808").ToCharArray();
                result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
                Assert.AreEqual(ParseResult.Overflow, result);
            }
        }

        [Test]
        public void Int32TryParse()
        {
            int i;
            char[] c = "43443333227".ToCharArray();
            ParseResult result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
            Assert.AreEqual(ParseResult.Overflow, result);

            c = "2147483647".ToCharArray();
            result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
            Assert.AreEqual(ParseResult.Success, result);
            Assert.AreEqual(2147483647, i);

            c = "2147483648".ToCharArray();
            result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
            Assert.AreEqual(ParseResult.Overflow, result);

            c = "-2147483648".ToCharArray();
            result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
            Assert.AreEqual(ParseResult.Success, result);
            Assert.AreEqual(-2147483648, i);

            c = "-2147483649".ToCharArray();
            result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
            Assert.AreEqual(ParseResult.Overflow, result);

            for (int j = 2; j < 10; j++)
            {
                for (int k = 2; k < 10; k++)
                {
                    string t = j.ToString(CultureInfo.InvariantCulture) + k.ToString(CultureInfo.InvariantCulture) + "47483647";

                    c = t.ToCharArray();
                    result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);

                    Assert.AreEqual(ParseResult.Overflow, result);
                }
            }

            for (int j = 2; j < 10; j++)
            {
                for (int k = 2; k < 10; k++)
                {
                    string t = "-" + j.ToString(CultureInfo.InvariantCulture) + k.ToString(CultureInfo.InvariantCulture) + "47483648";

                    c = t.ToCharArray();
                    result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);

                    Assert.AreEqual(ParseResult.Overflow, result);
                }
            }
        }

        [Test]
        public void HexParse()
        {
            HexParseSame("0000");
            HexParseSame("1234");
            HexParseSame("4321");
            HexParseSame("abcd");
            HexParseSame("dcba");
            HexParseSame("ffff");
            HexParseSame("ABCD");
            HexParseSame("DCBA");
            HexParseSame("FFFF");
        }

        [Test]
        public void HexParseOffset()
        {
            int value = ConvertUtils.HexTextToInt("!0000".ToCharArray(), 1, 5);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void HexParseError()
        {
            ExceptionAssert.Throws<FormatException>(() => { ConvertUtils.HexTextToInt("-100".ToCharArray(), 0, 4); }, "Invalid hex character: -");
            ExceptionAssert.Throws<FormatException>(() => { ConvertUtils.HexTextToInt("000g".ToCharArray(), 0, 4); }, "Invalid hex character: g");
            ExceptionAssert.Throws<FormatException>(() => { ConvertUtils.HexTextToInt(" ssd".ToCharArray(), 0, 4); }, "Invalid hex character:  ");
            ExceptionAssert.Throws<FormatException>(() => { ConvertUtils.HexTextToInt("000:".ToCharArray(), 0, 4); }, "Invalid hex character: :");
            ExceptionAssert.Throws<FormatException>(() => { ConvertUtils.HexTextToInt("000G".ToCharArray(), 0, 4); }, "Invalid hex character: G");
        }

        private void HexParseSame(string text)
        {
            int v1 = int.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            int v2 = ConvertUtils.HexTextToInt(text.ToCharArray(), 0, 4);

            Assert.AreEqual(v1, v2, "Invalid result when parsing hex text: " + text);
        }
    }
}