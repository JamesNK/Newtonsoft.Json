using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class ArraySliceFilter : PathFilter
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int? Step { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            if (Step == 0)
                throw new JsonException("Step cannot be zero.");

            // todo error when no match
            foreach (JToken t in current)
            {
                IList<JToken> sourceCollection = t as IList<JToken> ?? new List<JToken>(t);

                // set defaults for null arguments
                int stepCount = Step ?? 1;
                int startIndex = Start ?? ((stepCount > 0) ? 0 : sourceCollection.Count - 1);
                int stopIndex = End ?? ((stepCount > 0) ? sourceCollection.Count : -1);

                // start from the end of the list if start is negitive
                if (Start < 0) startIndex = sourceCollection.Count + startIndex;

                // end from the start of the list if stop is negitive
                if (End < 0) stopIndex = sourceCollection.Count + stopIndex;

                // ensure indexes keep within collection bounds
                startIndex = Math.Max(startIndex, (stepCount > 0) ? 0 : int.MinValue);
                startIndex = Math.Min(startIndex, (stepCount > 0) ? sourceCollection.Count : sourceCollection.Count - 1);
                stopIndex = Math.Max(stopIndex, -1);
                stopIndex = Math.Min(stopIndex, sourceCollection.Count);

                for (int i = startIndex; (stepCount > 0) ? i < stopIndex : i > stopIndex; i += stepCount)
                {
                    yield return sourceCollection[i];
                }
            }
        }
    }
}