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

namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies the state of the <see cref="JsonWriter"/>.
  /// </summary>
  public enum WriteState
  {
    /// <summary>
    /// An exception has been thrown, which has left the <see cref="JsonWriter"/> in an invalid state.
    /// You may call the <see cref="JsonWriter.Close"/> method to put the <see cref="JsonWriter"/> in the <c>Closed</c> state.
    /// Any other <see cref="JsonWriter"/> method calls results in an <see cref="InvalidOperationException"/> being thrown. 
    /// </summary>
    Error,
    /// <summary>
    /// The <see cref="JsonWriter.Close"/> method has been called. 
    /// </summary>
    Closed,
    /// <summary>
    /// An object is being written. 
    /// </summary>
    Object,
    /// <summary>
    /// A array is being written.
    /// </summary>
    Array,
    /// <summary>
    /// A constructor is being written.
    /// </summary>
    Constructor,
    /// <summary>
    /// A property is being written.
    /// </summary>
    Property,
    /// <summary>
    /// A write method has not been called.
    /// </summary>
    Start
  }
}