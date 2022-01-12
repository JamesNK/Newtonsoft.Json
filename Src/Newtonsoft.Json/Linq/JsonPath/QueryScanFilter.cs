using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    public class QueryScanFilter : PathFilter
    {
        public QueryExpression Expression { get; set; }

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken t in current)
            {
                if (t is JContainer c)
                {
                    foreach (JToken d in c.DescendantsAndSelf())
                    {
                        if (Expression.IsMatch(root, d, settings))
                        {
                            yield return d;
                        }
                    }
                }
                else
                {
                    if (Expression.IsMatch(root, t, settings))
                    {
                        yield return t;
                    }
                }
            }
        }
    }
}