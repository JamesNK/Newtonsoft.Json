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
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// <para>
    /// An in-memory representation of a JSON Schema.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonSchema
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets whether the object is required.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// Gets or sets whether the object is read-only.
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the object is visible to users.
        /// </summary>
        public bool? Hidden { get; set; }

        /// <summary>
        /// Gets or sets whether the object is transient.
        /// </summary>
        public bool? Transient { get; set; }

        /// <summary>
        /// Gets or sets the description of the object.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the types of values allowed by the object.
        /// </summary>
        /// <value>The type.</value>
        public JsonSchemaType? Type { get; set; }

        /// <summary>
        /// Gets or sets the pattern.
        /// </summary>
        /// <value>The pattern.</value>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the minimum length.
        /// </summary>
        /// <value>The minimum length.</value>
        public int? MinimumLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        public int? MaximumLength { get; set; }

        /// <summary>
        /// Gets or sets a number that the value should be divisible by.
        /// </summary>
        /// <value>A number that the value should be divisible by.</value>
        public double? DivisibleBy { get; set; }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the value can not equal the number defined by the <c>minimum</c> attribute (<see cref="JsonSchema.Minimum"/>).
        /// </summary>
        /// <value>A flag indicating whether the value can not equal the number defined by the <c>minimum</c> attribute (<see cref="JsonSchema.Minimum"/>).</value>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the value can not equal the number defined by the <c>maximum</c> attribute (<see cref="JsonSchema.Maximum"/>).
        /// </summary>
        /// <value>A flag indicating whether the value can not equal the number defined by the <c>maximum</c> attribute (<see cref="JsonSchema.Maximum"/>).</value>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of items.
        /// </summary>
        /// <value>The minimum number of items.</value>
        public int? MinimumItems { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items.
        /// </summary>
        /// <value>The maximum number of items.</value>
        public int? MaximumItems { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSchema"/> of items.
        /// </summary>
        /// <value>The <see cref="JsonSchema"/> of items.</value>
        public IList<JsonSchema> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items in an array are validated using the <see cref="JsonSchema"/> instance at their array position from <see cref="JsonSchema.Items"/>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if items are validated using their array position; otherwise, <c>false</c>.
        /// </value>
        public bool PositionalItemsValidation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSchema"/> of additional items.
        /// </summary>
        /// <value>The <see cref="JsonSchema"/> of additional items.</value>
        public JsonSchema AdditionalItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether additional items are allowed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if additional items are allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAdditionalItems { get; set; }

        /// <summary>
        /// Gets or sets whether the array items must be unique.
        /// </summary>
        public bool UniqueItems { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSchema"/> of properties.
        /// </summary>
        /// <value>The <see cref="JsonSchema"/> of properties.</value>
        public IDictionary<string, JsonSchema> Properties { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSchema"/> of additional properties.
        /// </summary>
        /// <value>The <see cref="JsonSchema"/> of additional properties.</value>
        public JsonSchema AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the pattern properties.
        /// </summary>
        /// <value>The pattern properties.</value>
        public IDictionary<string, JsonSchema> PatternProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether additional properties are allowed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if additional properties are allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the required property if this property is present.
        /// </summary>
        /// <value>The required property if this property is present.</value>
        public string Requires { get; set; }

        /// <summary>
        /// Gets or sets the a collection of valid enum values allowed.
        /// </summary>
        /// <value>A collection of valid enum values allowed.</value>
        public IList<JToken> Enum { get; set; }

        /// <summary>
        /// Gets or sets disallowed types.
        /// </summary>
        /// <value>The disallowed types.</value>
        public JsonSchemaType? Disallow { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public JToken Default { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="JsonSchema"/> that this schema extends.
        /// </summary>
        /// <value>The collection of <see cref="JsonSchema"/> that this schema extends.</value>
        public IList<JsonSchema> Extends { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public string Format { get; set; }

        internal string Location { get; set; }

        private readonly string _internalId = Guid.NewGuid().ToString("N");

        internal string InternalId
        {
            get { return _internalId; }
        }

        // if this is set then this schema instance is just a deferred reference
        // and will be replaced when the schema reference is resolved
        internal string DeferredReference { get; set; }
        internal bool ReferencesResolved { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchema"/> class.
        /// </summary>
        public JsonSchema()
        {
            AllowAdditionalProperties = true;
            AllowAdditionalItems = true;
        }

        /// <summary>
        /// Reads a <see cref="JsonSchema"/> from the specified <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the JSON Schema to read.</param>
        /// <returns>The <see cref="JsonSchema"/> object representing the JSON Schema.</returns>
        public static JsonSchema Read(JsonReader reader)
        {
            return Read(reader, new JsonSchemaResolver());
        }

        /// <summary>
        /// Reads a <see cref="JsonSchema"/> from the specified <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the JSON Schema to read.</param>
        /// <param name="resolver">The <see cref="JsonSchemaResolver"/> to use when resolving schema references.</param>
        /// <returns>The <see cref="JsonSchema"/> object representing the JSON Schema.</returns>
        public static JsonSchema Read(JsonReader reader, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(resolver, nameof(resolver));

            JsonSchemaBuilder builder = new JsonSchemaBuilder(resolver);
            return builder.Read(reader);
        }

        /// <summary>
        /// Load a <see cref="JsonSchema"/> from a string that contains JSON Schema.
        /// </summary>
        /// <param name="json">A <see cref="String"/> that contains JSON Schema.</param>
        /// <returns>A <see cref="JsonSchema"/> populated from the string that contains JSON Schema.</returns>
        public static JsonSchema Parse(string json)
        {
            return Parse(json, new JsonSchemaResolver());
        }

        /// <summary>
        /// Load a <see cref="JsonSchema"/> from a string that contains JSON Schema using the specified <see cref="JsonSchemaResolver"/>.
        /// </summary>
        /// <param name="json">A <see cref="String"/> that contains JSON Schema.</param>
        /// <param name="resolver">The resolver.</param>
        /// <returns>A <see cref="JsonSchema"/> populated from the string that contains JSON Schema.</returns>
        public static JsonSchema Parse(string json, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(json, nameof(json));

            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                return Read(reader, resolver);
            }
        }

        /// <summary>
        /// Writes this schema to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        public void WriteTo(JsonWriter writer)
        {
            WriteTo(writer, new JsonSchemaResolver());
        }

        /// <summary>
        /// Writes this schema to a <see cref="JsonWriter"/> using the specified <see cref="JsonSchemaResolver"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="resolver">The resolver used.</param>
        public void WriteTo(JsonWriter writer, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            ValidationUtils.ArgumentNotNull(resolver, nameof(resolver));

            JsonSchemaWriter schemaWriter = new JsonSchemaWriter(writer, resolver);
            schemaWriter.WriteSchema(this);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </returns>
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