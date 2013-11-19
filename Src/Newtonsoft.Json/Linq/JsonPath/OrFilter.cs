using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class OrFilter : PathFilter
    {
        public List<PathFilter> Filters { get; set; } 

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            List<JToken> cachedCurrent = current.ToList();
            List<JToken> remaining = cachedCurrent.ToList();
            List<JToken> matches = new List<JToken>();
            int previousResultCount = 0;

            foreach (PathFilter f in Filters)
            {
                matches.AddRange(f.ExecuteFilter(remaining, errorWhenNoMatch));

                for (int i = previousResultCount; i < matches.Count; i++)
                {
                    remaining.Remove(matches[i]);
                }
                previousResultCount = matches.Count;
            }

            return cachedCurrent.Where(matches.Contains);
        }
    }
}