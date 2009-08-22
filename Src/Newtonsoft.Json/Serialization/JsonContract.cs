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
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public abstract class JsonContract
  {
    /// <summary>
    /// Gets the underlying type for the contract.
    /// </summary>
    /// <value>The underlying type for the contract.</value>
    public Type UnderlyingType { get; private set; }
    /// <summary>
    /// Gets or sets whether this type contract is serialized as a reference.
    /// </summary>
    /// <value>Whether this type contract is serialized as a reference.</value>
    public bool? IsReference { get; set; }

#if !PocketPC && !SILVERLIGHT
    private static readonly StreamingContext SerializationStreamingContextParameter = new StreamingContext(StreamingContextStates.All);
    private static readonly object[] SerializationEventParameterValues = new object[] { SerializationStreamingContextParameter };

    /// <summary>
    /// Gets or sets the method called immediately after deserialization of the object.
    /// </summary>
    /// <value>The method called immediately after deserialization of the object.</value>
    public MethodInfo OnDeserialized { get; set; }
    /// <summary>
    /// Gets or sets the method called during deserialization of the object.
    /// </summary>
    /// <value>The method called during deserialization of the object.</value>
    public MethodInfo OnDeserializing { get; set; }
    /// <summary>
    /// Gets or sets the method called after serialization of the object graph.
    /// </summary>
    /// <value>The method called after serialization of the object graph.</value>
    public MethodInfo OnSerialized { get; set; }
    /// <summary>
    /// Gets or sets the method called before serialization of the object.
    /// </summary>
    /// <value>The method called before serialization of the object.</value>
    public MethodInfo OnSerializing { get; set; }

    public MethodInfo OnError { get; set; }
#endif

    internal void InvokeOnSerializing(object o)
    {
#if !PocketPC && !SILVERLIGHT
      if (OnSerializing != null)
        OnSerializing.Invoke(o, SerializationEventParameterValues);
#endif
    }

    internal void InvokeOnSerialized(object o)
    {
#if !PocketPC && !SILVERLIGHT
      if (OnSerialized != null)
        OnSerialized.Invoke(o, SerializationEventParameterValues);
#endif
    }

    internal void InvokeOnDeserializing(object o)
    {
#if !PocketPC && !SILVERLIGHT
      if (OnDeserializing != null)
        OnDeserializing.Invoke(o, SerializationEventParameterValues);
#endif
    }

    internal void InvokeOnDeserialized(object o)
    {
#if !PocketPC && !SILVERLIGHT
      if (OnDeserialized != null)
        OnDeserialized.Invoke(o, SerializationEventParameterValues);
#endif
    }

#if !PocketPC && !SILVERLIGHT && !NET20
    internal void InvokeOnError(object o, ErrorContext errorContext)
    {
      if (OnError != null && o == errorContext.OriginalObject)
        OnError.Invoke(o, new object[] { SerializationStreamingContextParameter, errorContext });
    }
#endif

    internal JsonContract(Type underlyingType)
    {
      ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");

      UnderlyingType = underlyingType;
    }
  }
}