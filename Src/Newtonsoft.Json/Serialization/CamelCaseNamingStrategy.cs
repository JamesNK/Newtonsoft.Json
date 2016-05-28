using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// A camel case naming strategy.
    /// </summary>
    public class CamelCaseNamingStrategy : INamingStrategy
    {
        /// <summary>
        /// A flag indicating whether dictionary keys should be camel cased.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool CamelCaseDictionaryKeys { get; set; }

        /// <summary>
        /// A flag indicating whether explicitly specified property names,
        /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>, should be camel cased.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool OverrideSpecifiedNames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseNamingStrategy"/> class.
        /// </summary>
        /// <param name="camelCaseDictionaryKeys">
        /// A flag indicating whether dictionary keys should be camel cased.
        /// </param>
        /// <param name="overrideSpecifiedNames">
        /// A flag indicating whether explicitly specified property names,
        /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>, should be camel cased.
        /// </param>
        public CamelCaseNamingStrategy(bool camelCaseDictionaryKeys, bool overrideSpecifiedNames)
        {
            CamelCaseDictionaryKeys = camelCaseDictionaryKeys;
            OverrideSpecifiedNames = overrideSpecifiedNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseNamingStrategy"/> class.
        /// </summary>
        public CamelCaseNamingStrategy()
        {
        }

        /// <summary>
        /// Gets the serialized name for a given property name.
        /// </summary>
        /// <param name="s">The initial property name.</param>
        /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specfied.</param>
        /// <returns>A property name.</returns>
        public string GetPropertyName(string s, bool hasSpecifiedName)
        {
            if (hasSpecifiedName && !OverrideSpecifiedNames)
            {
                return s;
            }

            return StringUtils.ToCamelCase(s);
        }

        /// <summary>
        /// Gets the serialized key for a given dictionary key.
        /// </summary>
        /// <param name="s">The initial dictionary key.</param>
        /// <returns>A dictionary key.</returns>
        public string GetDictionaryKey(string s)
        {
            if (!CamelCaseDictionaryKeys)
            {
                return s;
            }

            return StringUtils.ToCamelCase(s);
        }
    }
}