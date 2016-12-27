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
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_1
using System.Numerics;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json
{
    /// <summary>
    /// <para>
    /// Represents a reader that provides <see cref="JsonSchema"/> validation.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonValidatingReader : JsonReader, IJsonLineInfo
    {
        private class SchemaScope
        {
            private readonly JTokenType _tokenType;
            private readonly IList<JsonSchemaModel> _schemas;
            private readonly Dictionary<string, bool> _requiredProperties;

            public string CurrentPropertyName { get; set; }
            public int ArrayItemCount { get; set; }
            public bool IsUniqueArray { get; set; }
            public IList<JToken> UniqueArrayItems { get; set; }
            public JTokenWriter CurrentItemWriter { get; set; }

            public IList<JsonSchemaModel> Schemas
            {
                get { return _schemas; }
            }

            public Dictionary<string, bool> RequiredProperties
            {
                get { return _requiredProperties; }
            }

            public JTokenType TokenType
            {
                get { return _tokenType; }
            }

            public SchemaScope(JTokenType tokenType, IList<JsonSchemaModel> schemas)
            {
                _tokenType = tokenType;
                _schemas = schemas;

                _requiredProperties = schemas.SelectMany<JsonSchemaModel, string>(GetRequiredProperties).Distinct().ToDictionary(p => p, p => false);

                if (tokenType == JTokenType.Array && schemas.Any(s => s.UniqueItems))
                {
                    IsUniqueArray = true;
                    UniqueArrayItems = new List<JToken>();
                }
            }

            private IEnumerable<string> GetRequiredProperties(JsonSchemaModel schema)
            {
                if (schema == null || schema.Properties == null)
                {
                    return Enumerable.Empty<string>();
                }

                return schema.Properties.Where(p => p.Value.Required).Select(p => p.Key);
            }
        }

        private readonly JsonReader _reader;
        private readonly Stack<SchemaScope> _stack;
        private JsonSchema _schema;
        private JsonSchemaModel _model;
        private SchemaScope _currentScope;

        /// <summary>
        /// Sets an event handler for receiving schema validation errors.
        /// </summary>
        public event ValidationEventHandler ValidationEventHandler;

        /// <summary>
        /// Gets the text value of the current JSON token.
        /// </summary>
        /// <value></value>
        public override object Value
        {
            get { return _reader.Value; }
        }

        /// <summary>
        /// Gets the depth of the current token in the JSON document.
        /// </summary>
        /// <value>The depth of the current token in the JSON document.</value>
        public override int Depth
        {
            get { return _reader.Depth; }
        }

        /// <summary>
        /// Gets the path of the current JSON token. 
        /// </summary>
        public override string Path
        {
            get { return _reader.Path; }
        }

        /// <summary>
        /// Gets the quotation mark character used to enclose the value of a string.
        /// </summary>
        /// <value></value>
        public override char QuoteChar
        {
            get { return _reader.QuoteChar; }
            protected internal set { }
        }

        /// <summary>
        /// Gets the type of the current JSON token.
        /// </summary>
        /// <value></value>
        public override JsonToken TokenType
        {
            get { return _reader.TokenType; }
        }

        /// <summary>
        /// Gets the .NET type for the current JSON token.
        /// </summary>
        /// <value></value>
        public override Type ValueType
        {
            get { return _reader.ValueType; }
        }

        private void Push(SchemaScope scope)
        {
            _stack.Push(scope);
            _currentScope = scope;
        }

        private SchemaScope Pop()
        {
            SchemaScope poppedScope = _stack.Pop();
            _currentScope = (_stack.Count != 0)
                ? _stack.Peek()
                : null;

            return poppedScope;
        }

        private IList<JsonSchemaModel> CurrentSchemas
        {
            get { return _currentScope.Schemas; }
        }

        private static readonly IList<JsonSchemaModel> EmptySchemaList = new List<JsonSchemaModel>();

        private IList<JsonSchemaModel> CurrentMemberSchemas
        {
            get
            {
                if (_currentScope == null)
                {
                    return new List<JsonSchemaModel>(new[] { _model });
                }

                if (_currentScope.Schemas == null || _currentScope.Schemas.Count == 0)
                {
                    return EmptySchemaList;
                }

                switch (_currentScope.TokenType)
                {
                    case JTokenType.None:
                        return _currentScope.Schemas;
                    case JTokenType.Object:
                    {
                        if (_currentScope.CurrentPropertyName == null)
                        {
                            throw new JsonReaderException("CurrentPropertyName has not been set on scope.");
                        }

                        IList<JsonSchemaModel> schemas = new List<JsonSchemaModel>();

                        foreach (JsonSchemaModel schema in CurrentSchemas)
                        {
                            JsonSchemaModel propertySchema;
                            if (schema.Properties != null && schema.Properties.TryGetValue(_currentScope.CurrentPropertyName, out propertySchema))
                            {
                                schemas.Add(propertySchema);
                            }
                            if (schema.PatternProperties != null)
                            {
                                foreach (KeyValuePair<string, JsonSchemaModel> patternProperty in schema.PatternProperties)
                                {
                                    if (Regex.IsMatch(_currentScope.CurrentPropertyName, patternProperty.Key))
                                    {
                                        schemas.Add(patternProperty.Value);
                                    }
                                }
                            }

                            if (schemas.Count == 0 && schema.AllowAdditionalProperties && schema.AdditionalProperties != null)
                            {
                                schemas.Add(schema.AdditionalProperties);
                            }
                        }

                        return schemas;
                    }
                    case JTokenType.Array:
                    {
                        IList<JsonSchemaModel> schemas = new List<JsonSchemaModel>();

                        foreach (JsonSchemaModel schema in CurrentSchemas)
                        {
                            if (!schema.PositionalItemsValidation)
                            {
                                if (schema.Items != null && schema.Items.Count > 0)
                                {
                                    schemas.Add(schema.Items[0]);
                                }
                            }
                            else
                            {
                                if (schema.Items != null && schema.Items.Count > 0)
                                {
                                    if (schema.Items.Count > (_currentScope.ArrayItemCount - 1))
                                    {
                                        schemas.Add(schema.Items[_currentScope.ArrayItemCount - 1]);
                                    }
                                }

                                if (schema.AllowAdditionalItems && schema.AdditionalItems != null)
                                {
                                    schemas.Add(schema.AdditionalItems);
                                }
                            }
                        }

                        return schemas;
                    }
                    case JTokenType.Constructor:
                        return EmptySchemaList;
                    default:
                        throw new ArgumentOutOfRangeException("TokenType", "Unexpected token type: {0}".FormatWith(CultureInfo.InvariantCulture, _currentScope.TokenType));
                }
            }
        }

        private void RaiseError(string message, JsonSchemaModel schema)
        {
            IJsonLineInfo lineInfo = this;

            string exceptionMessage = (lineInfo.HasLineInfo())
                ? message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition)
                : message;

            OnValidationEvent(new JsonSchemaException(exceptionMessage, null, Path, lineInfo.LineNumber, lineInfo.LinePosition));
        }

        private void OnValidationEvent(JsonSchemaException exception)
        {
            ValidationEventHandler handler = ValidationEventHandler;
            if (handler != null)
            {
                handler(this, new ValidationEventArgs(exception));
            }
            else
            {
                throw exception;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValidatingReader"/> class that
        /// validates the content returned from the given <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from while validating.</param>
        public JsonValidatingReader(JsonReader reader)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            _reader = reader;
            _stack = new Stack<SchemaScope>();
        }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        public JsonSchema Schema
        {
            get { return _schema; }
            set
            {
                if (TokenType != JsonToken.None)
                {
                    throw new InvalidOperationException("Cannot change schema while validating JSON.");
                }

                _schema = value;
                _model = null;
            }
        }

        /// <summary>
        /// Gets the <see cref="JsonReader"/> used to construct this <see cref="JsonValidatingReader"/>.
        /// </summary>
        /// <value>The <see cref="JsonReader"/> specified in the constructor.</value>
        public JsonReader Reader
        {
            get { return _reader; }
        }

        /// <summary>
        /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
        /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the underlying <see cref="JsonReader"/> is also closed.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (CloseInput && _reader != null)
            {
                _reader.Close();
            }
        }

        private void ValidateNotDisallowed(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            JsonSchemaType? currentNodeType = GetCurrentNodeSchemaType();
            if (currentNodeType != null)
            {
                if (JsonSchemaGenerator.HasFlag(schema.Disallow, currentNodeType.GetValueOrDefault()))
                {
                    RaiseError("Type {0} is disallowed.".FormatWith(CultureInfo.InvariantCulture, currentNodeType), schema);
                }
            }
        }

        private JsonSchemaType? GetCurrentNodeSchemaType()
        {
            switch (_reader.TokenType)
            {
                case JsonToken.StartObject:
                    return JsonSchemaType.Object;
                case JsonToken.StartArray:
                    return JsonSchemaType.Array;
                case JsonToken.Integer:
                    return JsonSchemaType.Integer;
                case JsonToken.Float:
                    return JsonSchemaType.Float;
                case JsonToken.String:
                    return JsonSchemaType.String;
                case JsonToken.Boolean:
                    return JsonSchemaType.Boolean;
                case JsonToken.Null:
                    return JsonSchemaType.Null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Int32"/>.</returns>
        public override int? ReadAsInt32()
        {
            int? i = _reader.ReadAsInt32();

            ValidateCurrentToken();
            return i;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Byte"/>[].
        /// </summary>
        /// <returns>
        /// A <see cref="Byte"/>[] or <c>null</c> if the next JSON token is null.
        /// </returns>
        public override byte[] ReadAsBytes()
        {
            byte[] data = _reader.ReadAsBytes();

            ValidateCurrentToken();
            return data;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Decimal"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Decimal"/>.</returns>
        public override decimal? ReadAsDecimal()
        {
            decimal? d = _reader.ReadAsDecimal();

            ValidateCurrentToken();
            return d;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Double"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Double"/>.</returns>
        public override double? ReadAsDouble()
        {
            double? d = _reader.ReadAsDouble();

            ValidateCurrentToken();
            return d;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="Boolean"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="Boolean"/>.</returns>
        public override bool? ReadAsBoolean()
        {
            bool? b = _reader.ReadAsBoolean();

            ValidateCurrentToken();
            return b;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override string ReadAsString()
        {
            string s = _reader.ReadAsString();

            ValidateCurrentToken();
            return s;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override DateTime? ReadAsDateTime()
        {
            DateTime? dateTime = _reader.ReadAsDateTime();

            ValidateCurrentToken();
            return dateTime;
        }

#if !NET20
        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.</returns>
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            DateTimeOffset? dateTimeOffset = _reader.ReadAsDateTimeOffset();

            ValidateCurrentToken();
            return dateTimeOffset;
        }
#endif

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="JsonReader"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            if (!_reader.Read())
            {
                return false;
            }

            if (_reader.TokenType == JsonToken.Comment)
            {
                return true;
            }

            ValidateCurrentToken();
            return true;
        }

        private void ValidateCurrentToken()
        {
            // first time validate has been called. build model
            if (_model == null)
            {
                JsonSchemaModelBuilder builder = new JsonSchemaModelBuilder();
                _model = builder.Build(_schema);

                if (!JsonTokenUtils.IsStartToken(_reader.TokenType))
                {
                    Push(new SchemaScope(JTokenType.None, CurrentMemberSchemas));
                }
            }

            switch (_reader.TokenType)
            {
                case JsonToken.StartObject:
                    ProcessValue();
                    IList<JsonSchemaModel> objectSchemas = CurrentMemberSchemas.Where(ValidateObject).ToList();
                    Push(new SchemaScope(JTokenType.Object, objectSchemas));
                    WriteToken(CurrentSchemas);
                    break;
                case JsonToken.StartArray:
                    ProcessValue();
                    IList<JsonSchemaModel> arraySchemas = CurrentMemberSchemas.Where(ValidateArray).ToList();
                    Push(new SchemaScope(JTokenType.Array, arraySchemas));
                    WriteToken(CurrentSchemas);
                    break;
                case JsonToken.StartConstructor:
                    ProcessValue();
                    Push(new SchemaScope(JTokenType.Constructor, null));
                    WriteToken(CurrentSchemas);
                    break;
                case JsonToken.PropertyName:
                    WriteToken(CurrentSchemas);
                    foreach (JsonSchemaModel schema in CurrentSchemas)
                    {
                        ValidatePropertyName(schema);
                    }
                    break;
                case JsonToken.Raw:
                    ProcessValue();
                    break;
                case JsonToken.Integer:
                    ProcessValue();
                    WriteToken(CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in CurrentMemberSchemas)
                    {
                        ValidateInteger(schema);
                    }
                    break;
                case JsonToken.Float:
                    ProcessValue();
                    WriteToken(CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in CurrentMemberSchemas)
                    {
                        ValidateFloat(schema);
                    }
                    break;
                case JsonToken.String:
                    ProcessValue();
                    WriteToken(CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in CurrentMemberSchemas)
                    {
                        ValidateString(schema);
                    }
                    break;
                case JsonToken.Boolean:
                    ProcessValue();
                    WriteToken(CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in CurrentMemberSchemas)
                    {
                        ValidateBoolean(schema);
                    }
                    break;
                case JsonToken.Null:
                    ProcessValue();
                    WriteToken(CurrentMemberSchemas);
                    foreach (JsonSchemaModel schema in CurrentMemberSchemas)
                    {
                        ValidateNull(schema);
                    }
                    break;
                case JsonToken.EndObject:
                    WriteToken(CurrentSchemas);
                    foreach (JsonSchemaModel schema in CurrentSchemas)
                    {
                        ValidateEndObject(schema);
                    }
                    Pop();
                    break;
                case JsonToken.EndArray:
                    WriteToken(CurrentSchemas);
                    foreach (JsonSchemaModel schema in CurrentSchemas)
                    {
                        ValidateEndArray(schema);
                    }
                    Pop();
                    break;
                case JsonToken.EndConstructor:
                    WriteToken(CurrentSchemas);
                    Pop();
                    break;
                case JsonToken.Undefined:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    // these have no equivalent in JSON schema
                    WriteToken(CurrentMemberSchemas);
                    break;
                case JsonToken.None:
                    // no content, do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteToken(IList<JsonSchemaModel> schemas)
        {
            foreach (SchemaScope schemaScope in _stack)
            {
                bool isInUniqueArray = (schemaScope.TokenType == JTokenType.Array && schemaScope.IsUniqueArray && schemaScope.ArrayItemCount > 0);

                if (isInUniqueArray || schemas.Any(s => s.Enum != null))
                {
                    if (schemaScope.CurrentItemWriter == null)
                    {
                        if (JsonTokenUtils.IsEndToken(_reader.TokenType))
                        {
                            continue;
                        }

                        schemaScope.CurrentItemWriter = new JTokenWriter();
                    }

                    schemaScope.CurrentItemWriter.WriteToken(_reader, false);

                    // finished writing current item
                    if (schemaScope.CurrentItemWriter.Top == 0 && _reader.TokenType != JsonToken.PropertyName)
                    {
                        JToken finishedItem = schemaScope.CurrentItemWriter.Token;

                        // start next item with new writer
                        schemaScope.CurrentItemWriter = null;

                        if (isInUniqueArray)
                        {
                            if (schemaScope.UniqueArrayItems.Contains(finishedItem, JToken.EqualityComparer))
                            {
                                RaiseError("Non-unique array item at index {0}.".FormatWith(CultureInfo.InvariantCulture, schemaScope.ArrayItemCount - 1), schemaScope.Schemas.First(s => s.UniqueItems));
                            }

                            schemaScope.UniqueArrayItems.Add(finishedItem);
                        }
                        else if (schemas.Any(s => s.Enum != null))
                        {
                            foreach (JsonSchemaModel schema in schemas)
                            {
                                if (schema.Enum != null)
                                {
                                    if (!schema.Enum.ContainsValue(finishedItem, JToken.EqualityComparer))
                                    {
                                        StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                                        finishedItem.WriteTo(new JsonTextWriter(sw));

                                        RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, sw.ToString()), schema);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ValidateEndObject(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            Dictionary<string, bool> requiredProperties = _currentScope.RequiredProperties;

            if (requiredProperties != null)
            {
                List<string> unmatchedRequiredProperties =
                    requiredProperties.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

                if (unmatchedRequiredProperties.Count > 0)
                {
                    RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", unmatchedRequiredProperties.ToArray())), schema);
                }
            }
        }

        private void ValidateEndArray(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            int arrayItemCount = _currentScope.ArrayItemCount;

            if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
            {
                RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), schema);
            }

            if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
            {
                RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), schema);
            }
        }

        private void ValidateNull(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!TestType(schema, JsonSchemaType.Null))
            {
                return;
            }

            ValidateNotDisallowed(schema);
        }

        private void ValidateBoolean(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!TestType(schema, JsonSchemaType.Boolean))
            {
                return;
            }

            ValidateNotDisallowed(schema);
        }

        private void ValidateString(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!TestType(schema, JsonSchemaType.String))
            {
                return;
            }

            ValidateNotDisallowed(schema);

            string value = _reader.Value.ToString();

            if (schema.MaximumLength != null && value.Length > schema.MaximumLength)
            {
                RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MaximumLength), schema);
            }

            if (schema.MinimumLength != null && value.Length < schema.MinimumLength)
            {
                RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MinimumLength), schema);
            }

            if (schema.Patterns != null)
            {
                foreach (string pattern in schema.Patterns)
                {
                    if (!Regex.IsMatch(value, pattern))
                    {
                        RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, value, pattern), schema);
                    }
                }
            }
        }

        private void ValidateInteger(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!TestType(schema, JsonSchemaType.Integer))
            {
                return;
            }

            ValidateNotDisallowed(schema);

            object value = _reader.Value;

            if (schema.Maximum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Maximum) > 0)
                {
                    RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
                }
                if (schema.ExclusiveMaximum && JValue.Compare(JTokenType.Integer, value, schema.Maximum) == 0)
                {
                    RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
                }
            }

            if (schema.Minimum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Minimum) < 0)
                {
                    RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
                }
                if (schema.ExclusiveMinimum && JValue.Compare(JTokenType.Integer, value, schema.Minimum) == 0)
                {
                    RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
                }
            }

            if (schema.DivisibleBy != null)
            {
                bool notDivisible;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_1
                if (value is BigInteger)
                {
                    // not that this will lose any decimal point on DivisibleBy
                    // so manually raise an error if DivisibleBy is not an integer and value is not zero
                    BigInteger i = (BigInteger)value;
                    bool divisibleNonInteger = !Math.Abs(schema.DivisibleBy.Value - Math.Truncate(schema.DivisibleBy.Value)).Equals(0);
                    if (divisibleNonInteger)
                    {
                        notDivisible = i != 0;
                    }
                    else
                    {
                        notDivisible = i % new BigInteger(schema.DivisibleBy.Value) != 0;
                    }
                }
                else
#endif
                {
                    notDivisible = !IsZero(Convert.ToInt64(value, CultureInfo.InvariantCulture) % schema.DivisibleBy.GetValueOrDefault());
                }

                if (notDivisible)
                {
                    RaiseError("Integer {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.DivisibleBy), schema);
                }
            }
        }

        private void ProcessValue()
        {
            if (_currentScope != null && _currentScope.TokenType == JTokenType.Array)
            {
                _currentScope.ArrayItemCount++;

                foreach (JsonSchemaModel currentSchema in CurrentSchemas)
                {
                    // if there is positional validation and the array index is past the number of item validation schemas and there is no additonal items then error
                    if (currentSchema != null
                        && currentSchema.PositionalItemsValidation
                        && !currentSchema.AllowAdditionalItems
                        && (currentSchema.Items == null || _currentScope.ArrayItemCount - 1 >= currentSchema.Items.Count))
                    {
                        RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, _currentScope.ArrayItemCount), currentSchema);
                    }
                }
            }
        }

        private void ValidateFloat(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            if (!TestType(schema, JsonSchemaType.Float))
            {
                return;
            }

            ValidateNotDisallowed(schema);

            double value = Convert.ToDouble(_reader.Value, CultureInfo.InvariantCulture);

            if (schema.Maximum != null)
            {
                if (value > schema.Maximum)
                {
                    RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), schema);
                }
                if (schema.ExclusiveMaximum && value == schema.Maximum)
                {
                    RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), schema);
                }
            }

            if (schema.Minimum != null)
            {
                if (value < schema.Minimum)
                {
                    RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), schema);
                }
                if (schema.ExclusiveMinimum && value == schema.Minimum)
                {
                    RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), schema);
                }
            }

            if (schema.DivisibleBy != null)
            {
                double remainder = FloatingPointRemainder(value, schema.DivisibleBy.GetValueOrDefault());

                if (!IsZero(remainder))
                {
                    RaiseError("Float {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.DivisibleBy), schema);
                }
            }
        }

        private static double FloatingPointRemainder(double dividend, double divisor)
        {
            return dividend - Math.Floor(dividend / divisor) * divisor;
        }

        private static bool IsZero(double value)
        {
            const double epsilon = 2.2204460492503131e-016;

            return Math.Abs(value) < 20.0 * epsilon;
        }

        private void ValidatePropertyName(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return;
            }

            string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);

            if (_currentScope.RequiredProperties.ContainsKey(propertyName))
            {
                _currentScope.RequiredProperties[propertyName] = true;
            }

            if (!schema.AllowAdditionalProperties)
            {
                bool propertyDefinied = IsPropertyDefinied(schema, propertyName);

                if (!propertyDefinied)
                {
                    RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, propertyName), schema);
                }
            }

            _currentScope.CurrentPropertyName = propertyName;
        }

        private bool IsPropertyDefinied(JsonSchemaModel schema, string propertyName)
        {
            if (schema.Properties != null && schema.Properties.ContainsKey(propertyName))
            {
                return true;
            }

            if (schema.PatternProperties != null)
            {
                foreach (string pattern in schema.PatternProperties.Keys)
                {
                    if (Regex.IsMatch(propertyName, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ValidateArray(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return true;
            }

            return (TestType(schema, JsonSchemaType.Array));
        }

        private bool ValidateObject(JsonSchemaModel schema)
        {
            if (schema == null)
            {
                return true;
            }

            return (TestType(schema, JsonSchemaType.Object));
        }

        private bool TestType(JsonSchemaModel currentSchema, JsonSchemaType currentType)
        {
            if (!JsonSchemaGenerator.HasFlag(currentSchema.Type, currentType))
            {
                RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, currentSchema.Type, currentType), currentSchema);
                return false;
            }

            return true;
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            IJsonLineInfo lineInfo = _reader as IJsonLineInfo;
            return lineInfo != null && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                IJsonLineInfo lineInfo = _reader as IJsonLineInfo;
                return (lineInfo != null) ? lineInfo.LineNumber : 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                IJsonLineInfo lineInfo = _reader as IJsonLineInfo;
                return (lineInfo != null) ? lineInfo.LinePosition : 0;
            }
        }
    }
}