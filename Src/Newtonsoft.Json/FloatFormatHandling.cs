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

namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies float format handling options when writing special floating point numbers, e.g. <see cref="Double.NaN"/>,
    /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/> with <see cref="JsonWriter"/>.
    /// </summary>
    public enum FloatFormatHandling
    {
        /// <summary>
        /// Write special floating point values as strings in JSON, e.g. <c>"NaN"</c>, <c>"Infinity"</c>, <c>"-Infinity"</c>.
        /// </summary>
        String = 0,

        /// <summary>
        /// Write special floating point values as symbols in JSON, e.g. <c>NaN</c>, <c>Infinity</c>, <c>-Infinity</c>.
        /// Note that this will produce non-valid JSON.
        /// </summary>
        Symbol = 1,

        /// <summary>
        /// Write special floating point values as the property's default value in JSON, e.g. 0.0 for a <see cref="Double"/> property, <c>null</c> for a <see cref="Nullable{T}"/> of <see cref="Double"/> property.
        /// </summary>
        DefaultValue = 2
    }
}