﻿#region License
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
using System.ComponentModel;
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
    public class Issue1719 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            ExtensionDataTestClass a = JsonConvert.DeserializeObject<ExtensionDataTestClass>("{\"E\":null}", new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            Assert.IsNull(a.PropertyBag);
        }

        [Test]
        public void Test_PreviousWorkaround()
        {
            ExtensionDataTestClassWorkaround a = JsonConvert.DeserializeObject<ExtensionDataTestClassWorkaround>("{\"E\":null}", new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            Assert.IsNull(a.PropertyBag);
        }

        [Test]
        public void Test_DefaultValue()
        {
            ExtensionDataWithDefaultValueTestClass a = JsonConvert.DeserializeObject<ExtensionDataWithDefaultValueTestClass>("{\"E\":2}", new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });

            Assert.IsNull(a.PropertyBag);
        }

        class ExtensionDataTestClass
        {
            public B? E { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> PropertyBag { get; set; }
        }

        class ExtensionDataWithDefaultValueTestClass
        {
            [DefaultValue(2)]
            public int? E { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> PropertyBag { get; set; }
        }

        enum B
        {
            One,
            Two
        }

        class ExtensionDataTestClassWorkaround
        {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
            public B? E { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> PropertyBag { get; set; }
        }
    }
}