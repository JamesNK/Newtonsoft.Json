using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        public List<string> Names { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken c in current)
            {
                JToken value = c;
                JContainer container = c as JContainer;

                while (true)
                {
                    value = GetNextScanValue(c, container, value);
                    if (value == null)
                    {
                        break;
                    }

                    JProperty e = value as JProperty;
                    if (e != null)
                    {
                        foreach (string name in Names)
                        {
                            if (e.Name == name)
                            {
                                yield return e.Value;
                            }
                        }
                    }

                    container = value as JContainer;
                }
            }
        }
    }
}