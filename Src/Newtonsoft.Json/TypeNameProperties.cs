namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies type name handling options for the <see cref="JsonSerializer"/>.
    /// Specifies how type name metadata is serialized to json.
    /// Recommended to use with <see cref="Json.TypeNameHandling"/>.
    /// </summary>
    public enum TypeNameProperties
    {
        /// <summary>
        /// The .NET type name metadata is stored in fields prepended with '$'.
        /// For example: "$type"
        /// </summary>
        Default = 0,
        /// <summary>
        /// The .NET type name metadata is stored in fields prepended with '_$'.
        /// This must be enabled in order to store .NET type name information in MongoDB.
        /// For example: "_$type"
        /// </summary>
        Mongo = 1
    }
}