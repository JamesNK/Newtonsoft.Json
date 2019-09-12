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

using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a raw JSON string.
    /// </summary>
    public partial class JRaw : JValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JRaw"/> class from another <see cref="JRaw"/> object.
        /// </summary>
        /// <param name="other">A <see cref="JRaw"/> object to copy from.</param>
        public JRaw(JRaw other)
            : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JRaw"/> class.
        /// </summary>
        /// <param name="rawJson">The raw json.</param>
        public JRaw(object? rawJson)
            : base(rawJson, JTokenType.Raw)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="JRaw"/> with the content of the reader's current token.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>An instance of <see cref="JRaw"/> with the content of the reader's current token.</returns>
        public static JRaw Create(JsonReader reader)
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteToken(reader);

                return new JRaw(sw.ToString());
            }
        }

        internal override JToken CloneToken()
        {
            return new JRaw(this);
        }
    }
}