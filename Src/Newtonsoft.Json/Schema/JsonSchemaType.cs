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

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// The value types allowed by the <see cref="JsonSchema"/>.
    /// </summary>
    [Flags]
    public enum JsonSchemaType
    {
        /// <summary>
        /// No type specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// String type.
        /// </summary>
        String = 1,

        /// <summary>
        /// Float type.
        /// </summary>
        Float = 2,

        /// <summary>
        /// Integer type.
        /// </summary>
        Integer = 4,

        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean = 8,

        /// <summary>
        /// Object type.
        /// </summary>
        Object = 16,

        /// <summary>
        /// Array type.
        /// </summary>
        Array = 32,

        /// <summary>
        /// Null type.
        /// </summary>
        Null = 64,

        /// <summary>
        /// Any type.
        /// </summary>
        Any = String | Float | Integer | Boolean | Object | Array | Null
    }
}