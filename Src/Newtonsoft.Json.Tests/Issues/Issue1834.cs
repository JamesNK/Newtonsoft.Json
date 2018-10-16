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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
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
    public class Issue1834 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            string json = "{'foo':'test!'}";
            ItemWithJsonConstructor c = JsonConvert.DeserializeObject<ItemWithJsonConstructor>(json);

            Assert.IsNull(c.ExtensionData);
        }

        [Test]
        public void Test_UnsetRequired()
        {
            string json = "{'foo':'test!'}";
            ItemWithJsonConstructorAndDefaultValue c = JsonConvert.DeserializeObject<ItemWithJsonConstructorAndDefaultValue>(json);

            Assert.IsNull(c.ExtensionData);
        }

        public class ItemWithJsonConstructor
        {
            [JsonExtensionData]
            public IDictionary<string, JToken> ExtensionData;

            [JsonConstructor]
            private ItemWithJsonConstructor(string foo)
            {
                Foo = foo;
            }

            [JsonProperty(PropertyName = "foo", Required = Required.Always)]
            public string Foo { get; set; }
        }

        public class ItemWithJsonConstructorAndDefaultValue
        {
            [JsonExtensionData]
            public IDictionary<string, JToken> ExtensionData;

            [JsonConstructor]
            private ItemWithJsonConstructorAndDefaultValue(string foo)
            {
                Foo = foo;
            }

            [JsonProperty("foo")]
            public string Foo { get; set; }

            [JsonProperty(PropertyName = "bar", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            [System.ComponentModel.DefaultValue("default")]
            public string Bar { get; set; }
        }
    }
}