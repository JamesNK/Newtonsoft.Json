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
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Globalization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

#nullable disable

namespace Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaBuilder
    {
        private readonly IList<JsonSchema> _stack;
        private readonly JsonSchemaResolver _resolver;
        private readonly IDictionary<string, JsonSchema> _documentSchemas;
        private JsonSchema _currentSchema;
        private JObject _rootSchema;

        public JsonSchemaBuilder(JsonSchemaResolver resolver)
        {
            _stack = new List<JsonSchema>();
            _documentSchemas = new Dictionary<string, JsonSchema>();
            _resolver = resolver;
        }

        private void Push(JsonSchema value)
        {
            _currentSchema = value;
            _stack.Add(value);
            _resolver.LoadedSchemas.Add(value);
            _documentSchemas.Add(value.Location, value);
        }

        private JsonSchema Pop()
        {
            JsonSchema poppedSchema = _currentSchema;
            _stack.RemoveAt(_stack.Count - 1);
            _currentSchema = _stack.LastOrDefault();

            return poppedSchema;
        }

        private JsonSchema CurrentSchema => _currentSchema;

        internal JsonSchema Read(JsonReader reader)
        {
            JToken schemaToken = JToken.ReadFrom(reader);

            _rootSchema = schemaToken as JObject;

            JsonSchema schema = BuildSchema(schemaToken);

            ResolveReferences(schema);

            return schema;
        }

        private string UnescapeReference(string reference)
        {
            string unescapedReference = Uri.UnescapeDataString(reference);
            unescapedReference = StringUtils.Replace(unescapedReference, "~1", "/");
            unescapedReference = StringUtils.Replace(unescapedReference, "~0", "~");
            return unescapedReference;
        }

        private JsonSchema ResolveReferences(JsonSchema schema)
        {
            if (schema.DeferredReference != null)
            {
                string reference = schema.DeferredReference;

                bool locationReference = (reference.StartsWith("#", StringComparison.Ordinal));
                if (locationReference)
                {
                    reference = UnescapeReference(reference);
                }

                JsonSchema resolvedSchema = _resolver.GetSchema(reference);

                if (resolvedSchema == null)
                {
                    if (locationReference)
                    {
                        string[] escapedParts = schema.DeferredReference.TrimStart('#').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        JToken currentToken = _rootSchema;
                        foreach (string escapedPart in escapedParts)
                        {
                            string part = UnescapeReference(escapedPart);

                            if (currentToken.Type == JTokenType.Object)
                            {
                                currentToken = currentToken[part];
                            }
                            else if (currentToken.Type == JTokenType.Array || currentToken.Type == JTokenType.Constructor)
                            {
                                if (int.TryParse(part, out int index) && index >= 0 && index < currentToken.Count())
                                {
                                    currentToken = currentToken[index];
                                }
                                else
                                {
                                    currentToken = null;
                                }
                            }

                            if (currentToken == null)
                            {
                                break;
                            }
                        }

                        if (currentToken != null)
                        {
                            resolvedSchema = BuildSchema(currentToken);
                        }
                    }

                    if (resolvedSchema == null)
                    {
                        throw new JsonException("Could not resolve schema reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, schema.DeferredReference));
                    }
                }

                schema = resolvedSchema;
            }

            if (schema.ReferencesResolved)
            {
                return schema;
            }

            schema.ReferencesResolved = true;

            if (schema.Extends != null)
            {
                for (int i = 0; i < schema.Extends.Count; i++)
                {
                    schema.Extends[i] = ResolveReferences(schema.Extends[i]);
                }
            }

            if (schema.Items != null)
            {
                for (int i = 0; i < schema.Items.Count; i++)
                {
                    schema.Items[i] = ResolveReferences(schema.Items[i]);
                }
            }

            if (schema.AdditionalItems != null)
            {
                schema.AdditionalItems = ResolveReferences(schema.AdditionalItems);
            }

            if (schema.PatternProperties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> patternProperty in schema.PatternProperties.ToList())
                {
                    schema.PatternProperties[patternProperty.Key] = ResolveReferences(patternProperty.Value);
                }
            }

            if (schema.Properties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> property in schema.Properties.ToList())
                {
                    schema.Properties[property.Key] = ResolveReferences(property.Value);
                }
            }

            if (schema.AdditionalProperties != null)
            {
                schema.AdditionalProperties = ResolveReferences(schema.AdditionalProperties);
            }

            return schema;
        }

        private JsonSchema BuildSchema(JToken token)
        {
            if (!(token is JObject schemaObject))
            {
                throw JsonException.Create(token, token.Path, "Expected object while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            if (schemaObject.TryGetValue(JsonTypeReflector.RefPropertyName, out JToken referenceToken))
            {
                JsonSchema deferredSchema = new JsonSchema();
                deferredSchema.DeferredReference = (string)referenceToken;

                return deferredSchema;
            }

            string location = token.Path;
            location = StringUtils.Replace(location, ".", "/");
            location = StringUtils.Replace(location, "[", "/");
            location = StringUtils.Replace(location, "]", string.Empty);

            if (!StringUtils.IsNullOrEmpty(location))
            {
                location = "/" + location;
            }
            location = "#" + location;

            if (_documentSchemas.TryGetValue(location, out JsonSchema existingSchema))
            {
                return existingSchema;
            }

            Push(new JsonSchema { Location = location });

            ProcessSchemaProperties(schemaObject);

            return Pop();
        }

        private void ProcessSchemaProperties(JObject schemaObject)
        {
            foreach (KeyValuePair<string, JToken> property in schemaObject)
            {
                switch (property.Key)
                {
                    case JsonSchemaConstants.TypePropertyName:
                        CurrentSchema.Type = ProcessType(property.Value);
                        break;
                    case JsonSchemaConstants.IdPropertyName:
                        CurrentSchema.Id = (string)property.Value;
                        break;
                    case JsonSchemaConstants.TitlePropertyName:
                        CurrentSchema.Title = (string)property.Value;
                        break;
                    case JsonSchemaConstants.DescriptionPropertyName:
                        CurrentSchema.Description = (string)property.Value;
                        break;
                    case JsonSchemaConstants.PropertiesPropertyName:
                        CurrentSchema.Properties = ProcessProperties(property.Value);
                        break;
                    case JsonSchemaConstants.ItemsPropertyName:
                        ProcessItems(property.Value);
                        break;
                    case JsonSchemaConstants.AdditionalPropertiesPropertyName:
                        ProcessAdditionalProperties(property.Value);
                        break;
                    case JsonSchemaConstants.AdditionalItemsPropertyName:
                        ProcessAdditionalItems(property.Value);
                        break;
                    case JsonSchemaConstants.PatternPropertiesPropertyName:
                        CurrentSchema.PatternProperties = ProcessProperties(property.Value);
                        break;
                    case JsonSchemaConstants.RequiredPropertyName:
                        CurrentSchema.Required = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.RequiresPropertyName:
                        CurrentSchema.Requires = (string)property.Value;
                        break;
                    case JsonSchemaConstants.MinimumPropertyName:
                        CurrentSchema.Minimum = (double)property.Value;
                        break;
                    case JsonSchemaConstants.MaximumPropertyName:
                        CurrentSchema.Maximum = (double)property.Value;
                        break;
                    case JsonSchemaConstants.ExclusiveMinimumPropertyName:
                        CurrentSchema.ExclusiveMinimum = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.ExclusiveMaximumPropertyName:
                        CurrentSchema.ExclusiveMaximum = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.MaximumLengthPropertyName:
                        CurrentSchema.MaximumLength = (int)property.Value;
                        break;
                    case JsonSchemaConstants.MinimumLengthPropertyName:
                        CurrentSchema.MinimumLength = (int)property.Value;
                        break;
                    case JsonSchemaConstants.MaximumItemsPropertyName:
                        CurrentSchema.MaximumItems = (int)property.Value;
                        break;
                    case JsonSchemaConstants.MinimumItemsPropertyName:
                        CurrentSchema.MinimumItems = (int)property.Value;
                        break;
                    case JsonSchemaConstants.DivisibleByPropertyName:
                        CurrentSchema.DivisibleBy = (double)property.Value;
                        break;
                    case JsonSchemaConstants.DisallowPropertyName:
                        CurrentSchema.Disallow = ProcessType(property.Value);
                        break;
                    case JsonSchemaConstants.DefaultPropertyName:
                        CurrentSchema.Default = property.Value.DeepClone();
                        break;
                    case JsonSchemaConstants.HiddenPropertyName:
                        CurrentSchema.Hidden = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.ReadOnlyPropertyName:
                        CurrentSchema.ReadOnly = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.FormatPropertyName:
                        CurrentSchema.Format = (string)property.Value;
                        break;
                    case JsonSchemaConstants.PatternPropertyName:
                        CurrentSchema.Pattern = (string)property.Value;
                        break;
                    case JsonSchemaConstants.EnumPropertyName:
                        ProcessEnum(property.Value);
                        break;
                    case JsonSchemaConstants.ExtendsPropertyName:
                        ProcessExtends(property.Value);
                        break;
                    case JsonSchemaConstants.UniqueItemsPropertyName:
                        CurrentSchema.UniqueItems = (bool)property.Value;
                        break;
                }
            }
        }

        private void ProcessExtends(JToken token)
        {
            IList<JsonSchema> schemas = new List<JsonSchema>();

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken schemaObject in token)
                {
                    schemas.Add(BuildSchema(schemaObject));
                }
            }
            else
            {
                JsonSchema schema = BuildSchema(token);
                if (schema != null)
                {
                    schemas.Add(schema);
                }
            }

            if (schemas.Count > 0)
            {
                CurrentSchema.Extends = schemas;
            }
        }

        private void ProcessEnum(JToken token)
        {
            if (token.Type != JTokenType.Array)
            {
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            CurrentSchema.Enum = new List<JToken>();

            foreach (JToken enumValue in token)
            {
                CurrentSchema.Enum.Add(enumValue.DeepClone());
            }
        }

        private void ProcessAdditionalProperties(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
            {
                CurrentSchema.AllowAdditionalProperties = (bool)token;
            }
            else
            {
                CurrentSchema.AdditionalProperties = BuildSchema(token);
            }
        }

        private void ProcessAdditionalItems(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
            {
                CurrentSchema.AllowAdditionalItems = (bool)token;
            }
            else
            {
                CurrentSchema.AdditionalItems = BuildSchema(token);
            }
        }

        private IDictionary<string, JsonSchema> ProcessProperties(JToken token)
        {
            IDictionary<string, JsonSchema> properties = new Dictionary<string, JsonSchema>();

            if (token.Type != JTokenType.Object)
            {
                throw JsonException.Create(token, token.Path, "Expected Object token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            foreach (JProperty propertyToken in token)
            {
                if (properties.ContainsKey(propertyToken.Name))
                {
                    throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, propertyToken.Name));
                }

                properties.Add(propertyToken.Name, BuildSchema(propertyToken.Value));
            }

            return properties;
        }

        private void ProcessItems(JToken token)
        {
            CurrentSchema.Items = new List<JsonSchema>();

            switch (token.Type)
            {
                case JTokenType.Object:
                    CurrentSchema.Items.Add(BuildSchema(token));
                    CurrentSchema.PositionalItemsValidation = false;
                    break;
                case JTokenType.Array:
                    CurrentSchema.PositionalItemsValidation = true;
                    foreach (JToken schemaToken in token)
                    {
                        CurrentSchema.Items.Add(BuildSchema(schemaToken));
                    }
                    break;
                default:
                    throw JsonException.Create(token, token.Path, "Expected array or JSON schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }
        }

        private JsonSchemaType? ProcessType(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    // ensure type is in blank state before ORing values
                    JsonSchemaType? type = JsonSchemaType.None;

                    foreach (JToken typeToken in token)
                    {
                        if (typeToken.Type != JTokenType.String)
                        {
                            throw JsonException.Create(typeToken, typeToken.Path, "Expected JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
                        }

                        type = type | MapType((string)typeToken);
                    }

                    return type;
                case JTokenType.String:
                    return MapType((string)token);
                default:
                    throw JsonException.Create(token, token.Path, "Expected array or JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }
        }

        internal static JsonSchemaType MapType(string type)
        {
            if (!JsonSchemaConstants.JsonSchemaTypeMapping.TryGetValue(type, out JsonSchemaType mappedType))
            {
                throw new JsonException("Invalid JSON schema type: {0}".FormatWith(CultureInfo.InvariantCulture, type));
            }

            return mappedType;
        }

        internal static string MapType(JsonSchemaType type)
        {
            return JsonSchemaConstants.JsonSchemaTypeMapping.Single(kv => kv.Value == type).Key;
        }
    }
}