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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaWriter
    {
        private readonly JsonWriter _writer;
        private readonly JsonSchemaResolver _resolver;

        public JsonSchemaWriter(JsonWriter writer, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            _writer = writer;
            _resolver = resolver;
        }

        private void ReferenceOrWriteSchema(JsonSchema schema)
        {
            if (schema.Id != null && _resolver.GetSchema(schema.Id) != null)
            {
                _writer.WriteStartObject();
                _writer.WritePropertyName(JsonTypeReflector.RefPropertyName);
                _writer.WriteValue(schema.Id);
                _writer.WriteEndObject();
            }
            else
            {
                WriteSchema(schema);
            }
        }

        public void WriteSchema(JsonSchema schema)
        {
            ValidationUtils.ArgumentNotNull(schema, nameof(schema));

            if (!_resolver.LoadedSchemas.Contains(schema))
            {
                _resolver.LoadedSchemas.Add(schema);
            }

            _writer.WriteStartObject();
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.IdPropertyName, schema.Id);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.TitlePropertyName, schema.Title);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.DescriptionPropertyName, schema.Description);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.RequiredPropertyName, schema.Required);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.ReadOnlyPropertyName, schema.ReadOnly);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.HiddenPropertyName, schema.Hidden);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.TransientPropertyName, schema.Transient);
            if (schema.Type != null)
            {
                WriteType(JsonSchemaConstants.TypePropertyName, _writer, schema.Type.GetValueOrDefault());
            }
            if (!schema.AllowAdditionalProperties)
            {
                _writer.WritePropertyName(JsonSchemaConstants.AdditionalPropertiesPropertyName);
                _writer.WriteValue(schema.AllowAdditionalProperties);
            }
            else
            {
                if (schema.AdditionalProperties != null)
                {
                    _writer.WritePropertyName(JsonSchemaConstants.AdditionalPropertiesPropertyName);
                    ReferenceOrWriteSchema(schema.AdditionalProperties);
                }
            }
            if (!schema.AllowAdditionalItems)
            {
                _writer.WritePropertyName(JsonSchemaConstants.AdditionalItemsPropertyName);
                _writer.WriteValue(schema.AllowAdditionalItems);
            }
            else
            {
                if (schema.AdditionalItems != null)
                {
                    _writer.WritePropertyName(JsonSchemaConstants.AdditionalItemsPropertyName);
                    ReferenceOrWriteSchema(schema.AdditionalItems);
                }
            }
            WriteSchemaDictionaryIfNotNull(_writer, JsonSchemaConstants.PropertiesPropertyName, schema.Properties);
            WriteSchemaDictionaryIfNotNull(_writer, JsonSchemaConstants.PatternPropertiesPropertyName, schema.PatternProperties);
            WriteItems(schema);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MinimumPropertyName, schema.Minimum);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MaximumPropertyName, schema.Maximum);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.ExclusiveMinimumPropertyName, schema.ExclusiveMinimum);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.ExclusiveMaximumPropertyName, schema.ExclusiveMaximum);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MinimumLengthPropertyName, schema.MinimumLength);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MaximumLengthPropertyName, schema.MaximumLength);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MinimumItemsPropertyName, schema.MinimumItems);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.MaximumItemsPropertyName, schema.MaximumItems);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.DivisibleByPropertyName, schema.DivisibleBy);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.FormatPropertyName, schema.Format);
            WritePropertyIfNotNull(_writer, JsonSchemaConstants.PatternPropertyName, schema.Pattern);
            if (schema.Enum != null)
            {
                _writer.WritePropertyName(JsonSchemaConstants.EnumPropertyName);
                _writer.WriteStartArray();
                foreach (JToken token in schema.Enum)
                {
                    token.WriteTo(_writer);
                }
                _writer.WriteEndArray();
            }
            if (schema.Default != null)
            {
                _writer.WritePropertyName(JsonSchemaConstants.DefaultPropertyName);
                schema.Default.WriteTo(_writer);
            }
            if (schema.Disallow != null)
            {
                WriteType(JsonSchemaConstants.DisallowPropertyName, _writer, schema.Disallow.GetValueOrDefault());
            }
            if (schema.Extends != null && schema.Extends.Count > 0)
            {
                _writer.WritePropertyName(JsonSchemaConstants.ExtendsPropertyName);
                if (schema.Extends.Count == 1)
                {
                    ReferenceOrWriteSchema(schema.Extends[0]);
                }
                else
                {
                    _writer.WriteStartArray();
                    foreach (JsonSchema jsonSchema in schema.Extends)
                    {
                        ReferenceOrWriteSchema(jsonSchema);
                    }
                    _writer.WriteEndArray();
                }
            }
            _writer.WriteEndObject();
        }

        private void WriteSchemaDictionaryIfNotNull(JsonWriter writer, string propertyName, IDictionary<string, JsonSchema> properties)
        {
            if (properties != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteStartObject();
                foreach (KeyValuePair<string, JsonSchema> property in properties)
                {
                    writer.WritePropertyName(property.Key);
                    ReferenceOrWriteSchema(property.Value);
                }
                writer.WriteEndObject();
            }
        }

        private void WriteItems(JsonSchema schema)
        {
            if (schema.Items == null && !schema.PositionalItemsValidation)
            {
                return;
            }

            _writer.WritePropertyName(JsonSchemaConstants.ItemsPropertyName);

            if (!schema.PositionalItemsValidation)
            {
                if (schema.Items != null && schema.Items.Count > 0)
                {
                    ReferenceOrWriteSchema(schema.Items[0]);
                }
                else
                {
                    _writer.WriteStartObject();
                    _writer.WriteEndObject();
                }
                return;
            }

            _writer.WriteStartArray();
            if (schema.Items != null)
            {
                foreach (JsonSchema itemSchema in schema.Items)
                {
                    ReferenceOrWriteSchema(itemSchema);
                }
            }
            _writer.WriteEndArray();
        }

        private void WriteType(string propertyName, JsonWriter writer, JsonSchemaType type)
        {
            if (Enum.IsDefined(typeof(JsonSchemaType), type))
            {
                writer.WritePropertyName(propertyName);
                writer.WriteValue(JsonSchemaBuilder.MapType(type));
            }
            else
            {
                IEnumerator<JsonSchemaType> en = EnumUtils.GetFlagsValues(type).Where(v => v != JsonSchemaType.None).GetEnumerator();
                if (en.MoveNext())
                {
                    writer.WritePropertyName(propertyName);
                    JsonSchemaType first = en.Current;
                    if (en.MoveNext())
                    {
                        writer.WriteStartArray();
                        writer.WriteValue(JsonSchemaBuilder.MapType(first));
                        do
                        {
                            writer.WriteValue(JsonSchemaBuilder.MapType(en.Current));
                        } while (en.MoveNext());
                        writer.WriteEndArray();
                    }
                    else
                    {
                        writer.WriteValue(JsonSchemaBuilder.MapType(first));
                    }
                }
            }
        }

        private void WritePropertyIfNotNull(JsonWriter writer, string propertyName, object value)
        {
            if (value != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteValue(value);
            }
        }
    }
}