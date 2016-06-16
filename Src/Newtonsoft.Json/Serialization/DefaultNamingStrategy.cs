namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// The default naming strategy. Property names and dictionary keys are unchanged.
    /// </summary>
    public class DefaultNamingStrategy : NamingStrategy
    {
        /// <summary>
        /// Resolves the specified property name.
        /// </summary>
        /// <param name="name">The property name to resolve.</param>
        /// <returns>The resolved property name.</returns>
        protected override string ResolvePropertyName(string name)
        {
            return name;
        }
    }
}