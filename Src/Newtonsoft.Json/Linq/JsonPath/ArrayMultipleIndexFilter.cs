using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    public class ArrayMultipleIndexFilter : PathFilter
    {
        public List<int> Indexes { get; set; }

        public ArrayMultipleIndexFilter(List<int> indexes)
        {
            Indexes = indexes;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken t in current)
            {
                foreach (int i in Indexes)
                {
                    JToken? v = GetTokenIndex(t, settings, i);

                    if (v != null)
                    {
                        yield return v;
                    }
                }
            }
        }
    }
}