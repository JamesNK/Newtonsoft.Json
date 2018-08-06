using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters a document by an expression and traverses all descendant containers
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class QueryScanFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the filter expression.
        /// </summary>
        /// <value>
        /// The filter expression.
        /// </value>
        public QueryExpression Expression { get; set; }

        /// <inheritdoc />
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