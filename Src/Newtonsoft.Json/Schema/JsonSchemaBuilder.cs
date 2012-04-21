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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Globalization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Schema
{
  internal class JsonSchemaBuilder
  {
    private JsonReader _reader;
    private readonly IList<JsonSchema> _stack;
    private readonly JsonSchemaResolver _resolver;
    private JsonSchema _currentSchema;

    private void Push(JsonSchema value)
    {
      _currentSchema = value;
      _stack.Add(value);
      _resolver.LoadedSchemas.Add(value);
    }

    private JsonSchema Pop()
    {
      JsonSchema poppedSchema = _currentSchema;
      _stack.RemoveAt(_stack.Count - 1);
      _currentSchema = _stack.LastOrDefault();

      return poppedSchema;
    }

    private JsonSchema CurrentSchema
    {
      get { return _currentSchema; }
    }

    public JsonSchemaBuilder(JsonSchemaResolver resolver)
    {
      _stack = new List<JsonSchema>();
      _resolver = resolver;
    }

    internal JsonSchema Parse(JsonReader reader)
    {
      _reader = reader;

      if (reader.TokenType == JsonToken.None)
        _reader.Read();

      return BuildSchema();
    }

    private JsonSchema BuildSchema()
    {
      if (_reader.TokenType != JsonToken.StartObject)
        throw JsonReaderException.Create(_reader, "Expected StartObject while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

      _reader.Read();
      // empty schema object
      if (_reader.TokenType == JsonToken.EndObject)
      {
        Push(new JsonSchema());
        return Pop();
      }

      string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);
      _reader.Read();
      
      // schema reference
      if (propertyName == JsonSchemaConstants.ReferencePropertyName)
      {
        string id = (string)_reader.Value;

        // skip to the end of the current object
        while (_reader.Read() && _reader.TokenType != JsonToken.EndObject)
        {
            if (_reader.TokenType == JsonToken.StartObject)
              throw JsonReaderException.Create(_reader, "Found StartObject within the schema reference with the Id '{0}'".FormatWith(CultureInfo.InvariantCulture, id));
        }

        JsonSchema referencedSchema = _resolver.GetSchema(id);
        if (referencedSchema == null)
          throw new JsonException("Could not resolve schema reference for Id '{0}'.".FormatWith(CultureInfo.InvariantCulture, id));

        return referencedSchema;
      }

      // regular ol' schema object
      Push(new JsonSchema());

      ProcessSchemaProperty(propertyName);

      while (_reader.Read() && _reader.TokenType != JsonToken.EndObject)
      {
        propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);
        _reader.Read();

        ProcessSchemaProperty(propertyName);
      }

      return Pop();
    }

    private void ProcessSchemaProperty(string propertyName)
    {
      switch (propertyName)
      {
        case JsonSchemaConstants.TypePropertyName:
          CurrentSchema.Type = ProcessType();
          break;
        case JsonSchemaConstants.IdPropertyName:
          CurrentSchema.Id = (string) _reader.Value;
          break;
        case JsonSchemaConstants.TitlePropertyName:
          CurrentSchema.Title = (string) _reader.Value;
          break;
        case JsonSchemaConstants.DescriptionPropertyName:
          CurrentSchema.Description = (string)_reader.Value;
          break;
        case JsonSchemaConstants.PropertiesPropertyName:
          ProcessProperties();
          break;
        case JsonSchemaConstants.ItemsPropertyName:
          ProcessItems();
          break;
        case JsonSchemaConstants.AdditionalPropertiesPropertyName:
          ProcessAdditionalProperties();
          break;
        case JsonSchemaConstants.PatternPropertiesPropertyName:
          ProcessPatternProperties();
          break;
        case JsonSchemaConstants.RequiredPropertyName:
          CurrentSchema.Required = (bool)_reader.Value;
          break;
        case JsonSchemaConstants.RequiresPropertyName:
          CurrentSchema.Requires = (string) _reader.Value;
          break;
        case JsonSchemaConstants.IdentityPropertyName:
          ProcessIdentity();
          break;
        case JsonSchemaConstants.MinimumPropertyName:
          CurrentSchema.Minimum = Convert.ToDouble(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.MaximumPropertyName:
          CurrentSchema.Maximum = Convert.ToDouble(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.ExclusiveMinimumPropertyName:
          CurrentSchema.ExclusiveMinimum = (bool)_reader.Value;
          break;
        case JsonSchemaConstants.ExclusiveMaximumPropertyName:
          CurrentSchema.ExclusiveMaximum = (bool)_reader.Value;
          break;
        case JsonSchemaConstants.MaximumLengthPropertyName:
          CurrentSchema.MaximumLength = Convert.ToInt32(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.MinimumLengthPropertyName:
          CurrentSchema.MinimumLength = Convert.ToInt32(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.MaximumItemsPropertyName:
          CurrentSchema.MaximumItems = Convert.ToInt32(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.MinimumItemsPropertyName:
          CurrentSchema.MinimumItems = Convert.ToInt32(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.DivisibleByPropertyName:
          CurrentSchema.DivisibleBy = Convert.ToDouble(_reader.Value, CultureInfo.InvariantCulture);
          break;
        case JsonSchemaConstants.DisallowPropertyName:
          CurrentSchema.Disallow = ProcessType();
          break;
        case JsonSchemaConstants.DefaultPropertyName:
          ProcessDefault();
          break;
        case JsonSchemaConstants.HiddenPropertyName:
          CurrentSchema.Hidden = (bool) _reader.Value;
          break;
        case JsonSchemaConstants.ReadOnlyPropertyName:
          CurrentSchema.ReadOnly = (bool) _reader.Value;
          break;
        case JsonSchemaConstants.FormatPropertyName:
          CurrentSchema.Format = (string) _reader.Value;
          break;
        case JsonSchemaConstants.PatternPropertyName:
          CurrentSchema.Pattern = (string) _reader.Value;
          break;
        case JsonSchemaConstants.OptionsPropertyName:
          ProcessOptions();
          break;
        case JsonSchemaConstants.EnumPropertyName:
          ProcessEnum();
          break;
        case JsonSchemaConstants.ExtendsPropertyName:
          ProcessExtends();
          break;
        default:
          _reader.Skip();
          break;
      }
    }

    private void ProcessExtends()
    {
      CurrentSchema.Extends = BuildSchema();
    }

    private void ProcessEnum()
    {
      if (_reader.TokenType != JsonToken.StartArray)
        throw JsonReaderException.Create(_reader, "Expected StartArray token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

      CurrentSchema.Enum = new List<JToken>();

      while (_reader.Read() && _reader.TokenType != JsonToken.EndArray)
      {
        JToken value = JToken.ReadFrom(_reader);
        CurrentSchema.Enum.Add(value);
      }
    }

    private void ProcessOptions()
    {
      CurrentSchema.Options = new Dictionary<JToken, string>(new JTokenEqualityComparer());

      switch (_reader.TokenType)
      {
        case JsonToken.StartArray:
          while (_reader.Read() && _reader.TokenType != JsonToken.EndArray)
          {
            if (_reader.TokenType != JsonToken.StartObject)
              throw JsonReaderException.Create(_reader, "Expect object token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

            string label = null;
            JToken value = null;

            while (_reader.Read() && _reader.TokenType != JsonToken.EndObject)
            {
              string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);
              _reader.Read();

              switch (propertyName)
              {
                case JsonSchemaConstants.OptionValuePropertyName:
                  value = JToken.ReadFrom(_reader);
                  break;
                case JsonSchemaConstants.OptionLabelPropertyName:
                  label = (string) _reader.Value;
                  break;
                default:
                  throw JsonReaderException.Create(_reader, "Unexpected property in JSON schema option: {0}.".FormatWith(CultureInfo.InvariantCulture, propertyName));
              }
            }

            if (value == null)
              throw new JsonException("No value specified for JSON schema option.");

            if (CurrentSchema.Options.ContainsKey(value))
              throw new JsonException("Duplicate value in JSON schema option collection: {0}".FormatWith(CultureInfo.InvariantCulture, value));

            CurrentSchema.Options.Add(value, label);
          }
          break;
        default:
          throw JsonReaderException.Create(_reader, "Expected array token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));
      }
    }

    private void ProcessDefault()
    {
      CurrentSchema.Default = JToken.ReadFrom(_reader);
    }

    private void ProcessIdentity()
    {
      CurrentSchema.Identity = new List<string>();

      switch (_reader.TokenType)
      {
        case JsonToken.String:
          CurrentSchema.Identity.Add(_reader.Value.ToString());
          break;
        case JsonToken.StartArray:
          while (_reader.Read() && _reader.TokenType != JsonToken.EndArray)
          {
            if (_reader.TokenType != JsonToken.String)
              throw JsonReaderException.Create(_reader, "Exception JSON property name string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

            CurrentSchema.Identity.Add(_reader.Value.ToString());
          }
          break;
        default:
          throw JsonReaderException.Create(_reader, "Expected array or JSON property name string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));
      }
    }

    private void ProcessAdditionalProperties()
    {
      if (_reader.TokenType == JsonToken.Boolean)
        CurrentSchema.AllowAdditionalProperties = (bool)_reader.Value;
      else
        CurrentSchema.AdditionalProperties = BuildSchema();
    }

    private void ProcessPatternProperties()
    {
      Dictionary<string, JsonSchema> patternProperties = new Dictionary<string, JsonSchema>();

      if (_reader.TokenType != JsonToken.StartObject)
        throw JsonReaderException.Create(_reader, "Expected StartObject token.");

      while (_reader.Read() && _reader.TokenType != JsonToken.EndObject)
      {
        string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);
        _reader.Read();

        if (patternProperties.ContainsKey(propertyName))
          throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, propertyName));

        patternProperties.Add(propertyName, BuildSchema());
      }

      CurrentSchema.PatternProperties = patternProperties;
    }

    private void ProcessItems()
    {
      CurrentSchema.Items = new List<JsonSchema>();

      switch (_reader.TokenType)
      {
        case JsonToken.StartObject:
          CurrentSchema.Items.Add(BuildSchema());
          break;
        case JsonToken.StartArray:
          while (_reader.Read() && _reader.TokenType != JsonToken.EndArray)
          {
            CurrentSchema.Items.Add(BuildSchema());
          }
          break;
        default:
          throw JsonReaderException.Create(_reader, "Expected array or JSON schema object token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));
      }
    }

    private void ProcessProperties()
    {
      IDictionary<string, JsonSchema> properties = new Dictionary<string, JsonSchema>();

      if (_reader.TokenType != JsonToken.StartObject)
        throw JsonReaderException.Create(_reader, "Expected StartObject token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

      while (_reader.Read() && _reader.TokenType != JsonToken.EndObject)
      {
        string propertyName = Convert.ToString(_reader.Value, CultureInfo.InvariantCulture);
        _reader.Read();

        if (properties.ContainsKey(propertyName))
          throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, propertyName));

        properties.Add(propertyName, BuildSchema());
      }

      CurrentSchema.Properties = properties;
    }

    private JsonSchemaType? ProcessType()
    {
      switch (_reader.TokenType)
      {
        case JsonToken.String:
          return MapType(_reader.Value.ToString());
        case JsonToken.StartArray:
          // ensure type is in blank state before ORing values
          JsonSchemaType? type = JsonSchemaType.None;

          while (_reader.Read() && _reader.TokenType != JsonToken.EndArray)
          {
            if (_reader.TokenType != JsonToken.String)
              throw JsonReaderException.Create(_reader, "Exception JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));

            type = type | MapType(_reader.Value.ToString());
          }

          return type;
        default:
          throw JsonReaderException.Create(_reader, "Expected array or JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, _reader.TokenType));
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