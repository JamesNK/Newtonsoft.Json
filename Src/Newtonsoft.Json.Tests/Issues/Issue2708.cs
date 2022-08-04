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

#if !NET20
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2708 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            string json = @"
{
  ""Name"": ""MyName"",
  ""ChildClassProp"": ""MyValue"",
}";

            var record = JsonConvert.DeserializeObject<MyRecord>(json);
            Assert.AreEqual(null, record.Name); // Not set because doesn't have DataMember
            Assert.AreEqual("MyValue", record.ChildClassProp);
        }

        [DataContract]
        public abstract class RecordBase
        {
            [JsonExtensionData]
            protected IDictionary<string, JToken> additionalData;

            public string Name { get; set; }
        }

        [DataContract]
        public class MyRecord : RecordBase
        {
            public MyRecord(string childClassProp) => ChildClassProp = childClassProp;

            [DataMember]
            public string ChildClassProp { get; set; }
        }
    }
}
#endif
