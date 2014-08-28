namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies how JSON arrays are merged together.
    /// </summary>
    public enum MergeArrayHandling
    {
        /// <summary>Concatenate arrays.</summary>
        Concat,

        /// <summary>Union arrays, skipping items that already exist.</summary>
        Union,

        /// <summary>Replace all array items.</summary>
        Replace,

        /// <summary>Merge array items together, matched by index.</summary>
        Merge
    }
}