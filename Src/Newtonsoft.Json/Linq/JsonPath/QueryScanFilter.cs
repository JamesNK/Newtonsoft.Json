using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class QueryScanFilter : PathFilter
    {
        public QueryExpression Expression { get; set; }

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
                    if (Expression.IsMatch(root, t))
                    {
                        yield return t;
                    }
                }
            }
        }
    }
}