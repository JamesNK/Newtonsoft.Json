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
  /// Specifies reference handling options for the <see cref="JsonSerializer"/>.
  /// Note that references cannot be preserved when a value is set via a non-default constructor such as types that implement ISerializable.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="PreservingObjectReferencesOn" title="Preserve Object References" />       
  /// </example>
  [Flags]
  public enum PreserveReferencesHandling
  {
    /// <summary>
    /// Do not preserve references when serializing types.
    /// </summary>
    None = 0,
    /// <summary>
    /// Preserve references when serializing into a JSON object structure.
    /// </summary>
    Objects = 1,
    /// <summary>
    /// Preserve references when serializing into a JSON array structure.
    /// </summary>
    Arrays = 2,
    /// <summary>
    /// Preserve references when serializing.
    /// </summary>
    All = Objects | Arrays
  }
}
