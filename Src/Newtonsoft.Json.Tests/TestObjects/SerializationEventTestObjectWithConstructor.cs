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

#if !PocketPC
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class SerializationEventTestObjectWithConstructor
  {
    // This member is serialized and deserialized with no change.
    public int Member1 { get; private set; }

    // The value of this field is set and reset during and 
    // after serialization.
    public string Member2 { get; private set; }

    // This field is not serialized. The OnDeserializedAttribute 
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; private set; }

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; private set; }

    public SerializationEventTestObjectWithConstructor(int member1,
                                                       string member2,
                                                       string member4)
    {
      Member1 = member1;
      Member2 = member2;
      Member3 = "This is a nonserialized value";
      Member4 = member4;
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
  }
}
#endif