namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies whether to ignore empty arrays
    /// </summary>
    public enum EmptyArrayHandling
    {
        /// <summary>
        /// Ignore empty array when deserializing (use object current array).
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// Set empty array (replace object's current array with an empty one)
        /// </summary>
        Set = 1
    }
}