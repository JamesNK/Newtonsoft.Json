using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class FieldMultipleFilter : PathFilter
    {
        public List<string> Names { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                JObject o = t as JObject;
                if (o != null)
                {
                    foreach (string name in Names)
                    {
                        JToken v = o[name];

                        if (v != null)
                            yield return v;
                    }
                }
                //else
                //{
                //    if (errorWhenNoMatch)
                //        throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, Name ?? "*", t.GetType().Name));
                //}
            }
        }
    }
}