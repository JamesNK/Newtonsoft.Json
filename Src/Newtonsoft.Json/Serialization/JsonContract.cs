﻿#region License
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
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;
#if NETFX_CORE
using IConvertible = Newtonsoft.Json.Utilities.Convertible;
#endif

namespace Newtonsoft.Json.Serialization
{
  internal enum JsonContractType
  {
    None,
    Object,
    Array,
    Primitive,
    String,
    Dictionary,
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
    Dynamic,
#endif
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
    Serializable,
#endif
    Linq
  }

  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public abstract class JsonContract
  {
    internal bool IsNullable;
    internal bool IsConvertable;
    internal Type NonNullableUnderlyingType;
    internal ReadType InternalReadType;
    internal JsonContractType ContractType;

    /// <summary>
    /// Gets the underlying type for the contract.
    /// </summary>
    /// <value>The underlying type for the contract.</value>
    public Type UnderlyingType { get; private set; }

    /// <summary>
    /// Gets or sets the type created during deserialization.
    /// </summary>
    /// <value>The type created during deserialization.</value>
    public Type CreatedType { get; set; }

    /// <summary>
    /// Gets or sets whether this type contract is serialized as a reference.
    /// </summary>
    /// <value>Whether this type contract is serialized as a reference.</value>
    public bool? IsReference { get; set; }

    /// <summary>
    /// Gets or sets the default <see cref="JsonConverter" /> for this contract.
    /// </summary>
    /// <value>The converter.</value>
    public JsonConverter Converter { get; set; }

    // internally specified JsonConverter's to override default behavour
    // checked for after passed in converters and attribute specified converters
    internal JsonConverter InternalConverter { get; set; }

    /// <summary>
    /// Gets or sets all methods called immediately after deserialization of the object.
    /// </summary>
    /// <value>The methods called immediately after deserialization of the object.</value>
    public List<MethodInfo> OnDeserializedMethods { get; set; }

    /// <summary>
    /// Gets or sets all methods called during deserialization of the object.
    /// </summary>
    /// <value>The methods called during deserialization of the object.</value>
    public List<MethodInfo> OnDeserializingMethods { get; set; }

    /// <summary>
    /// Gets or sets all methods called after serialization of the object graph.
    /// </summary>
    /// <value>The methods called after serialization of the object graph.</value>
    public List<MethodInfo> OnSerializedMethods { get; set; }

    /// <summary>
    /// Gets or sets all methods called before serialization of the object.
    /// </summary>
    /// <value>The methods called before serialization of the object.</value>
    public List<MethodInfo> OnSerializingMethods { get; set; }

    /// <summary>
    /// Gets or sets the method called immediately after deserialization of the object.
    /// </summary>
    /// <value>The method called immediately after deserialization of the object.</value>
    public MethodInfo OnDeserialized
    {
      get { return (OnDeserializedMethods != null && OnDeserializedMethods.Count > 0 ? OnDeserializedMethods[0] : null); }
      set 
      {
        OnDeserializedMethods = new List<MethodInfo>();
        OnDeserializedMethods.Add(value);
      }
    }

    /// <summary>
    /// Gets or sets the method called during deserialization of the object.
    /// </summary>
    /// <value>The method called during deserialization of the object.</value>
    public MethodInfo OnDeserializing 
    {
      get { return (OnDeserializingMethods != null && OnDeserializingMethods.Count > 0 ? OnDeserializingMethods[0] : null); }
      set 
      {
        OnDeserializingMethods = new List<MethodInfo>();
        OnDeserializingMethods.Add(value);
      }
    }

    /// <summary>
    /// Gets or sets the method called after serialization of the object graph.
    /// </summary>
    /// <value>The method called after serialization of the object graph.</value>
    public MethodInfo OnSerialized 
    {
      get { return (OnSerializedMethods != null && OnSerializedMethods.Count > 0 ? OnSerializedMethods[0] : null); }
      set 
      {
        OnSerializedMethods = new List<MethodInfo>();
        OnSerializedMethods.Add(value);
      }
    }

    /// <summary>
    /// Gets or sets the method called before serialization of the object.
    /// </summary>
    /// <value>The method called before serialization of the object.</value>
    public MethodInfo OnSerializing 
    {
      get { return (OnSerializingMethods != null && OnSerializingMethods.Count > 0 ? OnSerializingMethods[0] : null); }
      set 
      {
        OnSerializingMethods = new List<MethodInfo>();
        OnSerializingMethods.Add(value);
      }
    }

    /// <summary>
    /// Gets or sets the default creator method used to create the object.
    /// </summary>
    /// <value>The default creator method used to create the object.</value>
    public Func<object> DefaultCreator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the default creator is non public.
    /// </summary>
    /// <value><c>true</c> if the default object creator is non-public; otherwise, <c>false</c>.</value>
    public bool DefaultCreatorNonPublic { get; set; }

    /// <summary>
    /// Gets or sets all method called when an error is thrown during the serialization of the object.
    /// </summary>
    /// <value>The methods called when an error is thrown during the serialization of the object.</value>
    public List<MethodInfo> OnErrorMethods { get; set; }

    /// <summary>
    /// Gets or sets the method called when an error is thrown during the serialization of the object.
    /// </summary>
    /// <value>The method called when an error is thrown during the serialization of the object.</value>
    public MethodInfo OnError
    {
      get { return (OnErrorMethods != null && OnErrorMethods.Count > 0 ? OnErrorMethods[0] : null); }
      set 
      {
        OnErrorMethods = new List<MethodInfo>();
        OnErrorMethods.Add(value);
      }
    }

    internal void InvokeOnSerializing(object o, StreamingContext context)
    {
      if (OnSerializingMethods == null) return;

      foreach (MethodInfo mi in OnSerializingMethods)
      {
        mi.Invoke(o, new object[] {context});
      }
    }

    internal void InvokeOnSerialized(object o, StreamingContext context)
    {
      if (OnSerializedMethods == null) return;

      foreach (MethodInfo mi in OnSerializedMethods)
      {
        mi.Invoke(o, new object[] {context});
      }
    }

    internal void InvokeOnDeserializing(object o, StreamingContext context)
    {
      if (OnDeserializingMethods == null) return;

      foreach (MethodInfo mi in OnDeserializingMethods)
      {
        mi.Invoke(o, new object[] {context});
      }
    }

    internal void InvokeOnDeserialized(object o, StreamingContext context)
    {
      if (OnDeserializedMethods == null) return;

      foreach (MethodInfo mi in OnDeserializedMethods)
      {
        mi.Invoke(o, new object[] {context});
      }
    }

    internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
    {
      if (OnErrorMethods == null) return;
      foreach (MethodInfo mi in OnErrorMethods)
      {
        mi.Invoke(o, new object[] {context, errorContext});
      }
    }

    internal JsonContract(Type underlyingType)
    {
      ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");

      UnderlyingType = underlyingType;

      IsNullable = ReflectionUtils.IsNullable(underlyingType);
      NonNullableUnderlyingType = (IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType;

      CreatedType = NonNullableUnderlyingType;

      IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);

      if (NonNullableUnderlyingType == typeof(byte[]))
      {
        InternalReadType = ReadType.ReadAsBytes;
      }
      else if (NonNullableUnderlyingType == typeof(int))
      {
        InternalReadType = ReadType.ReadAsInt32;
      }
      else if (NonNullableUnderlyingType == typeof(decimal))
      {
        InternalReadType = ReadType.ReadAsDecimal;
      }
      else if (NonNullableUnderlyingType == typeof(string))
      {
        InternalReadType = ReadType.ReadAsString;
      }
      else if (NonNullableUnderlyingType == typeof(DateTime))
      {
        InternalReadType = ReadType.ReadAsDateTime;
      }
#if !NET20
      else if (NonNullableUnderlyingType == typeof(DateTimeOffset))
      {
        InternalReadType = ReadType.ReadAsDateTimeOffset;
      }
#endif
      else
      {
        InternalReadType = ReadType.Read;
      }
    }
  }
}
