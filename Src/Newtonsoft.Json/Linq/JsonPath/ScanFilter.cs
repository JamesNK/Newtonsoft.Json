using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Scans a document and filters it by a property name
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class ScanFilter : PathFilter
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken c in current)
            {
                if (Name == null)
                {
                    yield return c;
                }

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
                        if (property.Name == Name)
                        {
                            yield return property.Value;
                        }
                    }
                    else
                    {
                        if (Name == null)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }
    }
}