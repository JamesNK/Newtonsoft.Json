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

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Provides information surrounding an error.
    /// </summary>
    public class ErrorContext
    {
        internal ErrorContext(object originalObject, object member, string path, Exception error)
        {
            OriginalObject = originalObject;
            Member = member;
            Error = error;
            Path = path;
        }

        internal bool Traced { get; set; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets the original object that caused the error.
        /// </summary>
        /// <value>The original object that caused the error.</value>
        public object OriginalObject { get; private set; }

        /// <summary>
        /// Gets the member that caused the error.
        /// </summary>
        /// <value>The member that caused the error.</value>
        public object Member { get; private set; }

        /// <summary>
        /// Gets the path of the JSON location where the error occurred.
        /// </summary>
        /// <value>The path of the JSON location where the error occurred.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ErrorContext"/> is handled.
        /// </summary>
        /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
        public bool Handled { get; set; }
    }
}