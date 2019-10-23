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

using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class NamingStrategyOverrideTests : TestFixtureBase
    {
        [Test]
        public void SerializeObject_OverrideNamingStrategyInObject_RespectsNamingStrategyOverride()
        {
            var input = new UsesOverriddenNamingStrategy
            {
                Id = 42,
                FullName = "Joe Bloggs",
                ExtraThings = new Dictionary<string, int>
                {
                    ["ExtraThing"] = 1
                },
                PreserveDictionaryKeys = new Dictionary<string, int>
                {
                    ["PreserveKey"] = 2
                }
            };

            string actual = JsonConvert.SerializeObject(input, Formatting.Indented);
            string expected = @"{
  ""id"": 42,
  ""full_name"": ""Joe Bloggs"",
  ""extra_things"": {
    ""extra_thing"": 1
  },
  ""preserve_dictionary_keys"": {
    ""PreserveKey"": 2
  }
}";

            StringAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Overrides the default global naming strategy
        /// </summary>
        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy),
                    NamingStrategyParameters = new object[] { true, true })]
        public class UsesOverriddenNamingStrategy
        {
            public int Id { get; set; }

            public string FullName { get; set; }

            /// <summary>
            /// Override the dictionary keys, as inherited from the class
            /// </summary>
            public IDictionary<string, int> ExtraThings { get; set; }

            /// <summary>
            /// Don't override the the dictionary keys on just this property
            /// </summary>
            [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy),
                          NamingStrategyParameters = new object[] { false, false })]
            public IDictionary<string, int> PreserveDictionaryKeys { get; set; }
        }
    }
}
