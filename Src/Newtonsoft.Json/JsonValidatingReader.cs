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
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
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
    /// Represents a reader that provides <see cref="JsonSchema"/> validation.
    /// </summary>
    public class JsonValidatingReader : JsonReader, IJsonLineInfo
    {
        private class SchemaScope
        {
            private readonly JTokenType _tokenType;
            private readonly IList<SchemaReport> _schemas;
            private IList<string> _locatedProperties;

            public string CurrentPropertyName { get; set; }
            public int ArrayItemCount { get; set; }
            public bool IsUniqueArray { get; set; }
            public IList<JToken> UniqueArrayItems { get; set; }
            public JTokenWriter CurrentItemWriter { get; set; }

            public IList<SchemaReport> Schemas
            {
                get { return _schemas; }
            }

            public IList<string> LocatedProperties
            {
                get 
                {
                    if (_locatedProperties == null)
                        _locatedProperties = new List<string>();

                    return _locatedProperties; 
                }
            }

            public int TotalPropertyCount
            {
                get
                {
                    if (_locatedProperties == null)
                        return 0;

                    return _locatedProperties.Count;
                }
            }

            public JTokenType TokenType
            {
                get { return _tokenType; }
            }

            public SchemaScope(JTokenType tokenType, IList<SchemaReport> schemas)
            {
                _tokenType = tokenType;
                _schemas = schemas;

                if (tokenType == JTokenType.Array && schemas.Any(s => s.Schema.UniqueItems))
                {
                    IsUniqueArray = true;
                    UniqueArrayItems = new List<JToken>();
                }
            }
        }

        private enum SchemaConditionType
        {
            AllOf,
            AnyOf,
            OneOf,
            NotOf,
            Dependency,
            Schema
        }

        private class SchemaReport
        {
            public SchemaReport Parent { get; set; }
            public IList<JsonSchemaException> Messages { get; set; }
            public string ConditionalPropertyName { get; set; }
            public SchemaConditionType ConditionType { get; private set; }
            public IList<SchemaReport> SubResults { get; private set; }
            public JsonSchema Schema { get; set; }

            public SchemaReport(JsonSchema schema, IList<JsonSchemaException> messages, SchemaConditionType type, SchemaReport parent, string conditionalPropertyName)
            {
                Schema = schema;
                Messages = messages;
                ConditionType = type;
                ConditionalPropertyName = conditionalPropertyName;
                SubResults = new List<SchemaReport>();
                if (parent != null && parent.Messages != null)
                    Parent = parent;
            }

            public bool IsValid(IList<string> locatedProperties)
            {
                switch (ConditionType)
                {
                    case SchemaConditionType.AllOf:
                        return SubResults.All(r => r.IsValid(locatedProperties));
                        
                    case SchemaConditionType.AnyOf:
                        return SubResults.Any(r => r.IsValid(locatedProperties));

                    case SchemaConditionType.OneOf:
                        return SubResults.Count(r => r.IsValid(locatedProperties)) == 1;

                    case SchemaConditionType.NotOf:
                        return !((Messages == null || Messages.Count == 0) &&
                            SubResults.All(r => r.IsValid(locatedProperties)));

                    case SchemaConditionType.Dependency:
                        return !locatedProperties.Contains(ConditionalPropertyName, StringComparer.Ordinal) ||
                            ((Messages == null || Messages.Count == 0) &&
                            SubResults.All(r => r.IsValid(locatedProperties)));

                    default:
                        return (Messages == null || Messages.Count == 0) &&
                            SubResults.All(r => r.IsValid(locatedProperties));
                }
            }
        }

        private readonly JsonReader _reader;
        private readonly Stack<SchemaScope> _stack;
        private JsonSchema _schema;
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
        /// Gets the Common Language Runtime (CLR) type for the current JSON token.
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

        private IList<SchemaReport> CurrentSchemas
        {
            get { return _currentScope.Schemas; }
        }

        private static readonly IList<SchemaReport> EmptySchemaList = new List<SchemaReport>();

        private IList<SchemaReport> CurrentMemberSchemas
        {
            get
            {
                if (_currentScope == null)
                {
                    // Simple value
                    SchemaReport simpleReport = new SchemaReport(_schema, null, SchemaConditionType.Schema, null, string.Empty);
                    List<SchemaReport> simpleSchema = new List<SchemaReport>(new[] { simpleReport });
                    AddConditionalSchemas(simpleSchema, _schema, simpleReport);
                    return simpleSchema;
                }

                if (_currentScope.Schemas == null || _currentScope.Schemas.Count == 0)
                    return EmptySchemaList;

                switch (_currentScope.TokenType)
                {
                    case JTokenType.None:
                        return _currentScope.Schemas;
                    case JTokenType.Object:
                    {
                        if (_currentScope.CurrentPropertyName == null)
                            throw new JsonReaderException("CurrentPropertyName has not been set on scope.");

                        IList<SchemaReport> schemas = new List<SchemaReport>();

                        foreach (SchemaReport report in CurrentSchemas)
                        {
                            JsonSchema schema = report.Schema;
                            JsonSchema propertySchema;
                            if (schema.Properties != null && schema.Properties.TryGetValue(_currentScope.CurrentPropertyName, out propertySchema))
                            {
                                SchemaReport propertyReport = new SchemaReport(propertySchema, report.Messages, SchemaConditionType.Schema, report, string.Empty);
                                schemas.Add(propertyReport);
                                AddConditionalSchemas(schemas, propertySchema, propertyReport);
                            }
                            if (schema.PatternProperties != null)
                            {
                                foreach (KeyValuePair<string, JsonSchema> patternProperty in schema.PatternProperties)
                                {
                                    if (Regex.IsMatch(_currentScope.CurrentPropertyName, patternProperty.Key))
                                    {
                                        schemas.Add(new SchemaReport(patternProperty.Value, report.Messages, SchemaConditionType.Schema, report, string.Empty));
                                        AddConditionalSchemas(schemas, patternProperty.Value, report);
                                    }
                                }
                            }

                            if (schemas.Count == 0 && schema.AllowAdditionalProperties && schema.AdditionalProperties != null)
                            {
                                schemas.Add(new SchemaReport(schema.AdditionalProperties, report.Messages, SchemaConditionType.Schema, report, string.Empty));
                                AddConditionalSchemas(schemas, schema.AdditionalProperties, report);
                            }
                        }

                        return schemas;
                    }
                    case JTokenType.Array:
                    {
                        IList<SchemaReport> schemas = new List<SchemaReport>();

                        foreach (SchemaReport report in CurrentSchemas)
                        {
                            JsonSchema schema = report.Schema;

                            if (!schema.PositionalItemsValidation)
                            {
                                if (schema.Items != null && schema.Items.Count > 0)
                                {
                                    schemas.Add(new SchemaReport(schema.Items[0], report.Messages, SchemaConditionType.Schema, report, string.Empty));
                                    AddConditionalSchemas(schemas, schema.Items[0], report);
                                }
                            }
                            else
                            {
                                if (schema.Items != null &&
                                    schema.Items.Count > 0 &&
                                    schema.Items.Count > (_currentScope.ArrayItemCount - 1))
                                {
                                    schemas.Add(new SchemaReport(schema.Items[_currentScope.ArrayItemCount - 1], report.Messages, SchemaConditionType.Schema, report, string.Empty));
                                    AddConditionalSchemas(schemas, schema.Items[_currentScope.ArrayItemCount - 1], report);
                                }
                                else
                                {
                                    if (schema.AllowAdditionalItems && schema.AdditionalItems != null)
                                    {
                                        schemas.Add(new SchemaReport(schema.AdditionalItems, report.Messages, SchemaConditionType.Schema, report, string.Empty));
                                        AddConditionalSchemas(schemas, schema.AdditionalItems, report);
                                    }
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

        private void AddConditionalSchemas(IList<SchemaReport> currentSchemas, JsonSchema schema, SchemaReport parentResultSet)
        {
            AddConditionalSchemas(currentSchemas, schema.AllOf, SchemaConditionType.AllOf, parentResultSet);
            AddConditionalSchemas(currentSchemas, schema.OneOf, SchemaConditionType.OneOf, parentResultSet);
            AddConditionalSchemas(currentSchemas, schema.AnyOf, SchemaConditionType.AnyOf, parentResultSet);
            if (schema.NotOf != null)
            {
                AddConditionalSchemas(currentSchemas, new List<JsonSchema> { schema.NotOf }, SchemaConditionType.NotOf, parentResultSet);
            }
            if (schema.Dependencies != null)
            {
                foreach (KeyValuePair<string, JsonSchema> dep in schema.Dependencies)
                {
                    SchemaReport childResultSet = new SchemaReport(dep.Value, new List<JsonSchemaException>(), SchemaConditionType.Dependency, parentResultSet, dep.Key);
                    currentSchemas.Add(childResultSet);
                    parentResultSet.SubResults.Add(childResultSet);
                    AddConditionalSchemas(currentSchemas, dep.Value, childResultSet);
                }
            }
        }

        private void AddConditionalSchemas(IList<SchemaReport> currentSchemas, IList<JsonSchema> schemaSet, SchemaConditionType resultType, SchemaReport parentResultSet)
        { 
            if (schemaSet != null)
            {
                SchemaReport resultSet = new SchemaReport(null, new List<JsonSchemaException>(), resultType, parentResultSet, string.Empty);
                parentResultSet.SubResults.Add(resultSet);

                foreach (JsonSchema schema in schemaSet)
                {
                    SchemaReport childResultSet = new SchemaReport(schema, new List<JsonSchemaException>(), SchemaConditionType.Schema, resultSet, string.Empty);
                    currentSchemas.Add(childResultSet);
                    resultSet.SubResults.Add(childResultSet);
                    AddConditionalSchemas(currentSchemas, schema, childResultSet);
                }
            }
        }

        private void RaiseError(string message, SchemaReport report)
        {
            IJsonLineInfo lineInfo = this;

            string exceptionMessage = (lineInfo.HasLineInfo())
                ? message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition)
                : message;

            JsonSchemaException ex = new JsonSchemaException(exceptionMessage, null, Path, lineInfo.LineNumber, lineInfo.LinePosition);

            if (report != null && report.Messages != null)
                report.Messages.Add(ex);
            else
                OnValidationEvent(ex);
        }

        private void OnValidationEvent(JsonSchemaException exception)
        {
            ValidationEventHandler handler = ValidationEventHandler;
            if (handler != null)
                handler(this, new ValidationEventArgs(exception));
            else
                throw exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValidatingReader"/> class that
        /// validates the content returned from the given <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from while validating.</param>
        public JsonValidatingReader(JsonReader reader)
        {
            ValidationUtils.ArgumentNotNull(reader, "reader");
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
                    throw new InvalidOperationException("Cannot change schema while validating JSON.");

                _schema = value;
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
        /// Reads the next JSON token from the stream as a <see cref="Nullable{Int32}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{Int32}"/>.</returns>
        public override int? ReadAsInt32()
        {
            int? i = _reader.ReadAsInt32();

            ValidateCurrentToken();
            return i;
        }

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null.
        /// </returns>
        public override byte[] ReadAsBytes()
        {
            byte[] data = _reader.ReadAsBytes();

            ValidateCurrentToken();
            return data;
        }

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{Decimal}"/>.</returns>
        public override decimal? ReadAsDecimal()
        {
            decimal? d = _reader.ReadAsDecimal();

            ValidateCurrentToken();
            return d;
        }

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override string ReadAsString()
        {
            string s = _reader.ReadAsString();

            ValidateCurrentToken();
            return s;
        }

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTime}"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public override DateTime? ReadAsDateTime()
        {
            DateTime? dateTime = _reader.ReadAsDateTime();

            ValidateCurrentToken();
            return dateTime;
        }

#if !NET20
        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{DateTimeOffset}"/>.</returns>
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            DateTimeOffset? dateTimeOffset = _reader.ReadAsDateTimeOffset();

            ValidateCurrentToken();
            return dateTimeOffset;
        }
#endif

        /// <summary>
        /// Reads the next JSON token from the stream.
        /// </summary>
        /// <returns>
        /// true if the next token was read successfully; false if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            if (!_reader.Read())
                return false;

            if (_reader.TokenType == JsonToken.Comment)
                return true;

            ValidateCurrentToken();
            return true;
        }

        private void ValidateCurrentToken()
        {
            // first time validate has been called.
            if (_stack.Count == 0)
            {
                if (!JsonWriter.IsStartToken(_reader.TokenType))
                    Push(new SchemaScope(JTokenType.None, CurrentMemberSchemas));
            }

            IList<SchemaReport> tokenSchemas;

            switch (_reader.TokenType)
            {
                case JsonToken.StartObject:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas.Where(ValidateObject).ToList();
                    Push(new SchemaScope(JTokenType.Object, tokenSchemas));
                    WriteToken(tokenSchemas);
                    break;
                case JsonToken.StartArray:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas.Where(ValidateArray).ToList();
                    Push(new SchemaScope(JTokenType.Array, tokenSchemas));
                    WriteToken(tokenSchemas);
                    break;
                case JsonToken.StartConstructor:
                    ProcessValue();
                    Push(new SchemaScope(JTokenType.Constructor, null));
                    WriteToken(CurrentSchemas);
                    break;
                case JsonToken.PropertyName:
                    WriteToken(CurrentSchemas);
                    foreach (SchemaReport report in CurrentSchemas)
                    {
                        ValidatePropertyName(report);
                    }
                    break;
                case JsonToken.Raw:
                    ProcessValue();
                    break;
                case JsonToken.Integer:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas;
                    WriteToken(tokenSchemas);
                    foreach (SchemaReport report in tokenSchemas)
                    {
                        ValidateInteger(report);
                    }
                    ValidateConditionalResults(tokenSchemas);
                    break;
                case JsonToken.Float:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas;
                    WriteToken(tokenSchemas);
                    foreach (SchemaReport report in tokenSchemas)
                    {
                        ValidateFloat(report);
                    }
                    ValidateConditionalResults(tokenSchemas);
                    break;
                case JsonToken.String:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas;
                    WriteToken(tokenSchemas);
                    foreach (SchemaReport report in tokenSchemas)
                    {
                        ValidateString(report);
                    }
                    ValidateConditionalResults(tokenSchemas);
                    break;
                case JsonToken.Boolean:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas;
                    WriteToken(tokenSchemas);
                    foreach (SchemaReport report in tokenSchemas)
                    {
                        ValidateBoolean(report);
                    }
                    ValidateConditionalResults(tokenSchemas);
                    break;
                case JsonToken.Null:
                    ProcessValue();
                    tokenSchemas = CurrentMemberSchemas;
                    WriteToken(tokenSchemas);
                    foreach (SchemaReport report in tokenSchemas)
                    {
                        ValidateNull(report);
                    }
                    ValidateConditionalResults(tokenSchemas);
                    break;
                case JsonToken.EndObject:
                    WriteToken(CurrentSchemas);
                    foreach (SchemaReport report in CurrentSchemas)
                    {
                        ValidateEndObject(report);
                    }
                    ValidateConditionalResults(CurrentSchemas);
                    Pop();
                    break;
                case JsonToken.EndArray:
                    WriteToken(CurrentSchemas);
                    foreach (SchemaReport report in CurrentSchemas)
                    {
                        ValidateEndArray(report);
                    }
                    ValidateConditionalResults(CurrentSchemas);
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

        private void WriteToken(IList<SchemaReport> schemas)
        {
            foreach (SchemaScope schemaScope in _stack)
            {
                bool isInUniqueArray = (schemaScope.TokenType == JTokenType.Array && schemaScope.IsUniqueArray && schemaScope.ArrayItemCount > 0);

                if (isInUniqueArray || schemas.Any(s => s.Schema.Enum != null))
                {
                    if (schemaScope.CurrentItemWriter == null)
                    {
                        if (JsonWriter.IsEndToken(_reader.TokenType))
                            continue;

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
                                foreach (SchemaReport report in schemaScope.Schemas.Where(s => s.Schema.UniqueItems))
                                {
                                    RaiseError("Non-unique array item at index {0}.".FormatWith(CultureInfo.InvariantCulture, schemaScope.ArrayItemCount - 1), report);
                                }
                            }

                            schemaScope.UniqueArrayItems.Add(finishedItem);
                        }
                        else if (schemas.Any(s => s.Schema.Enum != null))
                        {
                            foreach (SchemaReport report in schemas)
                            {
                                if (report.Schema.Enum != null)
                                {
                                    if (!report.Schema.Enum.ContainsValue(finishedItem, JToken.EqualityComparer))
                                    {
                                        StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                                        finishedItem.WriteTo(new JsonTextWriter(sw));

                                        RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, sw.ToString()), report);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ValidateEndObject(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            if (schema.Required != null)
            {
                List<string> unmatchedRequiredProperties = schema.Required.Except(_currentScope.LocatedProperties, StringComparer.Ordinal).ToList();

                if (unmatchedRequiredProperties.Count > 0)
                    RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", unmatchedRequiredProperties.ToArray())), report);
            }

            int objectPropertyCount = _currentScope.TotalPropertyCount;

            if (schema.MaximumProperties != null && objectPropertyCount > schema.MaximumProperties)
                RaiseError("Property count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, objectPropertyCount, schema.MaximumProperties), report);

            if (schema.MinimumProperties != null && objectPropertyCount < schema.MinimumProperties)
                RaiseError("Property count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, objectPropertyCount, schema.MinimumProperties), report);
        }

        private void ValidateEndArray(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            int arrayItemCount = _currentScope.ArrayItemCount;

            if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
                RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), report);

            if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
                RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), report);
        }

        private void ValidateNull(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            if (!TestType(report, JsonSchemaType.Null))
                return;
        }

        private void ValidateBoolean(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            if (!TestType(report, JsonSchemaType.Boolean))
                return;
        }

        private void ValidateString(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            if (!TestType(report, JsonSchemaType.String))
                return;

            string value = _reader.Value.ToString();

            if (schema.MaximumLength != null || schema.MinimumLength != null)
            {
                int[] unicodeString = StringInfo.ParseCombiningCharacters(value);

                if (schema.MaximumLength != null && unicodeString.Length > schema.MaximumLength)
                    RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MaximumLength), report);

                if (schema.MinimumLength != null && unicodeString.Length < schema.MinimumLength)
                    RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MinimumLength), report);
            }

            if (schema.Pattern != null)
            {
                if (!Regex.IsMatch(value, schema.Pattern))
                    RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, value, schema.Pattern), report);
            }
        }

        private void ValidateInteger(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            if (!TestType(report, JsonSchemaType.Integer))
                return;

            object value = _reader.Value;

            if (schema.Maximum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Maximum) > 0)
                    RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), report);
                if (schema.ExclusiveMaximum != null && (bool)schema.ExclusiveMaximum && JValue.Compare(JTokenType.Integer, value, schema.Maximum) == 0)
                    RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), report);
            }

            if (schema.Minimum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Minimum) < 0)
                    RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), report);
                if (schema.ExclusiveMinimum != null && (bool)schema.ExclusiveMinimum && JValue.Compare(JTokenType.Integer, value, schema.Minimum) == 0)
                    RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), report);
            }

            if (schema.MultipleOf != null)
            {
                bool notMultiple = false;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
                if (value is BigInteger)
                {
                    // not that this will lose any decimal point on MultipleOf
                    // so manually raise an error if MultipleOf is not an integer and value is not zero
                    BigInteger i = (BigInteger)value;
                    bool multipleNonInteger = !Math.Abs(schema.MultipleOf.Value - Math.Truncate(schema.MultipleOf.Value)).Equals(0);
                    if (multipleNonInteger)
                        notMultiple = i != 0;
                    else
                        notMultiple = i % new BigInteger(schema.MultipleOf.Value) != 0;
                }
                else
#endif
                    notMultiple = !IsZero(Convert.ToInt64(value, CultureInfo.InvariantCulture) % schema.MultipleOf.Value);

                if (notMultiple)
                    RaiseError("Integer {0} is not a multiple of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.MultipleOf), report);
            }
        }

        private void ProcessValue()
        {
            if (_currentScope != null && _currentScope.TokenType == JTokenType.Array)
            {
                _currentScope.ArrayItemCount++;

                foreach (SchemaReport report in CurrentSchemas)
                {
                    JsonSchema currentSchema = report.Schema;

                    // if there is positional validation and the array index is past the number of item validation schemas and there is no additonal items then error
                    if (currentSchema != null
                        && currentSchema.PositionalItemsValidation
                        && !currentSchema.AllowAdditionalItems
                        && (currentSchema.Items == null || _currentScope.ArrayItemCount - 1 >= currentSchema.Items.Count))
                    {
                        RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, _currentScope.ArrayItemCount), report);
                    }
                }
            }
        }

        private void ValidateFloat(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            if (!TestType(report, JsonSchemaType.Float))
                return;

            double value = Convert.ToDouble(_reader.Value, CultureInfo.InvariantCulture);

            if (schema.Maximum != null)
            {
                if (value > schema.Maximum)
                    RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), report);
                if (schema.ExclusiveMaximum != null && (bool)schema.ExclusiveMaximum && value == schema.Maximum)
                    RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), report);
            }

            if (schema.Minimum != null)
            {
                if (value < schema.Minimum)
                    RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), report);
                if (schema.ExclusiveMinimum != null && (bool)schema.ExclusiveMinimum && value == schema.Minimum)
                    RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), report);
            }

            if (schema.MultipleOf != null)
            {
                double remainder = FloatingPointRemainder(value, schema.MultipleOf.Value);

                if (!IsZero(remainder))
                    RaiseError("Float {0} is not a multiple of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.MultipleOf), report);
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

        private void ValidatePropertyName(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return;

            JsonSchema schema = report.Schema;

            string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);

            if (!_currentScope.LocatedProperties.Contains(propertyName, StringComparer.Ordinal))
                _currentScope.LocatedProperties.Add(propertyName);

            if (!schema.AllowAdditionalProperties)
            {
                bool propertyDefinied = IsPropertyDefinied(schema, propertyName);

                if (!propertyDefinied)
                    RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, propertyName), report);
            }

            _currentScope.CurrentPropertyName = propertyName;
        }

        private void ValidateConditionalResults(IList<SchemaReport> currentSchemas)
        {
            foreach (SchemaReport report in currentSchemas)
            {
                if (!report.IsValid(_currentScope.LocatedProperties))
                    RaiseError("Condition failed {0}-{1}.".FormatWith(CultureInfo.InvariantCulture, report.ConditionType, report.Schema), report.Parent);
            }
        }

        private bool IsPropertyDefinied(JsonSchema schema, string propertyName)
        {
            if (schema.Properties != null && schema.Properties.ContainsKey(propertyName))
                return true;

            if (schema.PatternProperties != null)
            {
                foreach (string pattern in schema.PatternProperties.Keys)
                {
                    if (Regex.IsMatch(propertyName, pattern))
                        return true;
                }
            }

            return false;
        }

        private bool ValidateArray(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return true;

            return (TestType(report, JsonSchemaType.Array));
        }

        private bool ValidateObject(SchemaReport report)
        {
            if (report == null || report.Schema == null)
                return true;

            return (TestType(report, JsonSchemaType.Object));
        }

        private bool TestType(SchemaReport report, JsonSchemaType currentType)
        {
            if (!JsonSchemaGenerator.HasFlag(report.Schema.Type, currentType))
            {
                RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, report.Schema.Type, currentType), report);
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