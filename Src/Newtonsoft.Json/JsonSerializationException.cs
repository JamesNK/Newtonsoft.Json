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
using System.Runtime.Serialization;
using System.Text;

namespace Newtonsoft.Json
{
    /// <summary>
    /// The exception thrown when an error occurs during JSON serialization or deserialization.
    /// </summary>
#if HAVE_BINARY_EXCEPTION_SERIALIZATION
    [Serializable]
#endif
    public class JsonSerializationException : JsonException
    {
        /// <summary>
        /// Gets the line number indicating where the error occurred.
        /// </summary>
        /// <value>The line number indicating where the error occurred.</value>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the line position indicating where the error occurred.
        /// </summary>
        /// <value>The line position indicating where the error occurred.</value>
        public int LinePosition { get; }

        /// <summary>
        /// Gets the path to the JSON where the error occurred.
        /// </summary>
        /// <value>The path to the JSON where the error occurred.</value>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class.
        /// </summary>
        public JsonSerializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public JsonSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public JsonSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if HAVE_BINARY_EXCEPTION_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0).</exception>
        public JsonSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class
        /// with a specified error message, JSON path, line number, line position, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path to the JSON where the error occurred.</param>
        /// <param name="lineNumber">The line number indicating where the error occurred.</param>
        /// <param name="linePosition">The line position indicating where the error occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public JsonSerializationException(string message, string path, int lineNumber, int linePosition, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        internal static JsonSerializationException Create(JsonReader reader, string message)
        {
            return Create(reader, message, null);
        }

        internal static JsonSerializationException Create(JsonReader reader, string message, Exception ex)
        {
            return Create(reader as IJsonLineInfo, reader.Path, message, ex);
        }

        internal static JsonSerializationException Create(IJsonLineInfo lineInfo, string path, string message, Exception ex)
        {
            message = JsonPosition.FormatMessage(lineInfo, path, message);

            int lineNumber;
            int linePosition;
            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }

            return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
        }
    }
}