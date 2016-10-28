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
                JToken container = c;

                while (true)
                {
                    if (container != null && container.HasValues)
                    {
                        value = container.First;
                    }
                    else
                    {
                        while (value != null && value != c && value == value.Parent.Last)
                        {
                            value = value.Parent;
                        }

                        if (value == null || value == c)
                        {
                            break;
                        }

                        value = value.Next;
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