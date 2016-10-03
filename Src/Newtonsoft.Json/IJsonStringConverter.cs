using System;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Interface to enable configuration of dictionary key serialization and deserialization. Should be implemented by a <see cref="JsonConverter"/>. 
    /// </summary>
    public interface IJsonStringConverter
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IJsonStringConverter"/> can convert from string.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IJsonStringConverter"/> can convert to string.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Converts the string representation of an object to that object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        object ConvertFromString(string value, Type objectType);

        /// <summary>
        /// Converts the object to its string representation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string ConvertToString(object value);
    }
}
