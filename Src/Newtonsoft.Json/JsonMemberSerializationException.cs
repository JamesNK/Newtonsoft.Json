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
    /// The types of errors related to members
    /// </summary>
    public enum MemberErrorType {
        NoMemberError=0,
        RequiredNotFound=1,
        RequiredIsNull=2,
        MissingMember=3
    }

    /// <summary>
    /// The exception thrown when an error occurs during JSON serialization or deserialization.
    /// </summary>
#if HAVE_BINARY_EXCEPTION_SERIALIZATION
    [Serializable]
#endif
    public class JsonMemberSerializationException : JsonSerializationException
    {

        /// <summary>
        /// Gets the member error type for a required or missing member.
        /// </summary>
        /// <value>The name of the missing JSON member.</value>
        public MemberErrorType MemberErrorType { get; }

        /// <summary>
        /// Gets the name of the Json member.
        /// </summary>
        /// <value>The name of the JSON member.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the object type name of the Json member.
        /// </summary>
        /// <value>The name of the JSON member's parent object type.</value>
        public string ObjectTypeName { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMemberSerializationException"/> class.
        /// </summary>
        public JsonMemberSerializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMemberSerializationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public  JsonMemberSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMemberSerializationException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public JsonMemberSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if HAVE_BINARY_EXCEPTION_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMemberSerializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0).</exception>
        public JsonMemberSerializationException(SerializationInfo info, StreamingContext context)
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
        /// <param name="memberErrorType">The type of member error which occurred.</param>
        /// <param name="memberName">The name of the member being parsed where the error occurred.</param>
        /// <param name="objectTypeName">The name of the type of object being serialized where the error occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public JsonMemberSerializationException(string message, string path, int lineNumber, int linePosition, MemberErrorType memberErrorType, string memberName, string objectTypeName, Exception? innerException)
            : base(message, path, lineNumber, linePosition, innerException)
        {
            MemberErrorType = memberErrorType;
            MemberName = memberName;
            ObjectTypeName = objectTypeName;
        }

        internal static JsonMemberSerializationException Create(JsonReader reader, string message, MemberErrorType memberErrorType, string memberName, string objectTypeName)
        {
            return Create(reader, message, memberErrorType, memberName, objectTypeName, null);
        }

        internal static JsonMemberSerializationException Create(JsonReader reader, string message, MemberErrorType memberErrorType, string memberName, string objectTypeName, Exception? ex)
        {
            return Create(reader as IJsonLineInfo, reader.Path, message, memberErrorType, memberName, objectTypeName, ex);
        }

        internal static JsonMemberSerializationException Create(IJsonLineInfo? lineInfo, string path, string message, MemberErrorType memberErrorType, string memberName, string objectTypeName, Exception? ex)
        {
            return new JsonMemberSerializationException(message, path, lineNumber, linePosition, memberErrorType, memberName, objectTypeName, ex);
        }
    }
}