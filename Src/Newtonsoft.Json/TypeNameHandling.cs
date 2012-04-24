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
  /// Specifies type name handling options for the <see cref="JsonSerializer"/>.
  /// </summary>
  [Flags]
  public enum TypeNameHandling
  {
    /// <summary>
    /// Do not include the .NET type name when serializing types.
    /// </summary>
    None = 0,
    /// <summary>
    /// Include the .NET type name when serializing into a JSON object structure.
    /// </summary>
    Objects = 1,
    /// <summary>
    /// Include the .NET type name when serializing into a JSON array structure.
    /// </summary>
    Arrays = 2,
    /// <summary>
    /// Always include the .NET type name when serializing.
    /// </summary>
    All = Objects | Arrays,
    /// <summary>
    /// Include the .NET type name when the type of the object being serialized is not the same as its declared type.
    /// </summary>
    Auto = 4
  }
}