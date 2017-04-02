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
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(NET20)
    [Serializable]
    [DataContract]
    public class TestClass
    {
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Name = "Rick";

        [DataMember]
        public DateTime Now
        {
            get { return _Now; }
            set { _Now = value; }
        }

        private DateTime _Now = DateTime.Now;

        [DataMember]
        public decimal BigNumber
        {
            get { return _BigNumber; }
            set { _BigNumber = value; }
        }

        private decimal _BigNumber = 1212121.22M;

        [DataMember]
        public Address Address1
        {
            get { return _Address1; }
            set { _Address1 = value; }
        }

        private Address _Address1 = new Address();

        [DataMember]
        public List<Address> Addresses
        {
            get { return _Addresses; }
            set { _Addresses = value; }
        }

        private List<Address> _Addresses = new List<Address>();

        [DataMember]
        public List<string> strings = new List<string>();

        [DataMember]
        public Dictionary<string, int> dictionary = new Dictionary<string, int>();
    }
#endif
}