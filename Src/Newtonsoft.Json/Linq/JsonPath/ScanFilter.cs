using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class ScanFilter : PathFilter
    {
        public string Name { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken c in current)
            {
                if (Name == null)
                {
                    yield return c;
                }

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
                        if (e.Name == Name)
                        {
                            yield return e.Value;
                        }
                    }
                    else
                    {
                        if (Name == null)
                        {
                            yield return value;
                        }
                    }

                    container = value as JContainer;
                }
            }
        }
    }
}