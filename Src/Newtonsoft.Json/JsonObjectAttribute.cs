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
  /// Instructs the <see cref="JsonSerializer"/> how to serialize the object.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
  public sealed class JsonObjectAttribute : JsonContainerAttribute
  {
    private MemberSerialization _memberSerialization = MemberSerialization.OptOut;

    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal Required? _itemRequired;

    /// <summary>
    /// Gets or sets the member serialization.
    /// </summary>
    /// <value>The member serialization.</value>
    public MemberSerialization MemberSerialization
    {
      get { return _memberSerialization; }
      set { _memberSerialization = value; }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the object's properties are required.
    /// </summary>
    /// <value>
    /// 	A value indicating whether the object's properties are required.
    /// </value>
    public Required ItemRequired
    {
      get { return _itemRequired ?? default(Required); }
      set { _itemRequired = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class.
    /// </summary>
    public JsonObjectAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified member serialization.
    /// </summary>
    /// <param name="memberSerialization">The member serialization.</param>
    public JsonObjectAttribute(MemberSerialization memberSerialization)
    {
      MemberSerialization = memberSerialization;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified container Id.
    /// </summary>
    /// <param name="id">The container Id.</param>
    public JsonObjectAttribute(string id)
      : base(id)
    {
    }
  }
}