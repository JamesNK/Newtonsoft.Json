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
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
  public class JsonMemberMapping
  {
    private readonly string _mappingName;
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

    public JsonMemberMapping(string mappingName, MemberInfo member, bool ignored, bool readable, bool writable, JsonConverter memberConverter, object defaultValue, bool required, bool isReference, NullValueHandling? nullValueHandling, DefaultValueHandling? defaultValueHandling, ReferenceLoopHandling? referenceLoopHandling)
    {
      _mappingName = mappingName;
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

    public string MappingName
    {
      get { return _mappingName; }
    }

    public MemberInfo Member
    {
      get { return _member; }
    }

    public bool Ignored
    {
      get { return _ignored; }
    }

    public bool Readable
    {
      get { return _readable; }
    }

    public bool Writable
    {
      get { return _writable; }
    }

    public JsonConverter MemberConverter
    {
      get { return _memberConverter; }
    }

    public object DefaultValue
    {
      get { return _defaultValue; }
    }

    public bool Required
    {
      get { return _required; }
    }

    public bool IsReference
    {
      get { return _isReference; }
    }

    public NullValueHandling? NullValueHandling
    {
      get { return _nullValueHandling; }
    }

    public DefaultValueHandling? DefaultValueHandling
    {
      get { return _defaultValueHandling; }
    }

    public ReferenceLoopHandling? ReferenceLoopHandling
    {
      get { return _referenceLoopHandling; }
    }
  }
}