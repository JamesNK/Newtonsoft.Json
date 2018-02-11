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
        RegexEquals = 10
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
                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftValue, rightValue))
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
    }
}