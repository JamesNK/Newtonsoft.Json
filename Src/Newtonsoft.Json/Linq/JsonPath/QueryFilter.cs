using System;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal enum QueryOperator
    {
        None,
        Equals,
        NotEquals,
        Exists,
        LessThan,
        LessThanOrEquals,
        GreaterThan,
        GreaterThanOrEquals
    }

    internal class QueryExpression
    {
        public List<PathFilter> Path { get; set; }
        public QueryOperator Operator { get; set; }
        public JValue Value { get; set; }
    }

    internal class QueryFilter : PathFilter
    {
        public List<QueryExpression> Expression { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                foreach (JToken v in t)
                {
                    if (Evaluate(errorWhenNoMatch, v))
                        yield return v;
                }
            }
        }

        private bool Evaluate(bool errorWhenNoMatch, JToken value)
        {
            foreach (QueryExpression expression in Expression)
            {
                IEnumerable<JToken> pathResult = JPath.Evaluate(expression.Path, value, errorWhenNoMatch);

                foreach (JToken r in pathResult)
                {
                    JValue v = r as JValue;
                    switch (expression.Operator)
                    {
                        case QueryOperator.Equals:
                            if (v != null && v.Equals(expression.Value))
                                return true;
                            break;
                        case QueryOperator.NotEquals:
                            if (v != null && !v.Equals(expression.Value))
                                return true;
                            break;
                        case QueryOperator.GreaterThan:
                            if (v != null && v.CompareTo(expression.Value) > 0)
                                return true;
                            break;
                        case QueryOperator.GreaterThanOrEquals:
                            if (v != null && v.CompareTo(expression.Value) >= 0)
                                return true;
                            break;
                        case QueryOperator.LessThan:
                            if (v != null && v.CompareTo(expression.Value) < 0)
                                return true;
                            break;
                        case QueryOperator.LessThanOrEquals:
                            if (v != null && v.CompareTo(expression.Value) <= 0)
                                return true;
                            break;
                        case QueryOperator.Exists:
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return false;
        }
    }
}