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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
  internal class JsonSchemaWriter
  {
    private readonly JsonWriter _writer;
    private readonly JsonSchemaResolver _resolver;

    public JsonSchemaWriter(JsonWriter writer, JsonSchemaResolver resolver)
    {
      ValidationUtils.ArgumentNotNull(writer, "writer");
      _writer = writer;
      _resolver = resolver;
    }

    private void ReferenceOrWriteSchema(JsonSchema schema)
    {
      if (schema.Id != null && _resolver.GetSchema(schema.Id) != null)
      {
        _writer.WriteStartObject();
        _writer.WritePropertyName(JsonSchemaConstants.ReferencePropertyName);
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
      ValidationUtils.ArgumentNotNull(schema, "schema");

      if (!_resolver.LoadedSchemas.Contains(schema))
        _resolver.LoadedSchemas.Add(schema);

      _writer.WriteStartObject();
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.IdPropertyName, schema.Id);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.TitlePropertyName, schema.Title);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.DescriptionPropertyName, schema.Description);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.RequiredPropertyName, schema.Required);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.ReadOnlyPropertyName, schema.ReadOnly);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.HiddenPropertyName, schema.Hidden);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.TransientPropertyName, schema.Transient);
      if (schema.Type != null)
        WriteType(JsonSchemaConstants.TypePropertyName, _writer, schema.Type.Value);
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
      if (schema.Properties != null)
      {
        _writer.WritePropertyName(JsonSchemaConstants.PropertiesPropertyName);
        _writer.WriteStartObject();
        foreach (KeyValuePair<string, JsonSchema> property in schema.Properties)
        {
          _writer.WritePropertyName(property.Key);
          ReferenceOrWriteSchema(property.Value);
        }
        _writer.WriteEndObject();
      }
      WriteItems(schema);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.MinimumPropertyName, schema.Minimum);
      WritePropertyIfNotNull(_writer, JsonSchemaConstants.MaximumPropertyName, schema.Maximum);
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
      if (schema.Options != null)
      {
        _writer.WritePropertyName(JsonSchemaConstants.OptionsPropertyName);
        _writer.WriteStartArray();
        foreach (KeyValuePair<JToken, string> option in schema.Options)
        {
          _writer.WriteStartObject();
          _writer.WritePropertyName(JsonSchemaConstants.OptionValuePropertyName);
          option.Key.WriteTo(_writer);
          if (option.Value != null)
          {
            _writer.WritePropertyName(JsonSchemaConstants.OptionLabelPropertyName);
            _writer.WriteValue(option.Value);
          }
          _writer.WriteEndObject();
        }
        _writer.WriteEndArray();
      }
      if (schema.Disallow != null)
        WriteType(JsonSchemaConstants.DisallowPropertyName, _writer, schema.Disallow.Value);
      if (schema.Extends != null)
      {
        _writer.WritePropertyName(JsonSchemaConstants.ExtendsPropertyName);
        ReferenceOrWriteSchema(schema.Extends);
      }
      _writer.WriteEndObject();
    }

    private void WriteItems(JsonSchema schema)
    {
      if (CollectionUtils.IsNullOrEmpty(schema.Items))
        return;

      _writer.WritePropertyName(JsonSchemaConstants.ItemsPropertyName);

      if (schema.Items.Count == 1)
      {
        ReferenceOrWriteSchema(schema.Items[0]);
        return;
      }

      _writer.WriteStartArray();
      foreach (JsonSchema itemSchema in schema.Items)
      {
        ReferenceOrWriteSchema(itemSchema);
      }
      _writer.WriteEndArray();
    }

    private void WriteType(string propertyName, JsonWriter writer, JsonSchemaType type)
    {
      IList<JsonSchemaType> types;
      if (System.Enum.IsDefined(typeof(JsonSchemaType), type))
        types = new List<JsonSchemaType> { type };
      else
        types = EnumUtils.GetFlagsValues(type).Where(v => v != JsonSchemaType.None).ToList();

      if (types.Count == 0)
        return;

      writer.WritePropertyName(propertyName);

      if (types.Count == 1)
      {
        writer.WriteValue(JsonSchemaBuilder.MapType(types[0]));
        return;
      }

      writer.WriteStartArray();
      foreach (JsonSchemaType jsonSchemaType in types)
      {
        writer.WriteValue(JsonSchemaBuilder.MapType(jsonSchemaType));
      }
      writer.WriteEndArray();
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
