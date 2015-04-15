using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies how JSON objects are merged together.
    /// </summary>
    [Flags]
    public enum MergeObjectHandling
    {
        /// <summary>
        /// Uses default behaviour (ignore null and undefined) to merge the objects.
        /// </summary>
        None = 0,

        /// <summary>
        /// The target object's null value will be set to the source object.
        /// </summary>
        Null = 1,

        /// <summary>
        /// The target objects's explicit <code>undefined</code> value will remove the source object's property.
        /// </summary>
        Undefined = 2,

        /// <summary>
        /// Merge objects uses <see cref="Null"/> and <see cref="Undefined"/> policy.
        /// </summary>
        All = Null | Undefined,
    }
}
