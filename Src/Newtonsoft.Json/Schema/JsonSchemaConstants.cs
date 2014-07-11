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

using System.Collections.Generic;

namespace Newtonsoft.Json.Schema
{
    internal static class JsonSchemaConstants
    {
        public const string TypePropertyName = "type";
        public const string PropertiesPropertyName = "properties";
        public const string ItemsPropertyName = "items";
        public const string AdditionalItemsPropertyName = "additionalItems";
        public const string RequiredPropertyName = "required";
        public const string PatternPropertiesPropertyName = "patternProperties";
        public const string AdditionalPropertiesPropertyName = "additionalProperties";
        public const string DependenciesPropertyName = "dependencies";
        public const string MinimumPropertyName = "minimum";
        public const string MaximumPropertyName = "maximum";
        public const string ExclusiveMinimumPropertyName = "exclusiveMinimum";
        public const string ExclusiveMaximumPropertyName = "exclusiveMaximum";
        public const string MinimumItemsPropertyName = "minItems";
        public const string MaximumItemsPropertyName = "maxItems";
        public const string PatternPropertyName = "pattern";
        public const string MaximumLengthPropertyName = "maxLength";
        public const string MinimumLengthPropertyName = "minLength";
        public const string EnumPropertyName = "enum";
        public const string ReadOnlyPropertyName = "readonly";
        public const string TitlePropertyName = "title";
        public const string DescriptionPropertyName = "description";
        public const string FormatPropertyName = "format";
        public const string DefaultPropertyName = "default";
        public const string TransientPropertyName = "transient";
        public const string MultipleOfPropertyName = "multipleOf";
        public const string HiddenPropertyName = "hidden";
        public const string AnyOfPropertyName = "anyOf";
        public const string AllOfPropertyName = "allOf";
        public const string OneOfPropertyName = "oneOf";
        public const string NotOfPropertyName = "not";
        public const string IdPropertyName = "id";
        public const string UniqueItemsPropertyName = "uniqueItems";
        public const string MinimumPropertiesPropertyName = "minProperties";
        public const string MaximumPropertiesPropertyName = "maxProperties";
        public const string SchemaTypePropertyName = "$schema";
        public const string SchemaTypePropertyValue = "http://json-schema.org/draft-04/schema#";
        public const string LinksPropertyName = "links";
        public const string HrefPropertyName = "href";
        public const string RelationPropertyName = "rel";
        public const string TargetSchemaPropertyName = "targetSchema";
        public const string MediaTypePropertyName = "mediaType";
        public const string MethodPropertyName = "method";
        public const string EncodingTypePropertyName = "encType";
        public const string SchemaPropertyName = "schema";
        public const string MediaPropertyName = "media";
        public const string BinaryEncodingPropertyName = "binaryEncoding";
        public const string FragmentResolutionPropertyName = "fragmentResolution";
        public const string PathStartPropertyName = "pathStart";
        public const string JsonContentType = "application/json";
        public const string JsonPointerProtocol = "json-pointer";
        public const string DefaultRequestMethod = "GET";
        public const string SelfRelation = "self";

        public const string OptionValuePropertyName = "value";
        public const string OptionLabelPropertyName = "label";

        public static readonly IDictionary<string, JsonSchemaType> JsonSchemaTypeMapping = new Dictionary<string, JsonSchemaType>
        {
            { "string", JsonSchemaType.String },
            { "object", JsonSchemaType.Object },
            { "integer", JsonSchemaType.Integer },
            { "number", JsonSchemaType.Float },
            { "null", JsonSchemaType.Null },
            { "boolean", JsonSchemaType.Boolean },
            { "array", JsonSchemaType.Array },
            { "any", JsonSchemaType.Any }
        };
    }
}