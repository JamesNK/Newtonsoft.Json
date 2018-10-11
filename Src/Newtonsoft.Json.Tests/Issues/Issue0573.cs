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

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using System;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue0573
    {
        [Test]
        public void Test()
        {
            string json = "{'Value':'hi'}";
            MemoryTraceWriter traceWriter = new MemoryTraceWriter { LevelFilter = TraceLevel.Info };
            PrivateSetterTestClass o = JsonConvert.DeserializeObject<PrivateSetterTestClass>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });
            List<string> messages = traceWriter.GetTraceMessages().ToList();

            bool hasMessage = messages.Any(message => message.Contains("Info Unable to deserialize value to non-writable property 'Value' on Newtonsoft.Json.Tests.Issues.Issue0573+PrivateSetterTestClass. Path 'Value', line 1, position 13."));
            Assert.IsTrue(hasMessage);
        }

        public class PrivateSetterTestClass
        {
            public string Value { get; private set; }
        }
    }
}
