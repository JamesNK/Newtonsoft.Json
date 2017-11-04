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

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD1_3 || NETSTANDARD2_0
    [Serializable]
    public class PreserveReferencesCallbackTestObject : ISerializable
    {
        internal string _stringValue;
        internal int _intValue;
        internal PersonReference _person1;
        internal PersonReference _person2;
        internal PersonReference _person3;
        internal PreserveReferencesCallbackTestObject _parent;
        internal SerializationInfo _serializationInfo;

        public PreserveReferencesCallbackTestObject(string stringValue, int intValue, PersonReference p1, PersonReference p2, PersonReference p3)
        {
            _stringValue = stringValue;
            _intValue = intValue;
            _person1 = p1;
            _person2 = p2;
            _person3 = p3;
        }

        protected PreserveReferencesCallbackTestObject(SerializationInfo info, StreamingContext context)
        {
            _serializationInfo = info;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("stringValue", _stringValue);
            info.AddValue("intValue", _intValue);
            info.AddValue("person1", _person1, typeof(PersonReference));
            info.AddValue("person2", _person2, typeof(PersonReference));
            info.AddValue("person3", _person3, typeof(PersonReference));
            info.AddValue("parent", _parent, typeof(PreserveReferencesCallbackTestObject));
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (_serializationInfo == null)
            {
                return;
            }

            _stringValue = _serializationInfo.GetString("stringValue");
            _intValue = _serializationInfo.GetInt32("intValue");
            _person1 = (PersonReference)_serializationInfo.GetValue("person1", typeof(PersonReference));
            _person2 = (PersonReference)_serializationInfo.GetValue("person2", typeof(PersonReference));
            _person3 = (PersonReference)_serializationInfo.GetValue("person3", typeof(PersonReference));
            _parent = (PreserveReferencesCallbackTestObject)_serializationInfo.GetValue("parent", typeof(PreserveReferencesCallbackTestObject));

            _serializationInfo = null;
        }
    }
#endif

}
