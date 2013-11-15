using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class ScanFilter : PathFilter
    {
        public string Name { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken root in current)
            {
                if (Name == null)
                    yield return root;

                JToken value = root;
                JToken container = root;

                while (true)
                {
                    if (container != null)
                    {
                        value = container.First;
                    }
                    else
                    {
                        while (value != null && value != root && value == value.Parent.Last)
                        {
                            value = value.Parent;
                        }

                        if (value == null || value == root)
                            break;

                        value = value.Next;
                    }

                    JProperty e = value as JProperty;
                    if (e != null)
                    {
                        if (e.Name == Name)
                            yield return e.Value;
                    }
                    else
                    {
                        if (Name == null)
                            yield return value;
                    }

                    container = value as JContainer;
                }
            }
        }
    }
}