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
    /// Provides data for the Error event.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current object the error event is being raised against.
        /// </summary>
        /// <value>The current object the error event is being raised against.</value>
        public object? CurrentObject { get; }

        /// <summary>
        /// Gets the error context.
        /// </summary>
        /// <value>The error context.</value>
        public ErrorContext ErrorContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="currentObject">The current object.</param>
        /// <param name="errorContext">The error context.</param>
        public ErrorEventArgs(object? currentObject, ErrorContext errorContext)
        {
            CurrentObject = currentObject;
            ErrorContext = errorContext;
        }
    }
}