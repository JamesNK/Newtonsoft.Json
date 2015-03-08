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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// A collection of <see cref="JsonProperty"/> objects.
    /// </summary>
    public class JsonPropertyCollection : IEnumerable<JsonProperty>
    {
        private readonly Type _type;
        private readonly Dictionary<string, JsonProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyCollection"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public JsonPropertyCollection(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, "type");
            _type = type;
            _properties = new Dictionary<string, JsonProperty>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Adds a <see cref="JsonProperty"/> object.
        /// </summary>
        /// <param name="property">The property to add to the collection.</param>
        public void AddProperty(JsonProperty property)
        {
            if (_properties.ContainsKey(property.PropertyName))
            {
                // don't overwrite existing property with ignored property
                if (property.Ignored)
                    return;

                JsonProperty existingProperty = _properties[property.PropertyName];
                bool duplicateProperty = true;

                if (existingProperty.Ignored)
                {
                    // remove ignored property so it can be replaced in collection
                    _properties.Remove(existingProperty.PropertyName);
                    duplicateProperty = false;
                }
                else
                {
                    if (property.DeclaringType != null && existingProperty.DeclaringType != null)
                    {
                        if (property.DeclaringType.IsSubclassOf(existingProperty.DeclaringType))
                        {
                            // current property is on a derived class and hides the existing
                            _properties.Remove(existingProperty.PropertyName);
                            duplicateProperty = false;
                        }
                        if (existingProperty.DeclaringType.IsSubclassOf(property.DeclaringType))
                        {
                            // current property is hidden by the existing so don't add it
                            return;
                        }
                    }
                }

                if (duplicateProperty)
                    throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _type));
            }

            _properties[property.PropertyName] = property;
        }

        /// <summary>
        /// Gets the closest matching <see cref="JsonProperty"/> object.
        /// First attempts to get an exact case match of propertyName and then
        /// a case insensitive match.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>A matching property if found.</returns>
        public JsonProperty GetClosestMatchProperty(string propertyName)
        {
            JsonProperty property;
            if (_properties.TryGetValue(propertyName, out property))
                return property;

            // if we return something from down below, we may want to cache those in their own dictionary
            // we could put them in the main dictionary, but then we would have to use Distinct on the enumerator
            foreach (var kvp in _properties)
            {
                if (kvp.Value.HasAlias(propertyName))
                {
                    return kvp.Value;
                }
            }

            foreach (var kvp in _properties)
            {
                if (kvp.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        public IEnumerator<JsonProperty> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(IEnumerable<JsonProperty> properties)
        {
            foreach (var property in properties)
                AddProperty(property);
        }

        public int Count { get { return _properties.Count; } }
    }
}