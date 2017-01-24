﻿#region License
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
#if DNXCORE50
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
        private void AssertDoubleTryParse(string s, ParseResult expectedResult, double? expectedValue)
        {
            double d;
            char[] c = s.ToCharArray();
            ParseResult result = ConvertUtils.DoubleTryParse(c, 0, c.Length, out d);

            double d2;
            bool result2 = double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d2)
                && !s.StartsWith(".")
                && !s.EndsWith(".")
                && !(s.StartsWith("0") && s.Length > 1 && !s.StartsWith("0.") && !s.StartsWith("0e", StringComparison.OrdinalIgnoreCase))
                && !(s.StartsWith("-0") && s.Length > 2 && !s.StartsWith("-0.") && !s.StartsWith("-0e", StringComparison.OrdinalIgnoreCase))
                && s.IndexOf(".e", StringComparison.OrdinalIgnoreCase) == -1;

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedResult == ParseResult.Success, result2);

            if (result2)
            {
                Assert.IsTrue(expectedValue.HasValue);

                Assert.AreEqual(expectedValue.Value, d, "Input string: " + s);

                Assert.AreEqual(expectedValue.Value, d2, "DoubleTryParse result is not equal to double.Parse. Input string: " + s);
            }
        }

        [Test]
        public void DoubleTryParse()
        {
            AssertDoubleTryParse("0e-10", ParseResult.Success, 0e-10);
            AssertDoubleTryParse("0E-10", ParseResult.Success, 0e-10);
            AssertDoubleTryParse("0.1", ParseResult.Success, 0.1);
            AssertDoubleTryParse("567.89", ParseResult.Success, 567.89);

            AssertDoubleTryParse("0.25e+5", ParseResult.Success, 25000d);
            AssertDoubleTryParse("0.25e-5", ParseResult.Success, 0.0000025d);

            AssertDoubleTryParse("-123", ParseResult.Success, -123);
            AssertDoubleTryParse("0", ParseResult.Success, 0);
            AssertDoubleTryParse("123", ParseResult.Success, 123);
            AssertDoubleTryParse("-567.89", ParseResult.Success, -567.89);
            AssertDoubleTryParse("1E23", ParseResult.Success, 1E23);
            AssertDoubleTryParse("1.1E23", ParseResult.Success, 1.1E23);
            AssertDoubleTryParse("1E+23", ParseResult.Success, 1E+23);
            AssertDoubleTryParse("1E-1", ParseResult.Success, 1E-1);
            AssertDoubleTryParse("1E-2", ParseResult.Success, 1E-2);
            AssertDoubleTryParse("1E-3", ParseResult.Success, 1E-3);
            AssertDoubleTryParse("1E-4", ParseResult.Success, 1E-4);
            AssertDoubleTryParse("1E-5", ParseResult.Success, 1E-5);
            AssertDoubleTryParse("1E-10", ParseResult.Success, 1E-10);
            AssertDoubleTryParse("1E-20", ParseResult.Success, 1E-20);
            AssertDoubleTryParse("1", ParseResult.Success, 1);
            AssertDoubleTryParse("1.2", ParseResult.Success, 1.2);

            AssertDoubleTryParse("1E-21", ParseResult.Success, 1E-21);
            AssertDoubleTryParse("1E-22", ParseResult.Success, 1E-22);
            AssertDoubleTryParse("1E-23", ParseResult.Success, 1E-23);
            AssertDoubleTryParse("1E-25", ParseResult.Success, 1E-25);
            AssertDoubleTryParse("1E-50", ParseResult.Success, 1E-50);
            AssertDoubleTryParse("1E-75", ParseResult.Success, 1E-75);
            AssertDoubleTryParse("1E-100", ParseResult.Success, 1E-100);
            AssertDoubleTryParse("1E-300", ParseResult.Success, 1E-300);

            AssertDoubleTryParse("1E+309", ParseResult.Overflow, null);
            AssertDoubleTryParse("-1E+5000", ParseResult.Overflow, null);

            AssertDoubleTryParse("01E308", ParseResult.Invalid, null);
            AssertDoubleTryParse("-01E308", ParseResult.Invalid, null);
            AssertDoubleTryParse("1.", ParseResult.Invalid, null);
            AssertDoubleTryParse("0.", ParseResult.Invalid, null);
            AssertDoubleTryParse(".1E23", ParseResult.Invalid, null);
            AssertDoubleTryParse("1..1E23", ParseResult.Invalid, null);
            AssertDoubleTryParse("1.E23", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E2.3", ParseResult.Invalid, null);
            AssertDoubleTryParse("1EE-10", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E-1-0", ParseResult.Invalid, null);
            AssertDoubleTryParse("1-E10", ParseResult.Invalid, null);
            AssertDoubleTryParse("", ParseResult.Invalid, null);
            AssertDoubleTryParse("5.1231231E", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E+23i", ParseResult.Invalid, null);
            AssertDoubleTryParse("1EE+23", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E++23", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E--23", ParseResult.Invalid, null);
            AssertDoubleTryParse("E23", ParseResult.Invalid, null);

            AssertDoubleTryParse("4.94065645841247E-324", ParseResult.Success, 4.94065645841247E-324);
            AssertDoubleTryParse("4.94065645841247E-342", ParseResult.Success, 4.94065645841247E-342);
            AssertDoubleTryParse("4.94065645841247E-555", ParseResult.Success, 0);

            AssertDoubleTryParse("1.7976931348623157E+308", ParseResult.Success, double.MaxValue);
            AssertDoubleTryParse("-1.7976931348623157E+308", ParseResult.Success, double.MinValue);

            AssertDoubleTryParse("1.7976931348623159E+308", ParseResult.Overflow, null);
            AssertDoubleTryParse("-1.7976931348623159E+308", ParseResult.Overflow, null);

            AssertDoubleTryParse("1E4294967297", ParseResult.Overflow, null);
            AssertDoubleTryParse("1E4294967297B", ParseResult.Invalid, null);
            AssertDoubleTryParse("1E-4294967297", ParseResult.Success, 0);
        }

        [Test]
        public void DoubleTryParse_Exponents()
        {
            AssertDoubleTryParse("4.94065645841247e-324", ParseResult.Success, double.Epsilon);

            AssertDoubleTryParse("4.9406564584124654E-324", ParseResult.Success, 4.9406564584124654E-324);
            AssertDoubleTryParse("4.9406564584124654E-325", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-460", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-461", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-462", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-463", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-464", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-465", ParseResult.Success, 0);
            AssertDoubleTryParse("4.94065645841247E-555", ParseResult.Success, 0);

            AssertDoubleTryParse("4.94065645841247E+555", ParseResult.Overflow, null);
        }

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
            int value;
            Assert.IsTrue(ConvertUtils.TryHexTextToInt("!0000".ToCharArray(), 1, 5, out value));
            Assert.AreEqual(0, value);
        }

        [Test]
        public void HexParseError()
        {
            int value;
            Assert.IsFalse(ConvertUtils.TryHexTextToInt("-100".ToCharArray(), 0, 4, out value));
            Assert.IsFalse(ConvertUtils.TryHexTextToInt("000g".ToCharArray(), 0, 4, out value));
            Assert.IsFalse(ConvertUtils.TryHexTextToInt(" ssd".ToCharArray(), 0, 4, out value));
            Assert.IsFalse(ConvertUtils.TryHexTextToInt("000:".ToCharArray(), 0, 4, out value));
            Assert.IsFalse(ConvertUtils.TryHexTextToInt("000G".ToCharArray(), 0, 4, out value));
        }

        private void HexParseSame(string text)
        {
            int v1 = int.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            int v2;
            Assert.IsTrue(ConvertUtils.TryHexTextToInt(text.ToCharArray(), 0, 4, out v2));

            Assert.AreEqual(v1, v2, "Invalid result when parsing hex text: " + text);
        }
    }
}