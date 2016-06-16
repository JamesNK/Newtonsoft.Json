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
using Newtonsoft.Json.Converters;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    public class VersionClass
    {
        public VersionClass(string version1, string version2)
        {
            StringProperty1 = "StringProperty1";
            Version1 = new Version(version1);
            Version2 = new Version(version2);
            StringProperty2 = "StringProperty2";
        }

        public VersionClass()
        {
        }

        public string StringProperty1 { get; set; }
        public Version Version1 { get; set; }
        public Version Version2 { get; set; }
        public string StringProperty2 { get; set; }
    }

    [TestFixture]
    public class VersionConverterTests : TestFixtureBase
    {
        private void SerializeVersionClass(string version1, string version2)
        {
            VersionClass versionClass = new VersionClass(version1, version2);

            string json = JsonConvert.SerializeObject(versionClass, Formatting.Indented, new VersionConverter());

            string expectedJson = string.Format(@"{{
  ""StringProperty1"": ""StringProperty1"",
  ""Version1"": ""{0}"",
  ""Version2"": ""{1}"",
  ""StringProperty2"": ""StringProperty2""
}}", version1, version2);

            StringAssert.AreEqual(expectedJson, json);
        }

        [Test]
        public void SerializeVersionClass()
        {
            SerializeVersionClass("1.0.0.0", "2.0.0.0");
            SerializeVersionClass("1.2.0.0", "2.3.0.0");
            SerializeVersionClass("1.2.3.0", "2.3.4.0");
            SerializeVersionClass("1.2.3.4", "2.3.4.5");

            SerializeVersionClass("1.2", "2.3");
            SerializeVersionClass("1.2.3", "2.3.4");
            SerializeVersionClass("1.2.3.4", "2.3.4.5");
        }

        private void DeserializeVersionClass(string version1, string version2)
        {
            string json = string.Format(@"{{""StringProperty1"": ""StringProperty1"", ""Version1"": ""{0}"", ""Version2"": ""{1}"", ""StringProperty2"": ""StringProperty2""}}", version1, version2);
            Version expectedVersion1 = new Version(version1);
            Version expectedVersion2 = new Version(version2);

            VersionClass versionClass = JsonConvert.DeserializeObject<VersionClass>(json, new VersionConverter());

            Assert.AreEqual("StringProperty1", versionClass.StringProperty1);
            Assert.AreEqual(expectedVersion1, versionClass.Version1);
            Assert.AreEqual(expectedVersion2, versionClass.Version2);
            Assert.AreEqual("StringProperty2", versionClass.StringProperty2);
        }

        [Test]
        public void DeserializeVersionClass()
        {
            DeserializeVersionClass("1.0.0.0", "2.0.0.0");
            DeserializeVersionClass("1.2.0.0", "2.3.0.0");
            DeserializeVersionClass("1.2.3.0", "2.3.4.0");
            DeserializeVersionClass("1.2.3.4", "2.3.4.5");

            DeserializeVersionClass("1.2", "2.3");
            DeserializeVersionClass("1.2.3", "2.3.4");
            DeserializeVersionClass("1.2.3.4", "2.3.4.5");
        }

        [Test]
        public void RoundtripImplicitConverter()
        {
            var version = new Version(1, 0, 0, 0);
            string reportJSON = JsonConvert.SerializeObject(version);

            //Test
            Version report2 = JsonConvert.DeserializeObject<Version>(reportJSON);
            string reportJSON2 = JsonConvert.SerializeObject(report2);

            Assert.AreEqual(reportJSON, reportJSON2);
        }
    }
}