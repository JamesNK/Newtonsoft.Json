namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// The default naming strategy. Property names and dictionary keys are unchanged.
    /// </summary>
    public class DefaultNamingStrategy : INamingStrategy
    {
        /// <summary>
        /// Gets the serialized name for a given property name.
        /// </summary>
        /// <param name="s">The initial property name.</param>
        /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specfied.</param>
        /// <returns>A property name.</returns>
        public string GetPropertyName(string s, bool hasSpecifiedName)
        {
            return s;
        }

        /// <summary>
        /// Gets the serialized key for a given dictionary key.
        /// </summary>
        /// <param name="s">The initial dictionary key.</param>
        /// <returns>A dictionary key.</returns>
        public string GetDictionaryKey(string s)
        {
            return s;
        }
    }
}