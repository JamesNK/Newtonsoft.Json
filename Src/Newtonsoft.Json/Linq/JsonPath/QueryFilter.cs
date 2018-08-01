using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters a document by a query expression
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class QueryFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the query expression.
        /// </summary>
        /// <value>
        /// The query expression.
        /// </value>
        public QueryExpression Expression { get; set; }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                foreach (JToken v in t)
                {
                    if (Expression.IsMatch(root, v))
                    {
                        yield return v;
                    }
                }
            }
        }
    }
}