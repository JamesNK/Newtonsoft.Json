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
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    internal abstract class JsonSerializerInternalBase
    {
        private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
#if !(NETFX_CORE)
                // put objects in a bucket based on their reference
                return RuntimeHelpers.GetHashCode(obj);
#else
    // put all objects in the same bucket so ReferenceEquals is called on all
        return -1;
#endif
            }
        }

        private ErrorContext _currentErrorContext;
        private BidirectionalDictionary<string, object> _mappings;
        private bool _serializing;

        internal readonly JsonSerializer Serializer;
        internal readonly ITraceWriter TraceWriter;

        protected JsonSerializerInternalBase(JsonSerializer serializer)
        {
            ValidationUtils.ArgumentNotNull(serializer, "serializer");

            Serializer = serializer;
            TraceWriter = serializer.TraceWriter;

            // kind of a hack but meh. might clean this up later
            _serializing = (GetType() == typeof(JsonSerializerInternalWriter));
        }

        internal BidirectionalDictionary<string, object> DefaultReferenceMappings
        {
            get
            {
                // override equality comparer for object key dictionary
                // object will be modified as it deserializes and might have mutable hashcode
                if (_mappings == null)
                    _mappings = new BidirectionalDictionary<string, object>(
                        EqualityComparer<string>.Default,
                        new ReferenceEqualsEqualityComparer(),
                        "A different value already has the Id '{0}'.",
                        "A different Id has already been assigned for value '{0}'.");

                return _mappings;
            }
        }

        private ErrorContext GetErrorContext(object currentObject, object member, string path, Exception error)
        {
            if (_currentErrorContext == null)
                _currentErrorContext = new ErrorContext(currentObject, member, path, error);

            if (_currentErrorContext.Error != error)
                throw new InvalidOperationException("Current error context error is different to requested error.");

            return _currentErrorContext;
        }

        protected void ClearErrorContext()
        {
            if (_currentErrorContext == null)
                throw new InvalidOperationException("Could not clear error context. Error context is already null.");

            _currentErrorContext = null;
        }

        protected bool IsErrorHandled(object currentObject, JsonContract contract, object keyValue, IJsonLineInfo lineInfo, string path, Exception ex)
        {
            ErrorContext errorContext = GetErrorContext(currentObject, keyValue, path, ex);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Error && !errorContext.Traced)
            {
                // only write error once
                errorContext.Traced = true;

                string message = (_serializing) ? "Error serializing" : "Error deserializing";
                if (contract != null)
                    message += " " + contract.UnderlyingType;
                message += ". " + ex.Message;

                // add line information to non-json.net exception message
                if (!(ex is JsonException))
                    message = JsonPosition.FormatMessage(lineInfo, path, message);

                TraceWriter.Trace(TraceLevel.Error, message, ex);
            }

            // attribute method is non-static so don't invoke if no object
            if (contract != null && currentObject != null)
                contract.InvokeOnError(currentObject, Serializer.Context, errorContext);

            if (!errorContext.Handled)
                Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));

            return errorContext.Handled;
        }
    }
}