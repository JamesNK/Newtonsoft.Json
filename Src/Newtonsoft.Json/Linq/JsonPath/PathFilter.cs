using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Represents a path filter for json paths
    /// </summary>
    public abstract class PathFilter
    {
        /// <summary>
        /// Executes the filter.
        /// </summary>
        /// <param name="root">The root token.</param>
        /// <param name="current">The current tokens.</param>
        /// <param name="errorWhenNoMatch">if set to <c>true</c> an error will be thrown when the match fails.</param>
        /// <returns>A list of all matching tokens</returns>
        public abstract IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch);

        /// <summary>
        /// Gets the index of the token.
        /// </summary>
        /// <param name="t">The token.</param>
        /// <param name="errorWhenNoMatch">if set to <c>true</c> an error will be thrown when the match fails.</param>
        /// <param name="index">The index.</param>
        /// <returns>The found token</returns>
        /// <exception cref="JsonException">
        /// Index is outside the bounds of JArray
        /// or
        /// Index is outside the bounds of JConstructor
        /// or
        /// Index is not valid on the given token type
        /// </exception>
        protected static JToken GetTokenIndex(JToken t, bool errorWhenNoMatch, int index)
        {

            if (t is JArray a)
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
            else if (t is JConstructor c)
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

        /// <summary>
        /// Gets the next scan value.
        /// </summary>
        /// <param name="originalParent">The original parent.</param>
        /// <param name="container">The container.</param>
        /// <param name="value">The value.</param>
        /// <returns>The next token</returns>
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