using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    public class CamelCaseNamingStrategy : INamingStrategy
    {
        public bool CamelCaseDictionaryKeys { get; set; }
        public bool OverrideSpecifiedNames { get; set; }

        public CamelCaseNamingStrategy()
        {
        }

        public CamelCaseNamingStrategy(bool camelCaseDictionaryKeys, bool overrideSpecifiedNames)
        {
            CamelCaseDictionaryKeys = camelCaseDictionaryKeys;
            OverrideSpecifiedNames = overrideSpecifiedNames;
        }

        public string GetPropertyName(string s, bool hasSpecifiedName)
        {
            if (hasSpecifiedName && !OverrideSpecifiedNames)
            {
                return s;
            }

            return StringUtils.ToCamelCase(s);
        }

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