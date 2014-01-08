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
        None,

        /// <summary>
        /// A JSON object.
        /// </summary>
        Object,

        /// <summary>
        /// A JSON array.
        /// </summary>
        Array,

        /// <summary>
        /// A JSON constructor.
        /// </summary>
        Constructor,

        /// <summary>
        /// A JSON object property.
        /// </summary>
        Property,

        /// <summary>
        /// A comment.
        /// </summary>
        Comment,

        /// <summary>
        /// An integer value.
        /// </summary>
        Integer,

        /// <summary>
        /// A float value.
        /// </summary>
        Float,

        /// <summary>
        /// A string value.
        /// </summary>
        String,

        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// A null value.
        /// </summary>
        Null,

        /// <summary>
        /// An undefined value.
        /// </summary>
        Undefined,

        /// <summary>
        /// A date value.
        /// </summary>
        Date,

        /// <summary>
        /// A raw JSON value.
        /// </summary>
        Raw,

        /// <summary>
        /// A collection of bytes value.
        /// </summary>
        Bytes,

        /// <summary>
        /// A Guid value.
        /// </summary>
        Guid,

        /// <summary>
        /// A Uri value.
        /// </summary>
        Uri,

        /// <summary>
        /// A TimeSpan value.
        /// </summary>
        TimeSpan
    }
}