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

#if HAVE_ENTITY_FRAMEWORK
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Diagnostics;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts an Entity Framework <see cref="T:System.Data.EntityKeyMember"/> to and from JSON.
    /// </summary>
    [RequiresUnreferencedCode(MiscellaneousUtils.TrimWarning)]
    [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
    public class EntityKeyMemberConverter : JsonConverter
    {
        private const string EntityKeyMemberFullTypeName = "System.Data.EntityKeyMember";

        private const string KeyPropertyName = "Key";
        private const string TypePropertyName = "Type";
        private const string ValuePropertyName = "Value";

        private static ReflectionObject? _reflectionObject;

        private static readonly Dictionary<string, Type> AllowedTypesByName = new Dictionary<string, Type>
        {
            { "System.Boolean", typeof(bool) },
            { "System.Byte", typeof(byte) },
            { "System.Byte[]", typeof(byte[]) },
            { "System.DateTime", typeof(DateTime) },
            { "System.DateTimeOffset", typeof(DateTimeOffset) },
            { "System.Decimal", typeof(decimal) },
            { "System.Double", typeof(double) },
            { "System.Guid", typeof(Guid) },
            { "System.Int16", typeof(short) },
            { "System.Int32", typeof(int) },
            { "System.Int64", typeof(long) },
            { "System.SByte", typeof(sbyte) },
            { "System.Single", typeof(float) },
            { "System.String", typeof(string) },
            { "System.TimeSpan", typeof(TimeSpan) },
            { "System.UInt64", typeof(ulong) }
        };

        private static readonly Dictionary<Type, string> AllowedNamesByType = CreateAllowedNamesByType();

        private static Dictionary<Type, string> CreateAllowedNamesByType()
        {
            Dictionary<Type, string> d = new Dictionary<Type, string>();
            foreach (KeyValuePair<string, Type> pair in AllowedTypesByName)
            {
                d[pair.Value] = pair.Key;
            }
            return d;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            EnsureReflectionObject(value.GetType());
            MiscellaneousUtils.Assert(_reflectionObject != null);

            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            string keyName = (string)_reflectionObject.GetValue(value, KeyPropertyName)!;
            object? keyValue = _reflectionObject.GetValue(value, ValuePropertyName);

            Type? keyValueType = keyValue?.GetType();

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(KeyPropertyName) : KeyPropertyName);
            writer.WriteValue(keyName);
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(TypePropertyName) : TypePropertyName);
            if (keyValueType != null)
            {
                if (!AllowedNamesByType.TryGetValue(keyValueType, out string? typeName))
                {
                    throw new JsonSerializationException("Type '{0}' is not a valid EntityKeyMember type.".FormatWith(CultureInfo.InvariantCulture, keyValueType));
                }
                writer.WriteValue(typeName);
            }
            else
            {
                writer.WriteNull();
            }

            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(ValuePropertyName) : ValuePropertyName);

            if (keyValueType != null)
            {
                if (JsonSerializerInternalWriter.TryConvertToString(keyValue!, keyValueType, out string? valueJson))
                {
                    writer.WriteValue(valueJson);
                }
                else
                {
                    writer.WriteValue(keyValue);
                }
            }
            else
            {
                writer.WriteNull();
            }

            writer.WriteEndObject();
        }

        private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
        {
            reader.ReadAndAssert();

            if (reader.TokenType != JsonToken.PropertyName || !string.Equals(reader.Value?.ToString(), propertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            EnsureReflectionObject(objectType);
            MiscellaneousUtils.Assert(_reflectionObject != null);

            object entityKeyMember = _reflectionObject.Creator!();

            ReadAndAssertProperty(reader, KeyPropertyName);
            reader.ReadAndAssert();
            _reflectionObject.SetValue(entityKeyMember, KeyPropertyName, reader.Value?.ToString());

            ReadAndAssertProperty(reader, TypePropertyName);
            reader.ReadAndAssert();
            string? type = reader.Value?.ToString();

            if (!AllowedTypesByName.TryGetValue(type!, out Type? t))
            {
                throw new JsonSerializationException("Type '{0}' is not a valid EntityKeyMember type.".FormatWith(CultureInfo.InvariantCulture, type));
            }

            ReadAndAssertProperty(reader, ValuePropertyName);
            reader.ReadAndAssert();
            _reflectionObject.SetValue(entityKeyMember, ValuePropertyName, serializer.Deserialize(reader, t));

            reader.ReadAndAssert();

            return entityKeyMember;
        }

        private static void EnsureReflectionObject(Type objectType)
        {
            if (_reflectionObject == null)
            {
                _reflectionObject = ReflectionObject.Create(objectType, KeyPropertyName, ValuePropertyName);
            }
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
            return objectType.AssignableToTypeName(EntityKeyMemberFullTypeName);
        }
    }
}

#endif