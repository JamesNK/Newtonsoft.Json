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
using System.Net;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal abstract class JsonSerializerInternalBase
  {
    private ErrorContext _currentErrorContext;

    internal JsonSerializer Serializer { get; private set; }

    protected JsonSerializerInternalBase(JsonSerializer serializer)
    {
      ValidationUtils.ArgumentNotNull(serializer, "serializer");

      Serializer = serializer;
    }

    protected ErrorContext GetErrorContext(object currentObject, object member, Exception error)
    {
      if (_currentErrorContext == null)
        _currentErrorContext = new ErrorContext(currentObject, member, error);

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

    protected bool IsErrorHandled(object currentObject, JsonContract contract, object keyValue, Exception ex)
    {
      ErrorContext errorContext = GetErrorContext(currentObject, keyValue, ex);
      contract.InvokeOnError(currentObject, Serializer.Context, errorContext);

      if (!errorContext.Handled)
        Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));

      return errorContext.Handled;
    }
  }
}