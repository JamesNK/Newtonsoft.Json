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
    public class EqualityTests
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
            AssertAll(strictEquality, target, rhs.Null);            
            AssertNone(strictEquality, target, rhs.ErrybodyButNull);

            // given x === y, if x is the same Number value as y, return true.
            target = lhs.One;
            AssertAll(strictEquality, target, rhs.One, rhs.OneDotZero);
            Assert.IsFalse(BooleanQueryExpression.IsStrictMatch(target, rhs.Two));
            target = lhs.Scientific;
            Assert.IsTrue(BooleanQueryExpression.IsStrictMatch(target, rhs.Scientific));

            // given x === y, if Type(x) is String, then return true if x and y are exactly the same sequence of characters (same length and same characters in corresponding positions); otherwise, return false.
            target = lhs.DerpString;
            AssertNone(strictEquality, target, rhs.HerpString);
            AssertAll(strictEquality, target, rhs.DerpString);

            // given x === y, if Type(x) is Boolean, return true if x and y are both true or both false; otherwise, return false.
            target = lhs.True;
            AssertAll(strictEquality, target, rhs.True);
            AssertNone(strictEquality, target, new[] { rhs.False }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Dates);
            target = lhs.False;
            AssertAll(strictEquality, target, rhs.False);
            AssertNone(strictEquality, target, new[] { rhs.True }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Dates);

            //Dates 
            target = lhs.DateYearMonth;
            AssertAll(strictEquality, target, rhs.DateYearMonth);
            AssertNone(strictEquality, target, rhs.DateYear);
            target = lhs.DateYear;
            AssertNone(strictEquality, target, rhs.DateYearMonth);
            AssertAll(strictEquality, target, rhs.DateYear);
            target = lhs.DateISO;
            Assert.IsTrue(BooleanQueryExpression.IsStrictMatch(target, rhs.DateISO));
            Assert.IsFalse(BooleanQueryExpression.IsStrictMatch(target, rhs.OtherISODate));
        }
        #region helpers        
        // used by asserters to perform the comparison
        public delegate bool Comparator(JToken lhs, JToken rhs);

        // there was going to be an abstractEquality, but check the exception for it's implementation for why that's skipped for now
        readonly Comparator strictEquality = (lhs, rhs) => BooleanQueryExpression.IsStrictMatch(lhs, rhs);
        // a bunch of convenience methods for the test belwo
        // these make sure the comparator returns false for all do not wants
        private void AssertNone(Comparator comparator, JToken token, params JToken[][] doNotWant)
        {
            foreach(var group in doNotWant)
                AssertNone(comparator, token, group);
        }
        private void AssertNone(Comparator comparator, JToken token, params JToken[] doNotWant)
        {
            foreach(var item in doNotWant)
            Assert.IsTrue(!comparator(token, item));
        }
        // these make sure the comparator returns true for all do not wants
        private void AssertAll(Comparator comparator, JToken token, params JToken[][] want)
        {
            foreach(var group in want)
                AssertAll(comparator, token, group);
        }
        private void AssertAll(Comparator comparator, JToken token, params JToken[] want)
        {
            foreach(var item in want)
                Assert.IsTrue(comparator(token, item));
        }
        #endregion
    }

    /// <summary>
    /// Holds (practically) all the different possible javascript types and variants of possible values gathered from the algorithm and (imperfect) observation
    /// </summary>
    public class TestData
    {
        public readonly JToken Null;
        //JSON.stringify({"undef": undefined}) returns {}
        //public readonly JToken Undefined;
        public readonly JToken[] Nopes;

        public readonly JToken One;
        public readonly JToken OneDotZero;
        public readonly JToken Two;
        public readonly JToken Scientific;
        // stringify returns these as 0
        //public readonly JToken NegativeZero;
        //public readonly JToken PositiveZero;
        // JSON.stringify({"lol": NaN}) returns "{"lol":null}"
        //public readonly JToken NaN;
        public readonly JToken[] Numbers;

        public readonly JToken HerpString;
        public readonly JToken DerpString;
        public readonly JToken[] Strings;

        public readonly JToken True;
        public readonly JToken False;
        public readonly JToken[] Boolies;

        // JSON.stringify({"lol": new Date("2018-09-02") - new Date("2018-09-01")}) returns "{"lol":86400000}", and so is indistinguishable from a number
        //public readonly JToken Timespan1;
        //public readonly JToken Timespan2;
        //public readonly JToken[] Timespans;

        public readonly JToken DateYearMonth;
        public readonly JToken DateYear;
        // stringify only ever uses the ISO 8601 zulu date format, so let's just bother with that one.
        public readonly JToken DateISO;
        public readonly JToken OtherISODate;
        public readonly JToken[] Dates;

        public readonly JToken[][] Errybody;
        public readonly JToken[][] ErrybodyButNull;
        public TestData()
        {
            var shebang = JObject.Parse("{\"null\":null,\"NaN\":null,\"true\":true,\"false\":false,\"two\":2,\"int\":1,\"float\":1.0,\"scifloat\":-1.3e+70,\"herp\":\"herp\",\"derp\":\"derp\",\"timespan\":86400000,\"dateYearMonth\":\"2018-09-01T00: 00:00.000Z\",\"dateYear\":\"2018-01-01T00: 00:00.000Z\",\"dateJSONAndISOZulu\":\"2018-09-20T20:38:59.463Z\", \"otherDate\": \"2018-09-20T20:41:14.821Z\"}");
            Null = shebang["null"];
            One = shebang["int"];
            OneDotZero = shebang["float"];
            Two = shebang["two"];
            Scientific = shebang["scifloat"];
            True = shebang["true"];
            False = shebang["false"];
            HerpString = shebang["herp"];
            DerpString = shebang["derp"];
            DateYearMonth = shebang["dateYearMonth"];
            DateYear = shebang["dateYear"];
            DateISO = shebang["dateJSONAndISOZulu"];
            OtherISODate = shebang["otherDate"];
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
