using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when cloning JSON.
    /// </summary>
    public class JsonCloneSettings
    {
        internal static readonly JsonCloneSettings SkipCopyAnnotations = new JsonCloneSettings
        {
            CopyAnnotations = false
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCloneSettings"/> class.
        /// </summary>
        public JsonCloneSettings()
        {
            CopyAnnotations = true;
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether to copy annotations when cloning a <see cref="JToken"/>.
        /// The default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// A flag that indicates whether to copy annotations when cloning a <see cref="JToken"/>.
        /// </value>
        public bool CopyAnnotations { get; set; }
    }
}