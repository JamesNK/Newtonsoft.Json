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

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// An in-memory representation of a JSON Schema link.
    /// </summary>
    public class JsonSchemaLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchemaLink"/> class.
        /// </summary>
        public JsonSchemaLink()
        {
            EncodingType = JsonSchemaConstants.JsonContentType;
            Method = JsonSchemaConstants.DefaultRequestMethod;
        }

        /// <summary>
        /// Gets or sets a URI template, as defined by RFC 6570, with the addition of the $, ( and ) characters for pre-processing
        /// </summary>
        public string Href { get; set; }
        
        /// <summary>
        /// Gets or sets the relation to the target resource of the link
        /// </summary>
        public string Relation { get; set; }

        /// <summary>
        /// Gets or sets a title for the link
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="JsonSchema"/> describing the link target
        /// </summary>
        public JsonSchema TargetSchema { get; set; }

        /// <summary>
        /// Gets or sets the media type (as defined by RFC 2046) describing the link target
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the method for requesting the target of the link (e.g. for HTTP this might be "GET" or "DELETE")
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the media type in which to submit data along with the request
        /// </summary>
        public string EncodingType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSchema"/> describing the data to submit along with the request
        /// </summary>
        public JsonSchema Schema { get; set; }
    }
}
