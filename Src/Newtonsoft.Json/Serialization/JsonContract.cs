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
#if !(NET35 || NET20)
    Dynamic,
#endif
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
    Serializable,
#endif
    Linq
  }

  /// <summary>
  /// Delegate for OnSerializing, OnSerialized, OnDeserializing, OnDeserialized methods.
  /// </summary>
  public delegate void SerializingCallback(object o, StreamingContext context);

  /// <summary>
  /// Delegate for OnError method.
  /// </summary>
  public delegate void SerializingErrorCallback(object o, StreamingContext context, ErrorContext errorContext);

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
    public List<SerializingCallback> OnDeserializedActions { get; private set; }

    /// <summary>
    /// Gets or sets all methods called during deserialization of the object.
    /// </summary>
    /// <value>The methods called during deserialization of the object.</value>
    public List<SerializingCallback> OnDeserializingActions { get; private set; }

    /// <summary>
    /// Gets or sets all methods called after serialization of the object graph.
    /// </summary>
    /// <value>The methods called after serialization of the object graph.</value>
    public List<SerializingCallback> OnSerializedActions { get; private set; }

    /// <summary>
    /// Gets or sets all methods called before serialization of the object.
    /// </summary>
    /// <value>The methods called before serialization of the object.</value>
    public List<SerializingCallback> OnSerializingActions { get; private set; }

    /// <summary>
    /// Gets or sets the method called immediately after deserialization of the object.
    /// </summary>
    /// <value>The method called immediately after deserialization of the object.</value>
    public MethodInfo OnDeserialized
    {
      get { return (OnDeserializedActions.Count > 0) ? OnDeserializedActions[0].Method : null; }
      set 
      {
        OnDeserializedActions.Clear();
        OnDeserializedActions.Add(SerializingMethodToAction(value));
      }
    }

    /// <summary>
    /// Gets or sets the method called during deserialization of the object.
    /// </summary>
    /// <value>The method called during deserialization of the object.</value>
    public MethodInfo OnDeserializing 
    {
      get { return (OnDeserializingActions.Count > 0) ? OnDeserializingActions[0].Method : null; }
      set 
      {
        OnDeserializingActions.Clear();
        OnDeserializingActions.Add(SerializingMethodToAction(value));
      }
    }

    /// <summary>
    /// Gets or sets the method called after serialization of the object graph.
    /// </summary>
    /// <value>The method called after serialization of the object graph.</value>
    public MethodInfo OnSerialized 
    {
      get { return (OnSerializedActions.Count > 0) ? OnSerializedActions[0].Method : null; }
      set 
      {
        OnSerializedActions.Clear();
        OnSerializedActions.Add(SerializingMethodToAction(value));
      }
    }

    /// <summary>
    /// Gets or sets the method called before serialization of the object.
    /// </summary>
    /// <value>The method called before serialization of the object.</value>
    public MethodInfo OnSerializing 
    {
      get { return (OnSerializingActions.Count > 0) ? OnSerializingActions[0].Method : null; }
      set 
      {
        OnSerializingActions.Clear();
        OnSerializingActions.Add(SerializingMethodToAction(value));
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
    public List<SerializingErrorCallback> OnErrorActions { get; private set; }

    /// <summary>
    /// Gets or sets the method called when an error is thrown during the serialization of the object.
    /// </summary>
    /// <value>The method called when an error is thrown during the serialization of the object.</value>
    public MethodInfo OnError
    {
      get { return (OnErrorActions.Count > 0) ? OnErrorActions[0].Method : null; }
      set 
      {
        OnErrorActions.Clear();
        OnErrorActions.Add(SerializingErrorMethodToAction(value));
      }
    }

    internal void InvokeOnSerializing(object o, StreamingContext context)
    {
      foreach (SerializingCallback action in OnSerializingActions)
      {
        action(o, context);
      }
    }

    internal void InvokeOnSerialized(object o, StreamingContext context)
    {
      foreach (SerializingCallback action in OnSerializedActions)
      {
        action(o, context);
      }
    }

    internal void InvokeOnDeserializing(object o, StreamingContext context)
    {
      foreach (SerializingCallback action in OnDeserializingActions)
      {
        action(o, context);
      }
    }

    internal void InvokeOnDeserialized(object o, StreamingContext context)
    {
      foreach (SerializingCallback action in OnDeserializedActions)
      {
        action(o, context);
      }
    }

    internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
    {
      foreach (SerializingErrorCallback action in OnErrorActions)
      {
        action(o, context, errorContext);
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

      OnErrorActions = new List<SerializingErrorCallback>();
      OnSerializedActions = new List<SerializingCallback>();
      OnSerializingActions = new List<SerializingCallback>();
      OnDeserializedActions = new List<SerializingCallback>();
      OnDeserializingActions = new List<SerializingCallback>();
    }

    internal static SerializingCallback SerializingMethodToAction(MethodInfo callbackMethodInfo)
    {
      return (o, context) => callbackMethodInfo.Invoke(o, new object[] { context });
    }

    internal static SerializingErrorCallback SerializingErrorMethodToAction(MethodInfo callbackMethodInfo)
    {
      return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] { context, econtext });
    }
  }
}