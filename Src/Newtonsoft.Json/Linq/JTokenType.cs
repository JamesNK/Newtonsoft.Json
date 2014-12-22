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

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the type of token.
    /// </summary>
    public enum JTokenType
    {
        /// <summary>
        /// No token type has been set.
        /// </summary>
        None = 0,

        /// <summary>
        /// A JSON object.
        /// </summary>
        Object = 1,

        /// <summary>
        /// A JSON array.
        /// </summary>
        Array = 2,

        /// <summary>
        /// A JSON constructor.
        /// </summary>
        Constructor = 3,

        /// <summary>
        /// A JSON object property.
        /// </summary>
        Property = 4,

        /// <summary>
        /// A comment.
        /// </summary>
        Comment = 5,

        /// <summary>
        /// An integer value.
        /// </summary>
        Integer = 6,

        /// <summary>
        /// A float value.
        /// </summary>
        Float = 7,

        /// <summary>
        /// A string value.
        /// </summary>
        String = 8,

        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean = 9,

        /// <summary>
        /// A null value.
        /// </summary>
        Null = 10,

        /// <summary>
        /// An undefined value.
        /// </summary>
        Undefined = 11,

        /// <summary>
        /// A date value.
        /// </summary>
        Date = 12,

        /// <summary>
        /// A raw JSON value.
        /// </summary>
        Raw = 13,

        /// <summary>
        /// A collection of bytes value.
        /// </summary>
        Bytes = 14,

        /// <summary>
        /// A Guid value.
        /// </summary>
        Guid = 15,

        /// <summary>
        /// A Uri value.
        /// </summary>
        Uri = 16,

        /// <summary>
        /// A TimeSpan value.
        /// </summary>
        TimeSpan = 17
    }
}