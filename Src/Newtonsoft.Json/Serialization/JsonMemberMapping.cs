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

using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Maps a JSON property to a .NET member.
  /// </summary>
  public class JsonMemberMapping
  {
    private readonly string _propertyName;
    private readonly MemberInfo _member;
    private readonly bool _ignored;
    private readonly bool _readable;
    private readonly bool _writable;
    private readonly JsonConverter _memberConverter;
    private readonly object _defaultValue;
    private readonly bool _required;
    private readonly bool _isReference;
    private readonly NullValueHandling? _nullValueHandling;
    private readonly DefaultValueHandling? _defaultValueHandling;
    private readonly ReferenceLoopHandling? _referenceLoopHandling;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMemberMapping"/> class.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="member">The member.</param>
    /// <param name="ignored">If set to <c>true</c> mapping is ignored during serialization.</param>
    /// <param name="readable">If set to <c>true</c> mapping is readable.</param>
    /// <param name="writable">If set to <c>true</c> mapping is writable.</param>
    /// <param name="memberConverter">The member converter to use during serialization.</param>
    /// <param name="defaultValue">The mapping default value.</param>
    /// <param name="required">If set to <c>true</c> mapping is required during serialization.</param>
    /// <param name="isReference">If set to <c>true</c> mapping preserve reference.</param>
    /// <param name="nullValueHandling">Mapping null value handling.</param>
    /// <param name="defaultValueHandling">Mapping default value handling.</param>
    /// <param name="referenceLoopHandling">Mapping reference loop handling.</param>
    public JsonMemberMapping(string propertyName, MemberInfo member, bool ignored, bool readable, bool writable, JsonConverter memberConverter, object defaultValue, bool required, bool isReference, NullValueHandling? nullValueHandling, DefaultValueHandling? defaultValueHandling, ReferenceLoopHandling? referenceLoopHandling)
    {
      _propertyName = propertyName;
      _member = member;
      _ignored = ignored;
      _readable = readable;
      _writable = writable;
      _memberConverter = memberConverter;
      _defaultValue = defaultValue;
      _required = required;
      _isReference = isReference;
      _nullValueHandling = nullValueHandling;
      _defaultValueHandling = defaultValueHandling;
      _referenceLoopHandling = referenceLoopHandling;
    }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public string PropertyName
    {
      get { return _propertyName; }
    }

    /// <summary>
    /// Gets the member.
    /// </summary>
    /// <value>The member.</value>
    public MemberInfo Member
    {
      get { return _member; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonMemberMapping"/> is ignored.
    /// </summary>
    /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
    public bool Ignored
    {
      get { return _ignored; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonMemberMapping"/> is readable.
    /// </summary>
    /// <value><c>true</c> if readable; otherwise, <c>false</c>.</value>
    public bool Readable
    {
      get { return _readable; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonMemberMapping"/> is writable.
    /// </summary>
    /// <value><c>true</c> if writable; otherwise, <c>false</c>.</value>
    public bool Writable
    {
      get { return _writable; }
    }

    /// <summary>
    /// Gets the member converter.
    /// </summary>
    /// <value>The member converter.</value>
    public JsonConverter MemberConverter
    {
      get { return _memberConverter; }
    }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    /// <value>The default value.</value>
    public object DefaultValue
    {
      get { return _defaultValue; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonMemberMapping"/> is required.
    /// </summary>
    /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
    public bool Required
    {
      get { return _required; }
    }

    /// <summary>
    /// Gets a value indicating whether this mapping preserves object references.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is reference; otherwise, <c>false</c>.
    /// </value>
    public bool IsReference
    {
      get { return _isReference; }
    }

    /// <summary>
    /// Gets the mapping null value handling.
    /// </summary>
    /// <value>The null value handling.</value>
    public NullValueHandling? NullValueHandling
    {
      get { return _nullValueHandling; }
    }

    /// <summary>
    /// Gets the mapping default value handling.
    /// </summary>
    /// <value>The default value handling.</value>
    public DefaultValueHandling? DefaultValueHandling
    {
      get { return _defaultValueHandling; }
    }

    /// <summary>
    /// Gets the mapping reference loop handling.
    /// </summary>
    /// <value>The reference loop handling.</value>
    public ReferenceLoopHandling? ReferenceLoopHandling
    {
      get { return _referenceLoopHandling; }
    }
  }
}