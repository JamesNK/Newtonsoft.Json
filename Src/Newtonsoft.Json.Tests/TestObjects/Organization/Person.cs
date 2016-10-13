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
using System.ComponentModel;

namespace Newtonsoft.Json.Tests.TestObjects.Organization
{
    [JsonObject(Id = "Person", Title = "Title!", Description = "JsonObjectAttribute description!", MemberSerialization = MemberSerialization.OptIn)]
#if !(DNXCORE50)
    [Description("DescriptionAttribute description!")]
#endif
    public class Person
    {
        // "John Smith"
        [JsonProperty]
        public string Name { get; set; }

        // "2000-12-15T22:11:03"
        [JsonProperty]
        //[JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime BirthDate { get; set; }

        // new Date(976918263055)
        [JsonProperty]
        //[JsonConverter(typeof(JavaScriptDateTimeConverter))]
        public DateTime LastModified { get; set; }

        // not serialized
        public string Department { get; set; }
    }
}