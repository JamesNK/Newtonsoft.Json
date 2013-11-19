using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class AndFilter : PathFilter
    {
        public List<PathFilter> Filters { get; set; } 

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (PathFilter f in Filters)
            {
                current = f.ExecuteFilter(current, errorWhenNoMatch).ToList();
            }

            return current;
        }
    }
}