using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    public class ScanMultipleFilter : PathFilter
    {
        public List<string> names { get; set; }

        public ScanMultipleFilter(List<string> names)
        {
            this.names = names;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken c in current)
            {
                JToken? value = c;

                while (true)
                {
                    JContainer? container = value as JContainer;

                    value = GetNextScanValue(c, container, value);
                    if (value == null)
                    {
                        break;
                    }

                    if (value is JProperty property)
                    {
                        foreach (string name in names)
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