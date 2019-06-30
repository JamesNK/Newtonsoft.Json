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

#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Utilities
{
    [TestFixture]
    public class StringUtilsTests : TestFixtureBase
    {
        private static object[] toCamelCaseTestCases = new[]
        {
            new object[] {"urlValue", "URLValue"},
            new object[] {"url", "URL"},
            new object[] {"id", "ID"},
            new object[] {"i", "I"},
            new object[] {"", ""},
            new object[] {null, null},
            new object[] {"person", "Person"},
            new object[] {"iPhone", "iPhone"},
            new object[] {"iPhone", "IPhone"},
            new object[] {"i Phone", "I Phone"},
            new object[] {"i  Phone", "I  Phone"},
            new object[] {" IPhone", " IPhone"},
            new object[] {" IPhone ", " IPhone "},
            new object[] {"isCIA", "IsCIA"},
            new object[] {"vmQ", "VmQ"},
            new object[] {"xml2Json", "Xml2Json"},
            new object[] {"snAkEcAsE", "SnAkEcAsE"},
            new object[] {"snA__kEcAsE", "SnA__kEcAsE"},
            new object[] {"snA__ kEcAsE", "SnA__ kEcAsE"},
            new object[] {"already_snake_case_ ", "already_snake_case_ "},
            new object[] {"isJSONProperty", "IsJSONProperty"},
            new object[] {"shoutinG_CASE", "SHOUTING_CASE"},
            new object[] {"9999-12-31T23:59:59.9999999Z", "9999-12-31T23:59:59.9999999Z"},
            new object[] {"hi!! This is text. Time to test.", "Hi!! This is text. Time to test."},
            new object[] {"building", "BUILDING"},
            new object[] {"building Property", "BUILDING Property"},
            new object[] {"building Property", "Building Property"},
            new object[] {"building PROPERTY", "BUILDING PROPERTY"}
        };
        private static object[] toSnakeCaseTestCases = new[]
        {

            new object[] {"url_value", "URLValue"},
            new object[] {"url", "URL"},
            new object[] {"id", "ID"},
            new object[] {"i", "I"},
            new object[] {"", ""},
            new object[] {null, null},
            new object[] {"person", "Person"},
            new object[] {"i_phone", "iPhone"},
            new object[] {"i_phone", "IPhone"},
            new object[] {"i_phone", "I Phone"},
            new object[] {"i_phone", "I  Phone"},
            new object[] {"i_phone", " IPhone"},
            new object[] {"i_phone", " IPhone "},
            new object[] {"is_cia", "IsCIA"},
            new object[] {"vm_q", "VmQ"},
            new object[] {"xml2_json", "Xml2Json"},
            new object[] {"sn_ak_ec_as_e", "SnAkEcAsE"},
            new object[] {"sn_a__k_ec_as_e", "SnA__kEcAsE"},
            new object[] {"sn_a__k_ec_as_e", "SnA__ kEcAsE"},
            new object[] {"already_snake_case_", "already_snake_case_ "},
            new object[] {"is_json_property", "IsJSONProperty"},
            new object[] {"shouting_case", "SHOUTING_CASE"},
            new object[] {"9999-12-31_t23:59:59.9999999_z", "9999-12-31T23:59:59.9999999Z"},
            new object[] {"hi!!_this_is_text._time_to_test.", "Hi!! This is text. Time to test."}
        };

#if NETFRAMEWORK
        [Test]
        [TestCaseSource(nameof(toCamelCaseTestCases))]
        public void ToCamelCaseTest(string expected, string input)
        {
            Assert.AreEqual(expected, StringUtils.ToCamelCase(input));
        }

        [Test]
        [TestCaseSource(nameof(toSnakeCaseTestCases))]
        public void ToSnakeCaseTest(string expected, string input)
        {
            Assert.AreEqual(expected, StringUtils.ToSnakeCase(input));
        }
#else
        [Test]
        public void ToCamelCaseTest()
        {
            foreach (var camelCaseTestCase in toCamelCaseTestCases)
            {
                if (camelCaseTestCase is object[] parameters)
                {
                    Assert.AreEqual((string)parameters[1], StringUtils.ToCamelCase((string)parameters[0]));
                }
            }
            
        }
        [Test]
        public void ToSnakeCaseTest()
        {
            foreach (var camelCaseTestCase in toSnakeCaseTestCases)
            {
                if (camelCaseTestCase is object[] parameters)
                {
                    Assert.AreEqual((string)parameters[1], StringUtils.ToSnakeCase((string)parameters[0]));
                }
            }
            
        }
#endif
    }
}