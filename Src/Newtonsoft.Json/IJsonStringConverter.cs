using System;

namespace Newtonsoft.Json
{
    public interface IJsonStringConverter
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        object ConvertFromString(string value, Type objectType);

        string ConvertToString(object value);
    }
}
