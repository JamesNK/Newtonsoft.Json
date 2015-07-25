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
    public class NumericConversionHandlingTests : TestFixtureBase
    {
        public class TestObject
        {            
            public int Integer { get; set; }            
            public float Float { get; set; }
            public string StrInt { get; set; }
            public string StrFloat { get; set; }
            public string Str { get; set; }         
        }


        [Test]
        public void MemberNameMatchHandlingTest()
        {            
            Assert.DoesNotThrow(() =>
                JsonConvert.DeserializeObject<TestObject>(@"{""Integer"": 5, ""Float"": 5.5, ""StrInt"": 5, ""StrFloat"": 5.5, ""Str"": ""aaa""}",
                    new JsonSerializerSettings()));

            Assert.DoesNotThrow(() =>
                JsonConvert.DeserializeObject<TestObject>(@"{""Integer"": 5, ""Float"": 5.5, ""StrInt"": 5, ""StrFloat"": 5.5, ""Str"": ""aaa""}",
                    new JsonSerializerSettings {NumericConversionHandling = NumericConversionHandling.AllowQuotes}));
            
            ExceptionAssert.Throws<Exception>(() =>
                JsonConvert.DeserializeObject<TestObject>(@"{""Integer"": ""5"", ""Float"": 5.5, ""StrInt"": 5, ""StrFloat"": 5.5, ""Str"": ""aaa""}",
                    new JsonSerializerSettings {NumericConversionHandling = NumericConversionHandling.ProhibitQuotes}),
                @"Error converting value ""5"" to type 'System.Int32'. Path 'Integer', line 1, position 15.");

            ExceptionAssert.Throws<Exception>(() =>
                JsonConvert.DeserializeObject<TestObject>(@"{""Interger"": 5, ""Float"": ""5.5"", ""StrInt"": 5, ""StrFloat"": 5.5, ""Str"": ""aaa""}",
                    new JsonSerializerSettings {NumericConversionHandling = NumericConversionHandling.ProhibitQuotes}),
                @"Error converting value ""5.5"" to type 'System.Single'. Path 'Float', line 1, position 30.");
        }
    }
}


