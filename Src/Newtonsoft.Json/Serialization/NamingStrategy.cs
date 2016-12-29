namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// A base class for resolving how property names and dictionary keys are serialized.
    /// </summary>
    public abstract class NamingStrategy
    {
        /// <summary>
        /// A flag indicating whether dictionary keys should be processed.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ProcessDictionaryKeys { get; set; }

        /// <summary>
        /// A flag indicating whether extension data names should be processed.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ProcessExtensionDataNames { get; set; }

        /// <summary>
        /// A flag indicating whether explicitly specified property names,
        /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>, should be processed.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool OverrideSpecifiedNames { get; set; }

        /// <summary>
        /// Gets the serialized name for a given property name.
        /// </summary>
        /// <param name="name">The initial property name.</param>
        /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specified.</param>
        /// <returns>The serialized property name.</returns>
        public virtual string GetPropertyName(string name, bool hasSpecifiedName)
        {
            if (hasSpecifiedName && !OverrideSpecifiedNames)
            {
                return name;
            }

            return ResolvePropertyName(name);
        }

        /// <summary>
        /// Gets the serialized name for a given extension data name.
        /// </summary>
        /// <param name="name">The initial extension data name.</param>
        /// <returns>The serialized extension data name.</returns>
        public virtual string GetExtensionDataName(string name)
        {
            if (!ProcessExtensionDataNames)
            {
                return name;
            }

            return ResolvePropertyName(name);
        }

        /// <summary>
        /// Gets the serialized key for a given dictionary key.
        /// </summary>
        /// <param name="key">The initial dictionary key.</param>
        /// <returns>The serialized dictionary key.</returns>
        public virtual string GetDictionaryKey(string key)
        {
            if (!ProcessDictionaryKeys)
            {
                return key;
            }

            return ResolvePropertyName(key);
        }

        /// <summary>
        /// Resolves the specified property name.
        /// </summary>
        /// <param name="name">The property name to resolve.</param>
        /// <returns>The resolved property name.</returns>
        protected abstract string ResolvePropertyName(string name);
    }
}