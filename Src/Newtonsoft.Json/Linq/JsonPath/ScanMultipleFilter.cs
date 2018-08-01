using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Scans a document and filters it by multiple property names
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class ScanMultipleFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the names.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public List<string> Names { get; set; }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken c in current)
            {
                JToken value = c;

                while (true)
                {
                    JContainer container = value as JContainer;

                    value = GetNextScanValue(c, container, value);
                    if (value == null)
                    {
                        break;
                    }

                    if (value is JProperty property)
                    {
                        foreach (string name in Names)
                        {
                            if (property.Name == name)
                            {
                                yield return property.Value;
                            }
                        }
                    }

                }
            }
        }
    }
}