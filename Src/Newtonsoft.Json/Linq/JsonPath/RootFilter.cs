using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath
{
    /// <summary>
    /// Filters a document by it's root
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Linq.JsonPath.PathFilter" />
    public class RootFilter : PathFilter
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public static readonly RootFilter Instance = new RootFilter();

        private RootFilter()
        {
        }

        /// <inheritdoc />
        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            return new[] { root };
        }
    }
}