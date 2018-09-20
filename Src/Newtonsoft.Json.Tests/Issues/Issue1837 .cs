using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Tests.Issues
{
    /// <summary>
    /// Here's a sandbox for testing Strict and Abstract equality algorithms. 
    /// </summary>
    public static class JTokenExtensions
    {
        /// <summary>
        /// Performs an abstract equality comparison.
        /// </summary>
        /// <remarks>
        /// This method uses the algorithm described in section 11.9.3 of Ecma-262 Edition 5.1, The ECMAScript Language Specification.
        /// </remarks>
        /// <param name="value"><see cref="JToken"/></param>
        /// <param name="other"><see cref="JToken"/></param>
        /// <returns>
        ///   <see cref="System.Boolean"/>
        /// </returns>
        public static bool AbstractEquality(this JToken value, JToken other)
        {
            var exp = new BooleanQueryExpression();
            exp.Left = value;
            exp.Right = other;
            exp.Operator = QueryOperator.Equals;
            throw new NotImplementedException("I tried to hack this, but I haven't yet understood how the heck BooleanQueryExpression.IsMatch works. Why root? What's t? I've abandoned this part of the sandbox to the junkies.");
        }
        /// <summary>
        /// Performs a strict equality comparison.
        /// </summary>
        /// <remarks>
        /// This method uses the algorithm described in section 11.9.6 of Ecma-262 Edition 5.1, The ECMAScript Language Specification.
        /// </remarks>
        /// <param name="value"><see cref="JToken"/></param>
        /// <param name="other"><see cref="JToken"/></param>
        /// <returns>
        ///   <see cref="System.Boolean"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> or <paramref name="other"/> is <c>null</c>.</exception>
        public static bool StrictEquality(this JToken value, JToken other)
        {
            /* 
             * If Type(x) is different from Type(y), return false.
             * If Type(x) is Undefined, return true.
             * If Type(x) is Null, return true.
             * If Type(x) is Number, then 
             *     If x is NaN, return false.
             *     If y is NaN, return false.
             *     If x is the same Number value as y, return true.
             *     If x is +0 and y is −0, return true.
             *     If x is −0 and y is +0, return true.
             *     Return false.
             * If Type(x) is String, then return true if x and y are exactly the same sequence of characters (same length and same characters in corresponding positions); otherwise, return false.
             * If Type(x) is Boolean, return true if x and y are both true or both false; otherwise, return false.
             * Return true if x and y refer to the same object. Otherwise, return false.
             */
            if(value == null)
                throw new ArgumentNullException(nameof(value));
            if(other == null)
                throw new ArgumentNullException(nameof(other));

            if(value.Type != other.Type) return false;

            switch(value.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return true;
                case JTokenType.Integer:
                case JTokenType.Float:
                    return value.Value<float>() == other.Value<float>();
                case JTokenType.String:
                    return string.Equals(value.Value<string>(), other.Value<string>(), StringComparison.Ordinal);
                case JTokenType.Boolean:
                    return value.Value<bool>() == other.Value<bool>();
                case JTokenType.Date:
                    return value.Value<DateTime>() == other.Value<DateTime>();
                // How the heck could this happen? Ain't no guids in ecmascript.
                case JTokenType.Guid:
                    return new Guid(value.Value<string>()) == new Guid(other.Value<string>());
                case JTokenType.TimeSpan:
                    return new TimeSpan(value.Value<long>()) == new TimeSpan(other.Value<long>());
                // unsure of Uri; it appears (from ad hoc testing) that javascript does not consider equivalent URLs to be equal no matter what
                // new URL("http://lol.com") === new URL("http://lol.com") returns false always
                // also, JSON.stringify renders {} for urls. JSON.stringify({lol: new URL("http://lol.com")}) renders "{"lol":{}}"
                // so I don't know how we could get this kind of token type
                case JTokenType.Uri:
                default:
                    throw new InvalidOperationException($"Unexpected or unsupported JTokenType {value.Type}");
            }
        }
    }
    /// <summary>
    /// Holds (practically) all the different possible javascript types and variants of possible values gathered from the algorithm and (imperfect) observation used in the test sandbox
    /// </summary>
    public class TestData
    {
        public readonly JToken Null = JToken.Parse("{ \"Value\": \"null\" }")["Value"];
        public readonly JToken Undefined = JToken.Parse("{ \"Value\": \"Undefined\" }")["Value"];
        public readonly JToken[] Nopes;

        public readonly JToken One = JToken.Parse("{ \"Value\": \"1\" }")["Value"];
        public readonly JToken OneDotZero = JToken.Parse("{ \"Value\": \"1.0\" }")["Value"];
        public readonly JToken NegativeZero = JToken.Parse("{ \"Value\": \"-0\" }")["Value"];
        public readonly JToken PositiveZero = JToken.Parse("{ \"Value\": \"+0\" }")["Value"];
        public readonly JToken NaN = JToken.Parse("{ \"Value\": \"NaN\" }")["Value"];
        public readonly JToken[] Numbers;

        public readonly JToken HerpString = JToken.Parse("{ \"Value\": \"herp\" }")["Value"];
        public readonly JToken DerpString = JToken.Parse("{ \"Value\": \"derp\" }")["Value"];
        public readonly JToken[] Strings;

        public readonly JToken True = JToken.Parse("{ \"Value\": \"true\" }")["Value"];
        public readonly JToken False = JToken.Parse("{ \"Value\": \"false\" }")["Value"];
        public readonly JToken[] Boolies;


        public readonly JToken Timespan1 = JToken.Parse("{ \"Value\": \"1118700000\" }")["Value"];
        public readonly JToken Timespan2 = JToken.Parse("{ \"Value\": \"1118700001\" }")["Value"];
        public readonly JToken[] Timespans;

        public readonly JToken DateYearMonth = JToken.Parse("{ \"Value\": \"2018-10\" }")["Value"];
        public readonly JToken DateYear = JToken.Parse("{ \"Value\": \"2018\" }")["Value"];
        public readonly JToken[] DatesOnly;

        // I suppose I should have done times-only as well. Meh.

        // various formats of the same date observed as valid (in Chrome at least)
        public readonly JToken DateShort = JToken.Parse("{ \"Value\": \"20/Nov/2013 19:15:00\" }")["Value"];
        public readonly JToken DateLocale = JToken.Parse("{ \"Value\": \"11/20/2013, 7:15:00 PM\" }")["Value"];
        public readonly JToken DateJavascript = JToken.Parse("{ \"Value\": \"Wed Nov 20 2013 19:15:00 GMT-0500 (Eastern Standard Time)\" }")["Value"];
        public readonly JToken DateGMT = JToken.Parse("{ \"Value\": \"Thu, 21 Nov 2013 00:15:00 GMT\" }")["Value"];
        public readonly JToken DateJSONAndISOZulu = JToken.Parse("{ \"Value\": \"2013-11-21T00:15:00.000Z\" }")["Value"];
        public readonly JToken DateISORelative = JToken.Parse("{ \"Value\": \"2013-11-21T00:15:00.000-05:00\" }")["Value"];
        public readonly JToken[] SameDates;

        public readonly JToken[][] Errybody;
        public TestData()
        {
            SameDates = new[] { DateShort, DateLocale, DateJavascript, DateGMT, DateJSONAndISOZulu, DateISORelative };
            DatesOnly = new[] { DateYearMonth, DateYear };
            Timespans = new[] { Timespan1, Timespan2 };
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
                NegativeZero,
                PositiveZero,
                NaN
            };
            Nopes = new[]
            {
                Null, Undefined
            };
            Errybody = new[] { Nopes, Numbers, Strings, Boolies, Timespans, DatesOnly, SameDates };
        }
    }
    /// <summary>
    /// Here's where we test the sandbox for needles and broken glass
    /// </summary>
    [TestFixture]
    public class EqualityTests
    {
        #region halpers        
        // used by asserters to perform the comparison
        public delegate bool Comparator(JToken lhs, JToken rhs);
        readonly Comparator strictEquality = (lhs, rhs) => lhs.StrictEquality(rhs);
        readonly Comparator abstractEquality = (lhs, rhs) => lhs.AbstractEquality(rhs);
        // a bunch of convenience methods for the test belwo
        // these make sure the comparator returns false for all do not wants
        public void AssertNone(Comparator comparator, JToken token, params JToken[][] doNotWant)
        {
            AssertNone(comparator, token, doNotWant.SelectMany(x => x));
        }
        public void AssertNone(Comparator comparator, JToken token, params JToken[] doNotWant)
        {
            AssertNone(comparator, token, (IEnumerable<JToken>)doNotWant);
        }
        public void AssertNone(Comparator comparator, JToken token, IEnumerable<JToken> doNotWant)
        {
            Assert.IsTrue(doNotWant.All(x => !token.StrictEquality(x)));
        }
        // these make sure the comparator returns true for all do not wants
        public void AssertAll(Comparator comparator, JToken token, params JToken[][] want)
        {
            AssertNone(comparator, token, want.SelectMany(x => x));
        }
        public void AssertAll(Comparator comparator, JToken token, params JToken[] want)
        {
            AssertNone(comparator, token, (IEnumerable<JToken>)want);
        }
        public void AssertAll(Comparator comparator, JToken token, IEnumerable<JToken> want)
        {
            Assert.IsTrue(want.All(x => comparator(token, x)));
        }
        #endregion
        
        [Test]
        public void AllStrictEqualityTests()
        {
            // this is a bit cargo-culty; making absolutely sure no false positives caused by instance equivalence
            var lhs = new TestData();
            var rhs = new TestData();

            // For all tests, if Type(x) is different from Type(y), return false.

            // given x === y, if Type(x) is Null, return true (see "for all tests" note above)
            var target = lhs.Null;
            AssertAll(strictEquality, target, rhs.Null);
            AssertNone(strictEquality, target, rhs.Errybody.SelectMany(x => x).Except(new[] { rhs.Null }));

            // given x === y, if Type(x) is Undefined, return true (see "for all tests" note above)
            target = lhs.Undefined;
            AssertAll(strictEquality, target, rhs.Undefined);
            AssertNone(strictEquality, target, rhs.Errybody.SelectMany(x => x).Except(new[] { rhs.Undefined }));

            // given x === y, if x is NaN, return false.
            target = lhs.NaN;
            AssertNone(strictEquality, target, rhs.Errybody);

            // given x === y, if y is NaN, return false.
            target = rhs.NaN;
            Assert.IsTrue(lhs.Errybody.SelectMany(x => x).All(x => !x.StrictEquality(target)));

            // given x === y, if x is the same Number value as y, return true.
            target = lhs.One;
            AssertAll(strictEquality, target, rhs.One, rhs.OneDotZero);

            // given x === y, if x is +0 and y is −0, return true.
            target = lhs.PositiveZero;
            AssertAll(strictEquality, target, rhs.NegativeZero, rhs.PositiveZero);

            // given x === y, if x is −0 and y is +0, return true.
            target = lhs.NegativeZero;
            AssertAll(strictEquality, target, rhs.NegativeZero, rhs.PositiveZero);

            // given x === y, if Type(x) is String, then return true if x and y are exactly the same sequence of characters (same length and same characters in corresponding positions); otherwise, return false.
            target = lhs.DerpString;
            AssertNone(strictEquality, target, rhs.HerpString);
            AssertAll(strictEquality, target, rhs.DerpString);

            // given x === y, if Type(x) is Boolean, return true if x and y are both true or both false; otherwise, return false.
            target = lhs.True;
            AssertAll(strictEquality, target, rhs.True);
            AssertNone(strictEquality, target, new[] { rhs.False }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Timespans, rhs.DatesOnly, rhs.SameDates);
            target = lhs.False;
            AssertAll(strictEquality, target, rhs.False);
            AssertNone(strictEquality, target, new[] { rhs.True }, rhs.Nopes, rhs.Numbers, rhs.Strings, rhs.Timespans, rhs.DatesOnly, rhs.SameDates);

            // Bring out the special cases!
            // timespans
            target = lhs.Timespan1;
            AssertAll(strictEquality, target, rhs.Timespan1);
            AssertNone(strictEquality, target, rhs.Timespan2);

            //DatesOnly was this worth it? Meh. 
            target = lhs.DateYearMonth;
            AssertAll(strictEquality, target, rhs.DateYearMonth);
            AssertNone(strictEquality, target, rhs.DateYear);
            target = lhs.DateYear;
            AssertNone(strictEquality, target, rhs.DateYearMonth);
            AssertAll(strictEquality, target, rhs.DateYear);

            //samedates
            var errbodyElse = rhs.Errybody.SelectMany(x => x).Except(rhs.SameDates);
            target = lhs.DateShort;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
            target = lhs.DateLocale;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
            target = lhs.DateJavascript;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
            target = lhs.DateGMT;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
            target = lhs.DateJSONAndISOZulu;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
            target = lhs.DateISORelative;
            AssertAll(strictEquality, target, rhs.SameDates);
            AssertNone(strictEquality, target, errbodyElse);
        }

        [Test]
        public void AllAbstractEqualityTests()
        {
            /* THE ALGORITHM
             * 
             * If Type(x) is the same as Type(y), then 
             *     If Type(x) is Undefined, return true.
             *     If Type(x) is Null, return true.
             *     If Type(x) is Number, then 
             *         If x is NaN, return false.
             *         If y is NaN, return false.
             *         If x is the same Number value as y, return true.
             *         If x is +0 and y is −0, return true.
             *         If x is −0 and y is +0, return true.
             *         Return false.
             *     If Type(x) is String, then return true if x and y are exactly the same sequence of characters (same length and same characters in corresponding positions). Otherwise, return false.
             *     If Type(x) is Boolean, return true if x and y are both true or both false. Otherwise, return false.
             *     Return true if x and y refer to the same object. Otherwise, return false.
             * If x is null and y is undefined, return true.
             * If x is undefined and y is null, return true.
             * If Type(x) is Number and Type(y) is String,
             * return the result of the comparison x == ToNumber(y).
             * If Type(x) is String and Type(y) is Number,
             * return the result of the comparison ToNumber(x) == y.
             * If Type(x) is Boolean, return the result of the comparison ToNumber(x) == y.
             * If Type(y) is Boolean, return the result of the comparison x == ToNumber(y).
             * If Type(x) is either String or Number and Type(y) is Object,
             * return the result of the comparison x == ToPrimitive(y).
             * If Type(x) is Object and Type(y) is either String or Number,
             * return the result of the comparison ToPrimitive(x) == y.
             * Return false.
             * 
             * NOTE 1
             * Given the above definition of equality:
             * String comparison can be forced by: "" + a == "" + b.
             * Numeric comparison can be forced by: +a == +b.
             * Boolean comparison can be forced by: !a == !b.
             * 
             * NOTE 2
             * The equality operators maintain the following invariants:
             * A != B is equivalent to !(A == B).
             * A == B is equivalent to B == A, except in the order of evaluation of A and B.
             * 
             * NOTE 3
             * The equality operator is not always transitive. For example, there might be two distinct String objects, each representing the same String value; each String object would be considered equal to the String value by the == operator, but the two String objects would not be equal to each other. For Example:
             * new String("a") == "a" and "a" == new String("a")are both true.
             * new String("a") == new String("a") is false.
             * 
             * NOTE 4
             * Comparison of Strings uses a simple equality test on sequences of code unit values. There is no attempt to use the more complex, semantically oriented definitions of character or string equality and collating order defined in the Unicode specification. Therefore Strings values that are canonically equal according to the Unicode standard could test as unequal. In effect this algorithm assumes that both Strings are already in normalised form.\
             */
            Assert.Warn("JTokenExtensions.AbstractEquality needs to be implemented then asserts added to test the algorithm's expectations");
        }
    }

}
