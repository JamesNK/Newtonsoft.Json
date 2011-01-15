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
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonObjectContract : JsonContract
  {
    /// <summary>
    /// Gets or sets the object member serialization.
    /// </summary>
    /// <value>The member object serialization.</value>
    public MemberSerialization MemberSerialization { get; set; }

    /// <summary>
    /// Gets the object's properties.
    /// </summary>
    /// <value>The object's properties.</value>
    public JsonPropertyCollection Properties { get; private set; }

    /// <summary>
    /// Gets or sets the override constructor used to create the object.
    /// This is set when a constructor is marked up using the
    /// JsonConstructor attribute.
    /// </summary>
    /// <value>The override constructor.</value>
    public ConstructorInfo OverrideConstructor { get; set; }

    /// <summary>
    /// Gets or sets the parametrized constructor used to create the object.
    /// </summary>
    /// <value>The parametrized constructor.</value>
    public ConstructorInfo ParametrizedConstructor { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonObjectContract(Type underlyingType)
      : base(underlyingType)
    {
      Properties = new JsonPropertyCollection(UnderlyingType);
    }
  }
}