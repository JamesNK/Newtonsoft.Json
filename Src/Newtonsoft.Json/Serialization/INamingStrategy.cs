namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// An interface for resolving how property names and dictionary keys are serialized.
    /// </summary>
    public interface INamingStrategy
    {
        /// <summary>
        /// Gets the serialized name for a given property name.
        /// </summary>
        /// <param name="s">The initial property name.</param>
        /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specfied.</param>
        /// <returns>A property name.</returns>
        string GetPropertyName(string s, bool hasSpecifiedName);

        /// <summary>
        /// Gets the serialized key for a given dictionary key.
        /// </summary>
        /// <param name="s">The initial dictionary key.</param>
        /// <returns>A dictionary key.</returns>
        string GetDictionaryKey(string s);
    }
}