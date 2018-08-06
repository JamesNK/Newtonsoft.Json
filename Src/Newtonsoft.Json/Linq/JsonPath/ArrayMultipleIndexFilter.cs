using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters an array by multiple indexes
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class ArrayMultipleIndexFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the indexes to filter by.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        public List<int> Indexes { get; set; }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                foreach (int i in Indexes)
                {
                    JToken v = GetTokenIndex(t, errorWhenNoMatch, i);

                    if (v != null)
                    {
                        yield return v;
                    }
                }
            }
        }
    }
}