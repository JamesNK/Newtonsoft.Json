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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    /// <summary>
    /// The exception thrown when an error occurs during JSON serialization or deserialization.
    /// </summary>
#if !(DOTNET || PORTABLE40 || PORTABLE)
    [Serializable]
#endif
    public class JsonException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonException"/> class.
        /// </summary>
        public JsonException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public JsonException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public JsonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        public JsonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        internal static JsonException Create(IJsonLineInfo lineInfo, string path, string message)
        {
            message = JsonPosition.FormatMessage(lineInfo, path, message);

            return new JsonException(message);
        }
    }
}