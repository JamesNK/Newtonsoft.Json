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

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to and from its name string value (case sensitive).
    /// </summary>
    public class StringEnumCaseInsensitiveConverter : StringEnumConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnumCaseInsensitiveConverter"/> class.
        /// </summary>
        public StringEnumCaseInsensitiveConverter()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnumCaseInsensitiveConverter"/> class.
        /// </summary>
        /// <param name="camelCaseText"><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</param>
        public StringEnumCaseInsensitiveConverter(bool camelCaseText)
            : base(camelCaseText, false)
        {
        }
    }
}
