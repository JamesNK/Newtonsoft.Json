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
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Reflection;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="KeyValuePair{TKey,TValue}"/> to and from JSON.
    /// </summary>
    public class KeyValuePairConverter : JsonConverter
    {
        private const string KeyName = "Key";
        private const string ValueName = "Value";

        private static readonly ThreadSafeStore<Type, ReflectionObject> ReflectionObjectPerType = new ThreadSafeStore<Type, ReflectionObject>(InitializeReflectionObject);

        private static ReflectionObject InitializeReflectionObject(Type t)
        {
            IList<Type> genericArguments = t.GetGenericArguments();
            Type keyType = genericArguments[0];
            Type valueType = genericArguments[1];

            return ReflectionObject.Create(t, t.GetConstructor(new[] { keyType, valueType }), KeyName, ValueName);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(value.GetType());

            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(KeyName) : KeyName);
            serializer.Serialize(writer, reflectionObject.GetValue(value, KeyName), reflectionObject.GetType(KeyName));
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(ValueName) : ValueName);
            serializer.Serialize(writer, reflectionObject.GetValue(value, ValueName), reflectionObject.GetType(ValueName));
            writer.WriteEndObject();
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
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullableType(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
                }

                return null;
            }

            object key = null;
            object value = null;

            reader.ReadAndAssert();

            Type t = ReflectionUtils.IsNullableType(objectType)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(t);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (string.Equals(propertyName, KeyName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadAndAssert();
                    key = serializer.Deserialize(reader, reflectionObject.GetType(KeyName));
                }
                else if (string.Equals(propertyName, ValueName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadAndAssert();
                    value = serializer.Deserialize(reader, reflectionObject.GetType(ValueName));
                }
                else
                {
                    reader.Skip();
                }

                reader.ReadAndAssert();
            }

            return reflectionObject.Creator(key, value);
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
            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            if (t.IsValueType() && t.IsGenericType())
            {
                return (t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>));
            }

            return false;
        }
    }
}