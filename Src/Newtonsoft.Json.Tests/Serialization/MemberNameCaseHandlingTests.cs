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
using System.IO;
using Newtonsoft.Json.Tests.TestObjects;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class MemberNameCaseHandlingTests : TestFixtureBase
    {
        public class TestObject
        {
            public string str { get; set; }            
        }

        [Test]
        public void MemberNameMatchHandlingTest()
        {
            var deserializeObject = JsonConvert.DeserializeObject<TestObject>(@"{""Str"": ""aaa""}",
                new JsonSerializerSettings());
            Assert.AreEqual("aaa", deserializeObject.str);


            deserializeObject = JsonConvert.DeserializeObject<TestObject>(@"{""str"": ""aaa""}",
                new JsonSerializerSettings {MemberNameCaseHandling = MemberNameCaseHandling.CaseInsensitive});
            Assert.AreEqual("aaa", deserializeObject.str);

            deserializeObject = JsonConvert.DeserializeObject<TestObject>(@"{""Str"": ""aaa""}",
                new JsonSerializerSettings {MemberNameCaseHandling = MemberNameCaseHandling.CaseSensitive});            
            Assert.AreEqual(null, deserializeObject.str);
        }
    }
}