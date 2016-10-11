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
        [Test]
        public void ToCamelCaseTest()
        {
            Assert.AreEqual("urlValue", StringUtils.ToCamelCase("URLValue"));
            Assert.AreEqual("url", StringUtils.ToCamelCase("URL"));
            Assert.AreEqual("id", StringUtils.ToCamelCase("ID"));
            Assert.AreEqual("i", StringUtils.ToCamelCase("I"));
            Assert.AreEqual("", StringUtils.ToCamelCase(""));
            Assert.AreEqual(null, StringUtils.ToCamelCase(null));
            Assert.AreEqual("person", StringUtils.ToCamelCase("Person"));
            Assert.AreEqual("iPhone", StringUtils.ToCamelCase("iPhone"));
            Assert.AreEqual("iPhone", StringUtils.ToCamelCase("IPhone"));
            Assert.AreEqual("i Phone", StringUtils.ToCamelCase("I Phone"));
            Assert.AreEqual("i  Phone", StringUtils.ToCamelCase("I  Phone"));
            Assert.AreEqual(" IPhone", StringUtils.ToCamelCase(" IPhone"));
            Assert.AreEqual(" IPhone ", StringUtils.ToCamelCase(" IPhone "));
            Assert.AreEqual("isCIA", StringUtils.ToCamelCase("IsCIA"));
            Assert.AreEqual("vmQ", StringUtils.ToCamelCase("VmQ"));
            Assert.AreEqual("xml2Json", StringUtils.ToCamelCase("Xml2Json"));
            Assert.AreEqual("snAkEcAsE", StringUtils.ToCamelCase("SnAkEcAsE"));
            Assert.AreEqual("snA__kEcAsE", StringUtils.ToCamelCase("SnA__kEcAsE"));
            Assert.AreEqual("snA__ kEcAsE", StringUtils.ToCamelCase("SnA__ kEcAsE"));
            Assert.AreEqual("already_snake_case_ ", StringUtils.ToCamelCase("already_snake_case_ "));
            Assert.AreEqual("isJSONProperty", StringUtils.ToCamelCase("IsJSONProperty"));
            Assert.AreEqual("shoutinG_CASE", StringUtils.ToCamelCase("SHOUTING_CASE"));
            Assert.AreEqual("9999-12-31T23:59:59.9999999Z", StringUtils.ToCamelCase("9999-12-31T23:59:59.9999999Z"));
            Assert.AreEqual("hi!! This is text. Time to test.", StringUtils.ToCamelCase("Hi!! This is text. Time to test."));
        }

        [Test]
        public void ToSnakeCaseTest()
        {
            Assert.AreEqual("url_value", StringUtils.ToSnakeCase("URLValue"));
            Assert.AreEqual("url", StringUtils.ToSnakeCase("URL"));
            Assert.AreEqual("id", StringUtils.ToSnakeCase("ID"));
            Assert.AreEqual("i", StringUtils.ToSnakeCase("I"));
            Assert.AreEqual("", StringUtils.ToSnakeCase(""));
            Assert.AreEqual(null, StringUtils.ToSnakeCase(null));
            Assert.AreEqual("person", StringUtils.ToSnakeCase("Person"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase("iPhone"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase("IPhone"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase("I Phone"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase("I  Phone"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase(" IPhone"));
            Assert.AreEqual("i_phone", StringUtils.ToSnakeCase(" IPhone "));
            Assert.AreEqual("is_cia", StringUtils.ToSnakeCase("IsCIA"));
            Assert.AreEqual("vm_q", StringUtils.ToSnakeCase("VmQ"));
            Assert.AreEqual("xml2_json", StringUtils.ToSnakeCase("Xml2Json"));
            Assert.AreEqual("sn_ak_ec_as_e", StringUtils.ToSnakeCase("SnAkEcAsE"));
            Assert.AreEqual("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__kEcAsE"));
            Assert.AreEqual("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__ kEcAsE"));
            Assert.AreEqual("already_snake_case_", StringUtils.ToSnakeCase("already_snake_case_ "));
            Assert.AreEqual("is_json_property", StringUtils.ToSnakeCase("IsJSONProperty"));
            Assert.AreEqual("shouting_case", StringUtils.ToSnakeCase("SHOUTING_CASE"));
            Assert.AreEqual("9999-12-31_t23:59:59.9999999_z", StringUtils.ToSnakeCase("9999-12-31T23:59:59.9999999Z"));
            Assert.AreEqual("hi!!_this_is_text._time_to_test.", StringUtils.ToSnakeCase("Hi!! This is text. Time to test."));
        }
    }
}