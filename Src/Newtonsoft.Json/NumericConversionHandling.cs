namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies quotes handling on numeric converion
    /// <example>"2"</example>whether the deserializer attempts to convert basic types (e.g. the string "2" to an integer field) <see cref="JsonSerializer"/>.
    /// </summary>    
    public enum NumericConversionHandling
    {
        /// <summary>
        /// Allows numeric conversion when quotes are encountered.
        /// </summary>
        AllowQuotes = 0,
        /// <summary>
        /// Throw a <see cref="JsonSerializationException"/> when a quotes are encountered.
        /// </summary>
        ProhibitQuotes = 1
    }
}