using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class QueryFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken t in current)
            {
                foreach (JToken v in t)
                {
                    if (Expression.IsMatch(root, v, settings))
                    {
                        yield return v;
                    }
                }
            }
        }
    }
}