using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        Or = 9
    }

    internal abstract class QueryExpression
    {
        public QueryOperator Operator { get; set; }

        public abstract bool IsMatch(JToken t);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression()
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(JToken t)
        {
            switch (Operator)
            {
                case QueryOperator.And:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (!e.IsMatch(t))
                        {
                            return false;
                        }
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(t))
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
        public List<PathFilter> Path { get; set; }
        public JValue Value { get; set; }

        public override bool IsMatch(JToken t)
        {
            IEnumerable<JToken> pathResult = JPath.Evaluate(Path, t, false);

            foreach (JToken r in pathResult)
            {
                JValue v = r as JValue;
                switch (Operator)
                {
                    case QueryOperator.Equals:
                        if (v != null && EqualsWithStringCoercion(v, Value))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.NotEquals:
                        if (v != null && !EqualsWithStringCoercion(v, Value))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThan:
                        if (v != null && v.CompareTo(Value) > 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (v != null && v.CompareTo(Value) >= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThan:
                        if (v != null && v.CompareTo(Value) < 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (v != null && v.CompareTo(Value) <= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Exists:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return false;
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
#if !NET20
                        if (value.Value is DateTimeOffset)
                        {
                            DateTimeUtils.WriteDateTimeOffsetString(writer, (DateTimeOffset)value.Value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
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