using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters a document by a field
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class FieldFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the name of the filtering property.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                if (t is JObject o)
                {
                    if (Name != null)
                    {
                        JToken v = o[Name];

                        if (v != null)
                        {
                            yield return v;
                        }
                        else if (errorWhenNoMatch)
                        {
                            throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, Name));
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, JToken> p in o)
                        {
                            yield return p.Value;
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, Name ?? "*", t.GetType().Name));
                    }
                }
            }
        }
    }
}