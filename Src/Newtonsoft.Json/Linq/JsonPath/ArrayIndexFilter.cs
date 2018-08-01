using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters an array by index
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class ArrayIndexFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int? Index { get; set; }

        /// <inheritdoc />
        /// <exception cref="JsonException">Index * not valid on the given token type</exception>
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                if (Index != null)
                {
                    JToken v = GetTokenIndex(t, errorWhenNoMatch, Index.GetValueOrDefault());

                    if (v != null)
                    {
                        yield return v;
                    }
                }
                else
                {
                    if (t is JArray || t is JConstructor)
                    {
                        foreach (JToken v in t)
                        {
                            yield return v;
                        }
                    }
                    else
                    {
                        if (errorWhenNoMatch)
                        {
                            throw new JsonException("Index * not valid on {0}.".FormatWith(CultureInfo.InvariantCulture, t.GetType().Name));
                        }
                    }
                }
            }
        }
    }
}