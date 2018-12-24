using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class QueryScanFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                if (t is JContainer c)
                {
                    foreach (JToken d in c.DescendantsAndSelf())
                    {
                        if (Expression.IsMatch(root, d))
                        {
                            yield return d;
                        }
                    }
                }
                else
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    if (Expression.IsMatch(root, t))
#pragma warning restore CS8604 // Possible null reference argument.
                    {
                        yield return t;
                    }
                }
            }
        }
    }
}