using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when selecting JSON.
    /// </summary>
    public class JsonCloneSettings
    {
        /// <summary>
        /// Gets or sets a flag that indicates whether to copy annotations when cloning a JToken.
        /// </summary>
        /// <value>
        /// A flag that indicates whether to copy annotations when cloning a JToken.
        /// </value>
        public bool CopyAnnotations { get; set; }
    }
}