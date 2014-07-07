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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Schema
{
    internal class JsonSchemaBuilder
    {
        private readonly IList<JsonSchema> _stack;
        private readonly IList<string> _resolutionScopeStack;
        private readonly JsonSchemaResolver _resolver;
        private readonly IDictionary<string, JsonSchema> _documentSchemas;
        private JsonSchema _currentSchema;
        private string _currentResolutionScope;
        private IDictionary<string, JObject> _rootSchemas;

        public JsonSchemaBuilder(JsonSchemaResolver resolver)
        {
            _stack = new List<JsonSchema>();
            _resolutionScopeStack = new List<string>();
            _documentSchemas = new Dictionary<string, JsonSchema>();
            _rootSchemas = new Dictionary<string, JObject>();
            _resolver = resolver;
        }

        private void Push(JsonSchema value)
        {
            _currentSchema = value;
            _stack.Add(value);
            _resolver.LoadedSchemas.Add(value);
            _documentSchemas.Add(value.Location, value);
        }

        private void PushScope(string resolutionScope)
        {
            _currentResolutionScope = resolutionScope;
            _resolutionScopeStack.Add(resolutionScope);
        }

        private JsonSchema Pop()
        {
            JsonSchema poppedSchema = _currentSchema;
            _stack.RemoveAt(_stack.Count - 1);
            _currentSchema = _stack.LastOrDefault();

            return poppedSchema;
        }

        private void PopScope()
        {
            _resolutionScopeStack.RemoveAt(_resolutionScopeStack.Count - 1);
            _currentResolutionScope = _resolutionScopeStack.LastOrDefault();
        }

        private JsonSchema CurrentSchema
        {
            get { return _currentSchema; }
        }

        internal JsonSchema Read(JsonReader reader)
        {
            return Read(reader, null);
        }

        internal JsonSchema Read(JsonReader reader, Uri documentLocation)
        {
            string documentScope = "";

            if (documentLocation != null)
                documentScope = documentLocation.ToString();

            PushScope(documentScope);

            JToken schemaToken = JToken.ReadFrom(reader);

            _rootSchemas.Add(documentScope, schemaToken as JObject);

            JsonSchema schema = BuildSchema(schemaToken);
            schema = ResolveReferences(schema);

            PopScope();

            return schema;
        }

        private string UnescapeReference(string reference)
        {
            return Uri.UnescapeDataString(reference).Replace("~1", "/").Replace("~0", "~");
        }

        private JsonSchema ResolveReferences(JsonSchema schema)
        {
            while (schema.DeferredReference != null)
            {
                string schemaUri;
                string idPath;
                bool isJsonPointer;

                PrepareLocationReference(schema.ResolutionScope, schema.DeferredReference, out schemaUri, out idPath, out isJsonPointer);

                JsonSchema resolvedSchema;

                if (isJsonPointer)
                {
                    string reference = schemaUri + UnescapeReference(idPath);
                    resolvedSchema = _resolver.GetSchema(reference);

                    if (resolvedSchema == null)
                    {
                        resolvedSchema = FindLocationReference(schemaUri, idPath);
                    }
                }
                else
                {
                    resolvedSchema = _resolver.GetSchema(idPath);
                }

                if (resolvedSchema == null)
                    throw new JsonException("Could not resolve schema reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, schema.DeferredReference));

                schema = resolvedSchema;
            }

            if (schema.ReferencesResolved)
                return schema;

            schema.ReferencesResolved = true;

            if (schema.AnyOf != null)
            {
                for (int i = 0; i < schema.AnyOf.Count; i++)
                {
                    schema.AnyOf[i] = ResolveReferences(schema.AnyOf[i]);
                }
            }

            if (schema.AllOf != null)
            {
                for (int i = 0; i < schema.AllOf.Count; i++)
                {
                    schema.AllOf[i] = ResolveReferences(schema.AllOf[i]);
                }
            }

            if (schema.OneOf != null)
            {
                for (int i = 0; i < schema.OneOf.Count; i++)
                {
                    schema.OneOf[i] = ResolveReferences(schema.OneOf[i]);
                }
            }

            if (schema.NotOf != null)
                schema.NotOf = ResolveReferences(schema.NotOf);

            if (schema.Items != null)
            {
                for (int i = 0; i < schema.Items.Count; i++)
                {
                    schema.Items[i] = ResolveReferences(schema.Items[i]);
                }
            }

            if (schema.AdditionalItems != null)
                schema.AdditionalItems = ResolveReferences(schema.AdditionalItems);

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
                schema.AdditionalProperties = ResolveReferences(schema.AdditionalProperties);

            return schema;
        }

        private void PrepareLocationReference(string resolutionScope, string reference, out string schemaUri, out string idPath, out bool isJsonPointer)
        {
            if (reference.Equals("#", StringComparison.Ordinal) ||
                reference.StartsWith("#/", StringComparison.Ordinal))
            {
                // Simple base schema reference or Json pointer fragment
                schemaUri = resolutionScope;
                idPath = reference;
                isJsonPointer = true;
                return;
            }

            Uri referenceUri = new Uri(reference, UriKind.RelativeOrAbsolute);

            if (referenceUri.IsAbsoluteUri)
            {
                schemaUri = Uri.UnescapeDataString(reference);
                isJsonPointer = true;

                int hashPosition = schemaUri.IndexOf("#", StringComparison.Ordinal);
                if (hashPosition > -1)
                {
                    idPath = schemaUri.Substring(hashPosition);
                    schemaUri = schemaUri.Substring(0, hashPosition);
                }
                else
                {
                    idPath = "#";
                }
            }
            else
            {
                Uri resolutionScopeUri = new Uri(resolutionScope, UriKind.RelativeOrAbsolute);

                if (resolutionScopeUri.IsAbsoluteUri)
                {
                    // Using id scopes
                    Uri fullUri = new Uri(resolutionScopeUri, referenceUri);
                    schemaUri = Uri.UnescapeDataString(fullUri.ToString());
                    isJsonPointer = true;

                    int hashPosition = schemaUri.IndexOf("#", StringComparison.Ordinal);
                    if (hashPosition > -1)
                    {
                        idPath = schemaUri.Substring(hashPosition);
                        schemaUri = schemaUri.Substring(0, hashPosition);
                    }
                    else
                    {
                        idPath = "#";
                    }
                }
                else
                {
                    // Using plain schema ids
                    schemaUri = "";
                    idPath = reference;
                    isJsonPointer = false;
                }
            }
        }

        private JsonSchema FindLocationReference(string schemaUri, string reference)
        {
            if (!_rootSchemas.Keys.Contains(schemaUri))
            {
                if (!_resolver.ResolveExternals)
                    return null;

                string remoteSchemaJson = _resolver.GetRemoteSchemaContents(new Uri(schemaUri));

                if (string.IsNullOrEmpty(remoteSchemaJson))
                    return null;

                using (JsonReader reader = new JsonTextReader(new StringReader(remoteSchemaJson)))
                {
                    JToken schemaToken = JToken.ReadFrom(reader);
                    _rootSchemas.Add(schemaUri, schemaToken as JObject);
                }
            }

            bool hasPushedScope = false;
            JsonSchema foundSchema = null;

            if (!schemaUri.Equals(_currentResolutionScope, StringComparison.Ordinal))
            {
                PushScope(schemaUri);
                hasPushedScope = true;
            }

            if (string.IsNullOrEmpty(reference) || reference.Equals("#", StringComparison.Ordinal))
            {
                // Whole schema
                foundSchema = BuildSchema(_rootSchemas[_currentResolutionScope]);
            }
            else
            {
                string[] escapedParts = reference.TrimStart('#').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                JToken currentToken = _rootSchemas[_currentResolutionScope];
                foreach (string escapedPart in escapedParts)
                {
                    string part = UnescapeReference(escapedPart);

                    if (currentToken.Type == JTokenType.Object)
                    {
                        currentToken = currentToken[part];
                    }
                    else if (currentToken.Type == JTokenType.Array || currentToken.Type == JTokenType.Constructor)
                    {
                        int index;
                        if (int.TryParse(part, out index) && index >= 0 && index < currentToken.Count())
                            currentToken = currentToken[index];
                        else
                            currentToken = null;
                    }

                    if (currentToken == null)
                        break;
                }

                if (currentToken != null)
                    foundSchema = BuildSchema(currentToken);
            } 

            if (hasPushedScope)
                PopScope();

            return foundSchema;
        }
        
        private JsonSchema BuildSchema(JToken token)
        {
            JObject schemaObject = token as JObject;
            if (schemaObject == null)
                throw JsonException.Create(token, token.Path, "Expected object while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

            // Check for change of scope before processing
            JToken idToken;
            bool hasPushedScope = false;
            if (schemaObject.TryGetValue(JsonSchemaConstants.IdPropertyName, out idToken))
            {
                string subScope = (string)idToken;
                Uri scopeUri;

                hasPushedScope = true;

                if (string.IsNullOrEmpty(_currentResolutionScope))
                {
                    PushScope(subScope);
                }
                else
                {
                    scopeUri = new Uri(_currentResolutionScope, UriKind.RelativeOrAbsolute);
                    Uri subScopeUri = new Uri(subScope, UriKind.RelativeOrAbsolute);

                    if (subScopeUri.IsAbsoluteUri)
                    {
                        PushScope(subScopeUri.ToString());
                    }
                    else
                    {
                        Uri combinedUri = new Uri(scopeUri, subScope);
                        PushScope(combinedUri.ToString());
                    }
                }
            }

            JToken referenceToken;
            if (schemaObject.TryGetValue(JsonTypeReflector.RefPropertyName, out referenceToken))
            {
                JsonSchema deferredSchema = new JsonSchema();
                deferredSchema.DeferredReference = (string)referenceToken;
                deferredSchema.ResolutionScope = _currentResolutionScope;

                return deferredSchema;
            }

            string location = token.Path.Replace(".", "/").Replace("[", "/").Replace("]", string.Empty);
            if (!string.IsNullOrEmpty(location))
                location = "/" + location;

            location = _currentResolutionScope + "#" + location;

            JsonSchema existingSchema;
            if (_documentSchemas.TryGetValue(location, out existingSchema))
                return existingSchema;

            Push(new JsonSchema { Location = location });

            ProcessSchemaProperties(schemaObject);

            if (hasPushedScope)
                PopScope();

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
                        ProcessRequired(property.Value);
                        break;
                    case JsonSchemaConstants.DependenciesPropertyName:
                        CurrentSchema.Dependencies = ProcessDependencies(property.Value);
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
                    case JsonSchemaConstants.MultipleOfPropertyName:
                        CurrentSchema.MultipleOf = (double)property.Value;
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
                    case JsonSchemaConstants.AnyOfPropertyName:
                        CurrentSchema.AnyOf = ProcessSchemaGroup(property.Value);
                        break;
                    case JsonSchemaConstants.AllOfPropertyName:
                        CurrentSchema.AllOf = ProcessSchemaGroup(property.Value);
                        break;
                    case JsonSchemaConstants.OneOfPropertyName:
                        CurrentSchema.OneOf = ProcessSchemaGroup(property.Value);
                        break;
                    case JsonSchemaConstants.NotOfPropertyName:
                        CurrentSchema.NotOf = BuildSchema(property.Value);
                        break;
                    case JsonSchemaConstants.UniqueItemsPropertyName:
                        CurrentSchema.UniqueItems = (bool)property.Value;
                        break;
                    case JsonSchemaConstants.MinimumPropertiesPropertyName:
                        CurrentSchema.MinimumProperties = (int)property.Value;
                        break;
                    case JsonSchemaConstants.MaximumPropertiesPropertyName:
                        CurrentSchema.MaximumProperties = (int)property.Value;
                        break;
                }
            }
        }

        private IList<JsonSchema> ProcessSchemaGroup(JToken token)
        {
            if (token.Type != JTokenType.Array)
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing schema group, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

            IList<JsonSchema> schemas = new List<JsonSchema>();

            foreach (JToken schemaObject in token)
            {
                schemas.Add(BuildSchema(schemaObject));
            }

            if (schemas.Count > 0)
            {
                return schemas;
            }

            return null;
        }

        private void ProcessEnum(JToken token)
        {
            if (token.Type != JTokenType.Array)
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

            CurrentSchema.Enum = new List<JToken>();

            foreach (JToken enumValue in token)
            {
                CurrentSchema.Enum.Add(enumValue.DeepClone());
            }
        }

        private void ProcessRequired(JToken token)
        {
            // Most common breaking change for draft v3 -> draft v4
            if (token.Type == JTokenType.Boolean)
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing required properties, got Boolean. Possible draft v3 schema.");

            if (token.Type != JTokenType.Array)
                throw JsonException.Create(token, token.Path, "Expected Array token while parsing required properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

            CurrentSchema.Required = new List<string>();

            foreach (JToken propValue in token)
            {
                CurrentSchema.Required.Add((string)propValue);
            }
        }

        private void ProcessAdditionalProperties(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
                CurrentSchema.AllowAdditionalProperties = (bool)token;
            else
                CurrentSchema.AdditionalProperties = BuildSchema(token);
        }

        private void ProcessAdditionalItems(JToken token)
        {
            if (token.Type == JTokenType.Boolean)
                CurrentSchema.AllowAdditionalItems = (bool)token;
            else
                CurrentSchema.AdditionalItems = BuildSchema(token);
        }

        private IDictionary<string, JsonSchema> ProcessProperties(JToken token)
        {
            IDictionary<string, JsonSchema> properties = new Dictionary<string, JsonSchema>();

            if (token.Type != JTokenType.Object)
                throw JsonException.Create(token, token.Path, "Expected Object token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

            foreach (JProperty propertyToken in token)
            {
                if (properties.ContainsKey(propertyToken.Name))
                    throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, propertyToken.Name));

                properties.Add(propertyToken.Name, BuildSchema(propertyToken.Value));
            }

            return properties;
        }

        private IDictionary<string, JsonSchema> ProcessDependencies(JToken token)
        {
            IDictionary<string, JsonSchema> dependencies = new Dictionary<string, JsonSchema>();

            foreach (JProperty dependencyToken in token)
            {
                if (dependencies.ContainsKey(dependencyToken.Name))
                    throw new JsonException("Dependency {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, dependencyToken.Name));

                switch (dependencyToken.Value.Type)
                {
                    case JTokenType.String:
                        JsonSchema singleProperty = new JsonSchema { Type = JsonSchemaType.Object };
                        singleProperty.Required = new List<string> { (string)dependencyToken.Value };
                        singleProperty.Properties = new Dictionary<string, JsonSchema>();
                        singleProperty.Properties.Add((string)dependencyToken.Value, new JsonSchema() { Type = JsonSchemaType.Any });
                        dependencies.Add(dependencyToken.Name, singleProperty);
                        break;
                    case JTokenType.Array:
                        JsonSchema multipleProperty = new JsonSchema { Type = JsonSchemaType.Object };
                        multipleProperty.Required = new List<string>();
                        multipleProperty.Properties = new Dictionary<string, JsonSchema>();
                        foreach (JToken schemaToken in dependencyToken.Value)
                        {
                            multipleProperty.Required.Add((string)schemaToken);
                            multipleProperty.Properties.Add((string)schemaToken, new JsonSchema() { Type = JsonSchemaType.Any });
                        }
                        dependencies.Add(dependencyToken.Name, multipleProperty);
                        break;
                    case JTokenType.Object:
                        dependencies.Add(dependencyToken.Name, BuildSchema(dependencyToken.Value));
                        break;
                    default:
                        throw JsonException.Create(token, token.Path, "Expected string, array or JSON schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

                }
            }

            return dependencies;
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
                            throw JsonException.Create(typeToken, typeToken.Path, "Exception JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));

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
            JsonSchemaType mappedType;
            if (!JsonSchemaConstants.JsonSchemaTypeMapping.TryGetValue(type, out mappedType))
                throw new JsonException("Invalid JSON schema type: {0}".FormatWith(CultureInfo.InvariantCulture, type));

            return mappedType;
        }

        internal static string MapType(JsonSchemaType type)
        {
            return JsonSchemaConstants.JsonSchemaTypeMapping.Single(kv => kv.Value == type).Key;
        }
    }
}
