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

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using System;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    /// <summary>
    /// Here's where we test the sandbox for needles and broken glass
    /// </summary>
    [TestFixture]
    public class Issue1837
    {
        [Test]
        public void AllStrictEqualityTests()
        {
            // this is a bit cargo-culty; making absolutely sure no false positives caused by instance equivalence
            var lhs = new TestData();
            var rhs = new TestData();

            // For all tests, if Type(x) is different from Type(y), return false.
            // given x === y, if Type(x) is Null, return true 
            var target = lhs.Null;
            AssertAll(StrictEquality, target, rhs.Null);            
            AssertNone(StrictEquality, target, rhs.ErrybodyButNull);

            // given x === y, if x is the same Number value as y, return true.
            target = lhs.One;
            AssertAll(StrictEquality, target, rhs.One, rhs.OneDotZero);
            Assert.IsFalse(BooleanQueryExpression.EqualsWithStrictMatch(target, rhs.Two));
            target = lhs.Scientific;
            Assert.IsTrue(BooleanQueryExpression.EqualsWithStrictMatch(target, rhs.Scientific));

            // given x === y, if Type(x) is String, then return true if x and y are exactly the same sequence of characters (same length and same characters in corresponding positions); otherwise, return false.
            target = lhs.DerpString;
            AssertNone(StrictEquality, target, rhs.HerpString);
            AssertAll(StrictEquality, target, rhs.DerpString);

            // given x === y, if Type(x) is Boolean, return true if x and y are both true or both false; otherwise, return false.
            target = lhs.True;
            AssertAll(StrictEquality, target, rhs.True);
            AssertNone(StrictEquality, target, new[] { rhs.False }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Dates);
            target = lhs.False;
            AssertAll(StrictEquality, target, rhs.False);
            AssertNone(StrictEquality, target, new[] { rhs.True }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Dates);

            //Dates 
            target = lhs.DateYearMonth;
            AssertAll(StrictEquality, target, rhs.DateYearMonth);
            AssertNone(StrictEquality, target, rhs.DateYear);
            target = lhs.DateYear;
            AssertNone(StrictEquality, target, rhs.DateYearMonth);
            AssertAll(StrictEquality, target, rhs.DateYear);
            target = lhs.DateISO;
            Assert.IsTrue(BooleanQueryExpression.EqualsWithStrictMatch(target, rhs.DateISO));
            Assert.IsFalse(BooleanQueryExpression.EqualsWithStrictMatch(target, rhs.OtherISODate));
        }

        #region helpers        
        // used by asserters to perform the comparison
        public delegate bool Comparator(JValue lhs, JValue rhs);

        // there was going to be an abstractEquality, but check the exception for it's implementation for why that's skipped for now
        private readonly Comparator StrictEquality = (lhs, rhs) => BooleanQueryExpression.EqualsWithStrictMatch(lhs, rhs);
 
        // a bunch of convenience methods for the test belwo
        // these make sure the comparator returns false for all do not wants
        private void AssertNone(Comparator comparator, JValue token, params JValue[][] doNotWant)
        {
            foreach (var group in doNotWant)
            {
                AssertNone(comparator, token, group);
            }
        }

        private void AssertNone(Comparator comparator, JValue token, params JValue[] doNotWant)
        {
            foreach (var item in doNotWant)
            {
                Assert.IsTrue(!comparator(token, item));
            }
        }

        // these make sure the comparator returns true for all do not wants
        private void AssertAll(Comparator comparator, JValue token, params JValue[][] want)
        {
            foreach (var group in want)
            {
                AssertAll(comparator, token, group);
            }
        }

        private void AssertAll(Comparator comparator, JValue token, params JValue[] want)
        {
            foreach (var item in want)
            {
                Assert.IsTrue(comparator(token, item));
            }
        }
        #endregion
    }

    /// <summary>
    /// Holds (practically) all the different possible javascript types and variants of possible values gathered from the algorithm and (imperfect) observation
    /// </summary>
    public class TestData
    {
        public readonly JValue Null;
        //JSON.stringify({"undef": undefined}) returns {}
        //public readonly JToken Undefined;
        public readonly JValue[] Nopes;

        public readonly JValue One;
        public readonly JValue OneDotZero;
        public readonly JValue Two;
        public readonly JValue Scientific;
        // stringify returns these as 0
        //public readonly JToken NegativeZero;
        //public readonly JToken PositiveZero;
        // JSON.stringify({"lol": NaN}) returns "{"lol":null}"
        //public readonly JToken NaN;
        public readonly JValue[] Numbers;

        public readonly JValue HerpString;
        public readonly JValue DerpString;
        public readonly JValue[] Strings;

        public readonly JValue True;
        public readonly JValue False;
        public readonly JValue[] Boolies;

        // JSON.stringify({"lol": new Date("2018-09-02") - new Date("2018-09-01")}) returns "{"lol":86400000}", and so is indistinguishable from a number
        //public readonly JToken Timespan1;
        //public readonly JToken Timespan2;
        //public readonly JToken[] Timespans;

        public readonly JValue DateYearMonth;
        public readonly JValue DateYear;
        // stringify only ever uses the ISO 8601 zulu date format, so let's just bother with that one.
        public readonly JValue DateISO;
        public readonly JValue OtherISODate;
        public readonly JValue[] Dates;

        public readonly JValue[][] Errybody;
        public readonly JValue[][] ErrybodyButNull;

        public TestData()
        {
            var shebang = JObject.Parse("{\"null\":null,\"NaN\":null,\"true\":true,\"false\":false,\"two\":2,\"int\":1,\"float\":1.0,\"scifloat\":-1.3e+70,\"herp\":\"herp\",\"derp\":\"derp\",\"timespan\":86400000,\"dateYearMonth\":\"2018-09-01T00: 00:00.000Z\",\"dateYear\":\"2018-01-01T00: 00:00.000Z\",\"dateJSONAndISOZulu\":\"2018-09-20T20:38:59.463Z\", \"otherDate\": \"2018-09-20T20:41:14.821Z\"}");
            Null = (JValue)shebang["null"];
            One = (JValue)shebang["int"];
            OneDotZero = (JValue)shebang["float"];
            Two = (JValue)shebang["two"];
            Scientific = (JValue)shebang["scifloat"];
            True = (JValue)shebang["true"];
            False = (JValue)shebang["false"];
            HerpString = (JValue)shebang["herp"];
            DerpString = (JValue)shebang["derp"];
            DateYearMonth = (JValue)shebang["dateYearMonth"];
            DateYear = (JValue)shebang["dateYear"];
            DateISO = (JValue)shebang["dateJSONAndISOZulu"];
            OtherISODate = (JValue)shebang["otherDate"];
            Dates = new[] { DateYearMonth, DateYear, DateISO, OtherISODate };
            Boolies = new[] { True, False };
            Strings = new[]
            {
                HerpString,
                DerpString
            };
            Numbers = new[]
            {
                One,
                OneDotZero,
                Two,
                Scientific
            };
            Nopes = new[]
            {
                Null
            };
            Errybody = new[] { Nopes, Numbers, Strings, Boolies, Dates };
            ErrybodyButNull = new[] { Numbers, Strings, Boolies, Dates };
        }
    }
}
