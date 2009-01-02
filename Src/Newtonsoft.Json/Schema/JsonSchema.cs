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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Schema
{
  public class JsonSchema
  {
    public string Id { get; set; }
    public string Title { get; set; }
    public bool? Optional { get; set; }
    public bool? ReadOnly { get; set; }
    public bool? Hidden { get; set; }
    public bool? Transient { get; set; }
    public string Description { get; set; }
    public JsonSchemaType? Type { get; set; }
    public string Pattern { get; set; }
    public int? MinimumLength { get; set; }
    public int? MaximumLength { get; set; }
    public int? MaximumDecimals { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }
    public int? MinimumItems { get; set; }
    public int? MaximumItems { get; set; }
    public IList<JsonSchema> Items { get; set; }
    public IDictionary<string, JsonSchema> Properties { get; set; }
    public JsonSchema AdditionalProperties { get; set; }
    public bool AllowAdditionalProperties { get; set; }
    public string Requires { get; set; }
    public IList<string> Identity { get; set; }
    public IList<JToken> Enum { get; set; }
    public IDictionary<JToken, string> Options { get; set; }
    public JsonSchemaType? Disallow { get; set; }
    public JToken Default { get; set; }
    public JsonSchema Extends { get; set; }
    public string Format { get; set; }

    private readonly string _internalId = Guid.NewGuid().ToString("N");

    internal string InternalId
    {
      get { return _internalId; }
    }

    public JsonSchema()
    {
      AllowAdditionalProperties = true;
    }

    public static JsonSchema Read(JsonReader reader)
    {
      return Read(reader, new JsonSchemaResolver());
    }

    public static JsonSchema Read(JsonReader reader, JsonSchemaResolver resolver)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");
      ValidationUtils.ArgumentNotNull(resolver, "resolver");

      JsonSchemaBuilder builder = new JsonSchemaBuilder(resolver);
      return builder.Parse(reader);
    }

    public static JsonSchema Parse(string json)
    {
      return Parse(json, new JsonSchemaResolver());
    }

    public static JsonSchema Parse(string json, JsonSchemaResolver resolver)
    {
      ValidationUtils.ArgumentNotNull(json, "json");

      JsonReader reader = new JsonTextReader(new StringReader(json));

      return Read(reader, resolver);
    }

    public void WriteTo(JsonWriter writer)
    {
      WriteTo(writer, new JsonSchemaResolver());
    }

    public void WriteTo(JsonWriter writer, JsonSchemaResolver resolver)
    {
      ValidationUtils.ArgumentNotNull(writer, "writer");
      ValidationUtils.ArgumentNotNull(resolver, "resolver");

      JsonSchemaWriter schemaWriter = new JsonSchemaWriter(writer, resolver);
      schemaWriter.WriteSchema(this);
    }

    public override string ToString()
    {
      StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      WriteTo(jsonWriter);

      return writer.ToString();
    }
  }
}