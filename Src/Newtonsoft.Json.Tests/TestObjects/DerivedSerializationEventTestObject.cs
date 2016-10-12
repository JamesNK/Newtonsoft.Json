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

using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
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