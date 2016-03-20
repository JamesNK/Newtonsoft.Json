namespace Newtonsoft.Json.Serialization
{
    public class DefaultNamingStrategy : INamingStrategy
    {
        public string GetPropertyName(string s, bool hasSpecifiedName)
        {
            return s;
        }

        public string GetDictionaryKey(string s)
        {
            return s;
        }
    }
}