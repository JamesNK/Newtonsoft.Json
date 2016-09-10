using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class RootFilter : PathFilter
    {
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            return root;
        }
    }
}