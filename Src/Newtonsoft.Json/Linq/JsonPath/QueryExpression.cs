using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal enum QueryOperator
    {
        None = 0,
        Equals = 1,
        NotEquals = 2,
        Exists = 3,
        LessThan = 4,
        LessThanOrEquals = 5,
        GreaterThan = 6,
        GreaterThanOrEquals = 7,
        And = 8,
        Or = 9,
        RegexEquals = 10,
        StrictEquals = 11,
        StrictNotEquals = 12
    }

    internal abstract class QueryExpression
    {
        public QueryOperator Operator { get; set; }

        public abstract bool IsMatch(JToken root, JToken t);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression()
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(JToken root, JToken t)
        {
            switch (Operator)
            {
                case QueryOperator.And:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (!e.IsMatch(root, t))
                        {
                            return false;
                        }
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(root, t))
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal class BooleanQueryExpression : QueryExpression
    {
        public object Left { get; set; }
        public object Right { get; set; }

        private IEnumerable<JToken> GetResult(JToken root, JToken t, object o)
        {
            if (o is JToken resultToken)
            {
                return new[] { resultToken };
            }

            if (o is List<PathFilter> pathFilters)
            {
                return JPath.Evaluate(pathFilters, root, t, false);
            }

            return CollectionUtils.ArrayEmpty<JToken>();
        }

        public override bool IsMatch(JToken root, JToken t)
        {
            if (Operator == QueryOperator.Exists)
            {
                return GetResult(root, t, Left).Any();
            }

            using (IEnumerator<JToken> leftResults = GetResult(root, t, Left).GetEnumerator())
            {
                if (leftResults.MoveNext())
                {
                    IEnumerable<JToken> rightResultsEn = GetResult(root, t, Right);
                    ICollection<JToken> rightResults = rightResultsEn as ICollection<JToken> ?? rightResultsEn.ToList();

                    do
                    {
                        JToken leftResult = leftResults.Current;
                        foreach (JToken rightResult in rightResults)
                        {
                            if (MatchTokens(leftResult, rightResult))
                            {
                                return true;
                            }
                        }
                    } while (leftResults.MoveNext());
                }
            }

            return false;
        }
        
        private bool MatchTokens(JToken leftResult, JToken rightResult)
        {
            if (leftResult is JValue leftValue && rightResult is JValue rightValue)
            {
                switch (Operator)
                {
                    case QueryOperator.RegexEquals:
                        if (RegexEquals(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Equals:
                        if (EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictEquals:
                        if (EqualsWithoutStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictNotEquals:
                        if (!EqualsWithoutStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThan:
                        if (leftValue.CompareTo(rightValue) > 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (leftValue.CompareTo(rightValue) >= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThan:
                        if (leftValue.CompareTo(rightValue) < 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (leftValue.CompareTo(rightValue) <= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Exists:
                        return true;
                }
            }
            else
            {
                switch (Operator)
                {
                    case QueryOperator.Exists:
                    // you can only specify primitive types in a comparison
                    // notequals will always be true
                    case QueryOperator.NotEquals:
                        return true;
                }
            }

            return false;
        }

        private static bool RegexEquals(JValue input, JValue pattern)
        {
            if (input.Type != JTokenType.String || pattern.Type != JTokenType.String)
            {
                return false;
            }

            string regexText = (string)pattern.Value;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            return Regex.IsMatch((string)input.Value, patternText, MiscellaneousUtils.GetRegexOptions(optionsText));
        }

        private bool EqualsWithStringCoercion(JValue value, JValue queryValue)
        {
            if (value.Equals(queryValue))
            {
                return true;
            }

            if (queryValue.Type != JTokenType.String)
            {
                return false;
            }

            string queryValueString = (string)queryValue.Value;

            string currentValueString;

            // potential performance issue with converting every value to string?
            switch (value.Type)
            {
                case JTokenType.Date:
                    using (StringWriter writer = StringUtils.CreateStringWriter(64))
                    {
#if HAVE_DATE_TIME_OFFSET
                        if (value.Value is DateTimeOffset offset)
                        {
                            DateTimeUtils.WriteDateTimeOffsetString(writer, offset, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                        }
                        else
#endif
                        {
                            DateTimeUtils.WriteDateTimeString(writer, (DateTime)value.Value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                        }

                        currentValueString = writer.ToString();
                    }
                    break;
                case JTokenType.Bytes:
                    currentValueString = Convert.ToBase64String((byte[])value.Value);
                    break;
                case JTokenType.Guid:
                case JTokenType.TimeSpan:
                    currentValueString = value.Value.ToString();
                    break;
                case JTokenType.Uri:
                    currentValueString = ((Uri)value.Value).OriginalString;
                    break;
                default:
                    return false;
            }

            return string.Equals(currentValueString, queryValueString, StringComparison.Ordinal);
        }

        private bool EqualsWithoutStringCoercion(JValue value, JValue queryValue)
        {
            return IsStrictMatch(value, queryValue);
        }
        internal static bool IsStrictMatch(JToken value, JToken other)
        {
            // I've made this internal and static for testing purposes because I really don't get how to call the IsMatch method :/
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
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // we handle floats and integers the exact same way, so they are pseudo equivalent
            if (value.Type != other.Type && 
                ((value.Type != JTokenType.Integer && value.Type != JTokenType.Float) || 
                (other.Type != JTokenType.Integer && other.Type != JTokenType.Float))) return false;

            switch (value.Type)
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
}