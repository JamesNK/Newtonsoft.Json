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
using System.Globalization;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(NET20)
    [Serializable]
    [DataContract]
    public class Address
    {
        [DataMember]
        public string Street
        {
            get { return _street; }
            set { _street = value; }
        }

        private string _street = "32 Kaiea";

        [DataMember]
        public string Phone
        {
            get { return _Phone; }
            set { _Phone = value; }
        }

        private string _Phone = "(503) 814-6335";

        [DataMember]
        public DateTime Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }

        private DateTime _Entered = DateTime.Parse("01/01/2007", CultureInfo.CurrentCulture.DateTimeFormat);
    }
#endif
}