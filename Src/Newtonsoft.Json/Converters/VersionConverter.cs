#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="Version"/> to and from a string (e.g. "1.2.3.4").
    /// </summary>
    public class VersionConverter : JsonStringConverter
    {
        /// <summary>
        /// Converts the object to its string representation. Used when serializing dictionary keys.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public override string ConvertToString(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is Version)
            {
                return value.ToString();
            }
            throw new JsonSerializationException("Expected Version object value");
        }

        /// <summary>
        /// Converts the string representation of an object to that object. Used when deserializing dictionary keys.
        /// </summary>
        /// <param name="value">The object's string representation.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object represented by its string representation.</returns>
        public override object ConvertFromString(string value, Type objectType)
        {
            if (value == null)
            {
                return null;
            }
            return new Version(value);
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Version);
        }
    }
}