using System;
using System.Collections.Generic;

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
                            return false;
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(t))
                            return true;
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
                        if (v != null && v.Equals(Value))
                            return true;
                        break;
                    case QueryOperator.NotEquals:
                        if (v != null && !v.Equals(Value))
                            return true;
                        break;
                    case QueryOperator.GreaterThan:
                        if (v != null && v.CompareTo(Value) > 0)
                            return true;
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (v != null && v.CompareTo(Value) >= 0)
                            return true;
                        break;
                    case QueryOperator.LessThan:
                        if (v != null && v.CompareTo(Value) < 0)
                            return true;
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (v != null && v.CompareTo(Value) <= 0)
                            return true;
                        break;
                    case QueryOperator.Exists:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return false;
        }
    }
}