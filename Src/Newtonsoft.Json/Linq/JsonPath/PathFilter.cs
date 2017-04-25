using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal abstract class PathFilter
    {
        public abstract IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch);

        protected static JToken GetTokenIndex(JToken t, bool errorWhenNoMatch, int index)
        {
            JArray a = t as JArray;
            JConstructor c = t as JConstructor;

            if (a != null)
            {
                if (a.Count <= index)
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, index));
                    }

                    return null;
                }

                return a[index];
            }
            else if (c != null)
            {
                if (c.Count <= index)
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith(CultureInfo.InvariantCulture, index));
                    }

                    return null;
                }

                return c[index];
            }
            else
            {
                if (errorWhenNoMatch)
                {
                    throw new JsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, t.GetType().Name));
                }

                return null;
            }
        }

        protected static JToken GetNextScanValue(JToken originalParent, JToken container, JToken value)
        {
            // step into container's values
            if (container != null && container.HasValues)
            {
                value = container.First;
            }
            else
            {
                // finished container, move to parent
                while (value != null && value != originalParent && value == value.Parent.Last)
                {
                    value = value.Parent;
                }

                // finished
                if (value == null || value == originalParent)
                {
                    return null;
                }

                // move to next value in container
                value = value.Next;
            }

            return value;
        }
    }
}