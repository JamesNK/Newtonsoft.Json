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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// Contains the JSON schema extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the <see cref="JToken"/> is valid.
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="JToken"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this JToken source, JsonSchema schema)
        {
            bool valid = true;
            source.Validate(schema, (sender, args) => { valid = false; });
            return valid;
        }

        /// <summary>
        /// Determines whether the <see cref="JToken"/> is valid.
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <param name="errorMessages">When this method returns, contains any error messages generated while validating. </param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="JToken"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this JToken source, JsonSchema schema, out IList<string> errorMessages)
        {
            IList<string> errors = new List<string>();

            source.Validate(schema, (sender, args) => errors.Add(args.Message));

            errorMessages = errors;
            return (errorMessages.Count == 0);
        }

        /// <summary>
        /// Validates the specified <see cref="JToken"/>.
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        public static void Validate(this JToken source, JsonSchema schema)
        {
            source.Validate(schema, null);
        }

        /// <summary>
        /// Validates the specified <see cref="JToken"/>.
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        /// <param name="validationEventHandler">The validation event handler.</param>
        public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
        {
            ValidationUtils.ArgumentNotNull(source, "source");
            ValidationUtils.ArgumentNotNull(schema, "schema");

            using (JsonValidatingReader reader = new JsonValidatingReader(source.CreateReader()))
            {
                reader.Schema = schema;
                if (validationEventHandler != null)
                    reader.ValidationEventHandler += validationEventHandler;

                while (reader.Read())
                {
                }
            }
        }
    }
}