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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema
{
  /// <summary>
  /// Returns detailed information related to the <see cref="ValidationEventHandler"/>.
  /// </summary>
  public class ValidationEventArgs : EventArgs
  {
    private readonly JsonSchemaException _ex;

    internal ValidationEventArgs(JsonSchemaException ex)
    {
      ValidationUtils.ArgumentNotNull(ex, "ex");
      _ex = ex;
    }

    /// <summary>
    /// Gets the <see cref="JsonSchemaException"/> associated with the validation event.
    /// </summary>
    /// <value>The JsonSchemaException associated with the validation event.</value>
    public JsonSchemaException Exception
    {
      get { return _ex; }
    }

    /// <summary>
    /// Gets the text description corresponding to the validation event.
    /// </summary>
    /// <value>The text description.</value>
    public string Message
    {
      get { return _ex.Message; }
    }
  }
}