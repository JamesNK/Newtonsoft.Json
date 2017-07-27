#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Author: Max Brönnimann, 2017

#endregion

#if NET35 || NET40 || NET45

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// A JsonConverter class that handles contextual serialization based on PropertyPaths
    /// definitions per root type. <br/>
    ///
    /// This class is mostly helpful for serializing ViewModels
    ///
    /// TODO handle JsonPath wildcards ?
    /// </summary>
    public class ProjectionConverter : JsonConverter
    {
        /// <summary>
        /// TODO comment this
        /// </summary>
        public Dictionary<Type, HashSet<string>> PropertyPaths = new Dictionary<Type, HashSet<string>>();

        private Dictionary<string, IEnumerable<PropertyInfo>> _cachedProperties = new Dictionary<string, IEnumerable<PropertyInfo>>();

        // TODO handle regex options depending on the .NET version
        private static Regex ordinalIndexedPptyPathRemover = new Regex("\\[\\d+\\]", RegexOptions.Compiled);

        private object _root = null;
        private Type _rootType = null;

        private ReferenceLoopHandling baseReferenceLoopHandling;
        private HashSet<String> propertyPaths = null;

        /// <summary>
        /// Creates a new ProjectionConverter instance
        /// </summary>
        /// <param name="projections"></param>
        // TODO implement constructors from KeyValuePairs and replace the HashSet by a IEnumerable
        public ProjectionConverter(Dictionary<Type, HashSet<string>> projections = null)
            : base()
        {
            PropertyPaths = projections;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can read JSON; otherwise, <c>false</c>.</value>
        public override bool CanRead { get { return false; } }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(string)) return false;
            if (objectType.HasElementType || (objectType.IsGenericType && objectType.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable))))) return false;
            return (PropertyPaths.Keys.Any(k => objectType.IsAssignableFrom(k)) || (!objectType.IsPrimitive && !objectType.IsValueType));
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter is usable for serialization only yet");
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;
            if (String.IsNullOrEmpty(writer.Path) && _root == null)
            {
                // Safekeeps the current serializer.ReferenceLoopHandling value;
                baseReferenceLoopHandling = serializer.ReferenceLoopHandling;

                _root = value;
                _rootType = value.GetType();
                // NOTE ? is it valuable to handle inheritance and interfaces ?
                while (propertyPaths == null)
                {
                    if (!PropertyPaths.TryGetValue(value.GetType(), out propertyPaths))
                    {
                        _rootType = _rootType.BaseType;
                        if (_rootType == null)
                        {
                            // TODO ? revert to basic native serialization based on the current
                            // serializerSettings or throw an exception ?
                            serializer.Serialize(writer, value);
                            return;
                            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "No projection found for type {0}", _rootType));
                        }
                    }
                }
            }

            String currentCorrectedPath = ordinalIndexedPptyPathRemover.Replace(writer.Path, "");
            writer.WriteStartObject();
            // TODO handle the PreserveReferencesHandling setting

            IEnumerable<PropertyInfo> indexedProperties;
            var cacheKey = _rootType + "#" + currentCorrectedPath;
            if (!_cachedProperties.TryGetValue(cacheKey, out indexedProperties))
            {
                indexedProperties = value.GetType().GetProperties().Where(ppty =>
                {
                    // TODO handle EntityFramework lazy-loading if necessary
                    var pptyPath = ((String.IsNullOrEmpty(currentCorrectedPath) ? "" : currentCorrectedPath + ".") + ppty.Name);
                    return propertyPaths.Any(p => p == pptyPath || p.StartsWith(pptyPath + ".", StringComparison.CurrentCulture));
                }).ToList();
                _cachedProperties[cacheKey] = indexedProperties;
            }
            foreach (var ppty in indexedProperties)
            {
                var pptyValue = ppty.GetValue(value, null);
                if (pptyValue == null) continue;
                try
                {
                    writer.WritePropertyName(ppty.Name);
                    // If any projection is defined it is finite, so we can safely serialize the
                    // property value until JsonPath wildcards are supported
                    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                    serializer.Serialize(writer, pptyValue);
                    serializer.ReferenceLoopHandling = baseReferenceLoopHandling;
                }
                catch (Exception any)
                {
                    serializer.ReferenceLoopHandling = baseReferenceLoopHandling;
                    Reset();
                    throw any;
                }
            }
            writer.WriteEndObject();
            if (String.IsNullOrEmpty(writer.Path)) Reset();
        }

        private void Reset()
        {
            _root = null;
        }
    }
}

#endif