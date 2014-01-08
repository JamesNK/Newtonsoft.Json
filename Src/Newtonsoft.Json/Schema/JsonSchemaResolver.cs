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

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// Resolves <see cref="JsonSchema"/> from an id.
    /// </summary>
    public class JsonSchemaResolver
    {
        /// <summary>
        /// Gets or sets the loaded schemas.
        /// </summary>
        /// <value>The loaded schemas.</value>
        public IList<JsonSchema> LoadedSchemas { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchemaResolver"/> class.
        /// </summary>
        public JsonSchemaResolver()
        {
            LoadedSchemas = new List<JsonSchema>();
        }

        /// <summary>
        /// Gets a <see cref="JsonSchema"/> for the specified reference.
        /// </summary>
        /// <param name="reference">The id.</param>
        /// <returns>A <see cref="JsonSchema"/> for the specified reference.</returns>
        public virtual JsonSchema GetSchema(string reference)
        {
            JsonSchema schema = LoadedSchemas.SingleOrDefault(s => string.Equals(s.Id, reference, StringComparison.Ordinal));

            if (schema == null)
                schema = LoadedSchemas.SingleOrDefault(s => string.Equals(s.Location, reference, StringComparison.Ordinal));

            return schema;
        }
    }
}