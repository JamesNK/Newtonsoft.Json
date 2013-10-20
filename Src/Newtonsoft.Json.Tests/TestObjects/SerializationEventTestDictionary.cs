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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class SerializationEventTestDictionary : Dictionary<decimal, string>
    {
        // This member is serialized and deserialized with no change.
        public int Member1 { get; private set; }

        // The value of this field is set and reset during and 
        // after serialization.
        public string Member2 { get; private set; }

        // This field is not serialized. The OnDeserializedAttribute 
        // is used to set the member value after serialization.
        public string Member3 { get; private set; }

        // This field is set to null, but populated after deserialization.
        public string Member4 { get; private set; }

        public SerializationEventTestDictionary()
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
            Add(decimal.MaxValue, "Inserted on serializing");
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