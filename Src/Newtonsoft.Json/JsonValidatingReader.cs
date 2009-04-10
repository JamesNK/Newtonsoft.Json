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
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Newtonsoft.Json
{
  public class JsonValidatingReader : JsonReader, IJsonLineInfo
  {
    private class SchemaScope
    {
      private readonly JTokenType _tokenType;
      private readonly JsonSchemaModel _schema;
      private readonly Dictionary<string, bool> _requiredProperties;

      public string CurrentPropertyName { get; set; }
      public int ArrayItemCount { get; set; }

      public JsonSchemaModel Schema
      {
        get { return _schema; }
      }

      public Dictionary<string, bool> RequiredProperties
      {
        get { return _requiredProperties; }
      }

      public JTokenType TokenType
      {
        get { return _tokenType; }
      }

      public SchemaScope(JTokenType tokenType, JsonSchemaModel schema)
      {
        _tokenType = tokenType;
        _schema = schema;

        if (_schema != null && _schema.Properties != null)
        {
          _requiredProperties = GetRequiredProperties(_schema).Distinct().ToDictionary(p => p, p => false);
        }
      }

      private IEnumerable<string> GetRequiredProperties(JsonSchemaModel schema)
      {
        return schema.Properties.Where(p => !p.Value.Optional).Select(p => p.Key);
        //if (schema == null)
        //  return Enumerable.Empty<string>();

        //IEnumerable<string> extendedRequiredProperties = GetRequiredProperties(schema.Extends);

        //if (_schema.Properties == null)
        //  return extendedRequiredProperties;

        //return extendedRequiredProperties.Union(schema.Properties.Where(p => !(p.Value.Optional ?? false)).Select(p => p.Key));
      }
    }

    private readonly JsonReader _reader;
    private readonly Stack<SchemaScope> _stack;
    private JsonSchema _schema;
    private JsonSchemaModel _model;
    private SchemaScope _currentScope;

    public event ValidationEventHandler ValidationEventHandler;

    public override object Value
    {
      get { return _reader.Value; }
    }

    public override int Depth
    {
      get { return _reader.Depth; }
    }

    public override char QuoteChar
    {
      get { return _reader.QuoteChar; }
      protected internal set { }
    }

    public override JsonToken TokenType
    {
      get { return _reader.TokenType; }
    }

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

    private JsonSchemaModel CurrentSchema
    {
      get { return _currentScope.Schema; }
    }

    private JsonSchemaModel CurrentMemberSchema
    {
      get
      {
        if (_currentScope == null)
          return _model;

        if (_currentScope.Schema == null)
          return null;

        switch (_currentScope.TokenType)
        {
          case JTokenType.None:
            return _currentScope.Schema;
          case JTokenType.Object:
            if (_currentScope.CurrentPropertyName == null)
              throw new Exception("CurrentPropertyName has not been set on scope.");

            JsonSchemaModel propertySchema;
            if (CurrentSchema.Properties.TryGetValue(_currentScope.CurrentPropertyName, out propertySchema))
              return propertySchema;

            return (CurrentSchema.AllowAdditionalProperties) ? CurrentSchema.AdditionalProperties : null;
          case JTokenType.Array:
            if (!CollectionUtils.IsNullOrEmpty(CurrentSchema.Items))
            {
              if (CurrentSchema.Items.Count == 1)
                return CurrentSchema.Items[0];

              if (CurrentSchema.Items.Count > (_currentScope.ArrayItemCount - 1))
                return CurrentSchema.Items[_currentScope.ArrayItemCount - 1];
            }

            return (CurrentSchema.AllowAdditionalProperties) ? CurrentSchema.AdditionalProperties : null;
          case JTokenType.Constructor:
            return null;
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

      OnValidationEvent(new JsonSchemaException(exceptionMessage, null, lineInfo.LineNumber, lineInfo.LinePosition));
    }

    private void OnValidationEvent(JsonSchemaException exception)
    {
      ValidationEventHandler handler = ValidationEventHandler;
      if (handler != null)
        handler(this, new ValidationEventArgs(exception));
      else
        throw exception;
    }

    public JsonValidatingReader(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");
      _reader = reader;
      _stack = new Stack<SchemaScope>();
    }

    public JsonSchema Schema
    {
      get { return _schema; }
      set
      {
        if (TokenType != JsonToken.None)
          throw new Exception("Cannot change schema while validating JSON.");

        _schema = value;
        _model = null;
      }
    }

    public JsonReader Reader
    {
      get { return _reader; }
    }

    private void ValidateInEnumAndNotDisallowed(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      JToken value = new JValue(_reader.Value);

      if (schema.Enum != null)
      {
        if (!schema.Enum.Contains(value, new JTokenEqualityComparer()))
          RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, value),
                     schema);
      }

      JsonSchemaType? currentNodeType = GetCurrentNodeSchemaType();
      if (currentNodeType != null)
      {
        if (JsonSchemaGenerator.HasFlag(schema.Disallow, currentNodeType.Value))
          RaiseError("Type {0} is disallowed.".FormatWith(CultureInfo.InvariantCulture, currentNodeType), schema);
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

    public override bool Read()
    {
      if (!_reader.Read())
        return false;

      if (_reader.TokenType == JsonToken.Comment)
        return true;

      // first time Read has been called. build model
      if (_model == null)
      {
        JsonSchemaModelBuilder builder = new JsonSchemaModelBuilder();
        _model = builder.Build(_schema);
      }

      switch (_reader.TokenType)
      {
        case JsonToken.StartObject:
          ProcessValue();
          JsonSchemaModel objectSchema = (ValidateObject(CurrentMemberSchema))
            ? CurrentMemberSchema 
            : null;
          Push(new SchemaScope(JTokenType.Object, objectSchema));
          break;
        case JsonToken.StartArray:
          ProcessValue();
          JsonSchemaModel arraySchema = (ValidateArray(CurrentMemberSchema))
            ? CurrentMemberSchema
            : null;
          Push(new SchemaScope(JTokenType.Array, arraySchema));
          break;
        case JsonToken.StartConstructor:
          Push(new SchemaScope(JTokenType.Constructor, null));
          break;
        case JsonToken.PropertyName:
          ValidatePropertyName(CurrentSchema);
          break;
        case JsonToken.Raw:
          break;
        case JsonToken.Integer:
          ProcessValue();
          ValidateInteger(CurrentMemberSchema);
          break;
        case JsonToken.Float:
          ProcessValue();
          ValidateFloat(CurrentMemberSchema);
          break;
        case JsonToken.String:
          ProcessValue();
          ValidateString(CurrentMemberSchema);
          break;
        case JsonToken.Boolean:
          ProcessValue();
          ValidateBoolean(CurrentMemberSchema);
          break;
        case JsonToken.Null:
          ProcessValue();
          ValidateNull(CurrentMemberSchema);
          break;
        case JsonToken.Undefined:
          break;
        case JsonToken.EndObject:
          ValidateEndObject(CurrentSchema);
          Pop();
          break;
        case JsonToken.EndArray:
          ValidateEndArray(CurrentSchema);
          Pop();
          break;
        case JsonToken.EndConstructor:
          Pop();
          break;
        case JsonToken.Date:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return true;
    }

    private void ValidateEndObject(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      Dictionary<string, bool> requiredProperties = _currentScope.RequiredProperties;

      if (requiredProperties != null)
      {
        List<string> unmatchedRequiredProperties =
          requiredProperties.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

        if (unmatchedRequiredProperties.Count > 0)
          RaiseError("Non-optional properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", unmatchedRequiredProperties.ToArray())), schema);
      }
    }

    private void ValidateEndArray(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      int arrayItemCount = _currentScope.ArrayItemCount;

      if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
        RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), schema);

      if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
        RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), schema);
    }

    private void ValidateNull(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      if (!TestType(schema, JsonSchemaType.Null))
        return;

      ValidateInEnumAndNotDisallowed(schema);
    }

    private void ValidateBoolean(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      if (!TestType(schema, JsonSchemaType.Boolean))
        return;

      ValidateInEnumAndNotDisallowed(schema);
    }

    private void ValidateString(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      if (!TestType(schema, JsonSchemaType.String))
        return;

      ValidateInEnumAndNotDisallowed(schema);

      string value = _reader.Value.ToString();

      if (schema.MaximumLength != null && value.Length > schema.MaximumLength)
        RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MaximumLength), schema);

      if (schema.MinimumLength != null && value.Length < schema.MinimumLength)
        RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MinimumLength), schema);

      if (schema.Patterns != null)
      {
        foreach (string pattern in schema.Patterns)
        {
          if (!Regex.IsMatch(value, pattern))
            RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, value, pattern), schema);
        }
      }
    }

    private void ValidateInteger(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      if (!TestType(schema, JsonSchemaType.Integer))
        return;

      ValidateInEnumAndNotDisallowed(schema);
      
      long value = Convert.ToInt64(_reader.Value, CultureInfo.InvariantCulture);

      if (schema.Maximum != null && value > schema.Maximum)
        RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);

      if (schema.Minimum != null && value < schema.Minimum)
        RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
    }

    private void ProcessValue()
    {
      if (_currentScope != null && _currentScope.TokenType == JTokenType.Array)
      {
        _currentScope.ArrayItemCount++;

        if (CurrentSchema != null && CurrentSchema.Items != null && CurrentSchema.Items.Count > 1 && _currentScope.ArrayItemCount >= CurrentSchema.Items.Count)
          RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, _currentScope.ArrayItemCount), CurrentSchema);
      }
    }

    private void ValidateFloat(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      if (!TestType(schema, JsonSchemaType.Float))
        return;

      ValidateInEnumAndNotDisallowed(schema);
      
      double value = Convert.ToDouble(_reader.Value);

      if (schema.Maximum != null && value > schema.Maximum)
        RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), schema);

      if (schema.Minimum != null && value < schema.Minimum)
        RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), schema);

      if (schema.MaximumDecimals != null && MathUtils.GetDecimalPlaces(value) > schema.MaximumDecimals)
        RaiseError("Float {0} exceeds the maximum allowed number decimal places of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.MaximumDecimals), schema);
    }

    private void ValidatePropertyName(JsonSchemaModel schema)
    {
      if (schema == null)
        return;

      string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);

      if (_currentScope.RequiredProperties.ContainsKey(propertyName))
        _currentScope.RequiredProperties[propertyName] = true;

      if (!schema.Properties.ContainsKey(propertyName))
      {
        IList<string> definedProperties = schema.Properties.Select(p => p.Key).ToList();

        if (!schema.AllowAdditionalProperties && !definedProperties.Contains(propertyName))
        {
          RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, propertyName), schema);
        }
      }

      _currentScope.CurrentPropertyName = propertyName;
    }

    private bool ValidateArray(JsonSchemaModel schema)
    {
      if (schema == null)
        return true;

      return (TestType(schema, JsonSchemaType.Array));
    }

    private bool ValidateObject(JsonSchemaModel schema)
    {
      if (schema == null)
        return true;

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
      return (lineInfo != null) ? lineInfo.HasLineInfo() : false;
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