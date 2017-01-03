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
using System.Collections.ObjectModel;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaNode
    {
        public string Id { get; private set; }
        public ReadOnlyCollection<JsonSchema> Schemas { get; private set; }
        public Dictionary<string, JsonSchemaNode> Properties { get; private set; }
        public Dictionary<string, JsonSchemaNode> PatternProperties { get; private set; }
        public List<JsonSchemaNode> Items { get; private set; }
        public JsonSchemaNode AdditionalProperties { get; set; }
        public JsonSchemaNode AdditionalItems { get; set; }

        public JsonSchemaNode(JsonSchema schema)
        {
            Schemas = new ReadOnlyCollection<JsonSchema>(new[] { schema });
            Properties = new Dictionary<string, JsonSchemaNode>();
            PatternProperties = new Dictionary<string, JsonSchemaNode>();
            Items = new List<JsonSchemaNode>();

            Id = GetId(Schemas);
        }

        private JsonSchemaNode(JsonSchemaNode source, JsonSchema schema)
        {
            Schemas = new ReadOnlyCollection<JsonSchema>(source.Schemas.Union(new[] { schema }).ToList());
            Properties = new Dictionary<string, JsonSchemaNode>(source.Properties);
            PatternProperties = new Dictionary<string, JsonSchemaNode>(source.PatternProperties);
            Items = new List<JsonSchemaNode>(source.Items);
            AdditionalProperties = source.AdditionalProperties;
            AdditionalItems = source.AdditionalItems;

            Id = GetId(Schemas);
        }

        public JsonSchemaNode Combine(JsonSchema schema)
        {
            return new JsonSchemaNode(this, schema);
        }

        public static string GetId(IEnumerable<JsonSchema> schemata)
        {
            return string.Join("-", schemata.Select(s => s.InternalId).OrderBy(id => id, StringComparer.Ordinal)
#if NET20 || NET35
                    .ToArray()
#endif
                );
        }
    }
}