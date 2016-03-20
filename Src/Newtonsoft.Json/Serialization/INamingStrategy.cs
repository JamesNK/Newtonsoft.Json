namespace Newtonsoft.Json.Serialization
{
    public interface INamingStrategy
    {
        string GetPropertyName(string s, bool hasSpecifiedName);
        string GetDictionaryKey(string s);
    }
}