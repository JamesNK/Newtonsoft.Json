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
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class SerializationEventTestObject
  {
    // This member is serialized and deserialized with no change.
    public int Member1 { get; set; }

    // The value of this field is set and reset during and 
    // after serialization.
    public string Member2 { get; set; }

    // This field is not serialized. The OnDeserializedAttribute 
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; set; }

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; set; }

    // This field is set to null, but populated after error.
    [JsonIgnore]
    public string Member5 { get; set; }

    // Getting or setting this field will throw an error.
    public string Member6
    {
      get { throw new Exception("Member5 get error!"); }
      set { throw new Exception("Member5 set error!"); }
    }

    public SerializationEventTestObject()
    {
      Member1 = 11;
      Member2 = "Hello World!";
      Member3 = "This is a nonserialized value";
      Member4 = null;
    }

    [OnSerializing]
    internal void OnSerializingMethod(StreamingContext context)
    {
      Member2 = "This value went into the data file during serialization.";
    }

    [OnSerialized]
    internal void OnSerializedMethod(StreamingContext context)
    {
      Member2 = "This value was reset after serialization.";
    }

    [OnDeserializing]
    internal void OnDeserializingMethod(StreamingContext context)
    {
      Member3 = "This value was set during deserialization";
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
      Member4 = "This value was set after deserialization.";
    }

    [OnError]
    internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
    {
      Member5 = "Error message for member " + errorContext.Member + " = " + errorContext.Error.Message;
      errorContext.Handled = true;
    }
  }

  public class DerivedSerializationEventTestObject : SerializationEventTestObject
  {
    // This field is set to null, but populated after deserialization, only
    // in the derived class
    [JsonIgnore]
    public string Member7 { get; set; }

    // These empty methods exist to make sure we're not covering up the base
    // methods
    [OnSerializing]
    internal void OnDerivedSerializingMethod(StreamingContext context)
    {
    }

    [OnSerialized]
    internal void OnDerivedSerializedMethod(StreamingContext context)
    {
    }

    [OnDeserializing]
    internal void OnDerivedDeserializingMethod(StreamingContext context)
    {
    }

    [OnDeserialized]
    internal void OnDerivedDeserializedMethod(StreamingContext context)
    {
      Member7 = "This value was set after deserialization.";
    }

    [OnError]
    internal void OnDerivedErrorMethod(StreamingContext context, ErrorContext errorContext)
    {
    }
  }
}
