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

namespace Newtonsoft.Json
{
    /// <summary>
    /// Provides an interface to enable a class to return line and position information.
    /// </summary>
    public interface IJsonLineInfo
    {
        /// <summary>
        /// Gets a value indicating whether the class can return line information.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if <see cref="LineNumber"/> and <see cref="LinePosition"/> can be provided; otherwise, <c>false</c>.
        /// </returns>
        bool HasLineInfo();

        /// <summary>
        /// Gets the current line number.
        /// </summary>
        /// <value>The current line number or 0 if no line information is available (for example, when <see cref="HasLineInfo"/> returns <c>false</c>).</value>
        int LineNumber { get; }

        /// <summary>
        /// Gets the current line position.
        /// </summary>
        /// <value>The current line position or 0 if no line information is available (for example, when <see cref="HasLineInfo"/> returns <c>false</c>).</value>
        int LinePosition { get; }
    }
}