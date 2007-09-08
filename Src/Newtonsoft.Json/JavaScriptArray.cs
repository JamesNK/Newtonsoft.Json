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
using System.Text;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Represents a JavaScript array.
  /// </summary>
  public class JavaScriptArray : List<object>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptObject"/> class.
    /// </summary>
    public JavaScriptArray()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptArray"/> class that
    /// contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new array.</param>
    public JavaScriptArray(IEnumerable<object> collection)
      : base(collection)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptArray"/> class that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new array can initially store.</param>
    public JavaScriptArray(int capacity)
      : base(capacity)
    {
    }
  }
}