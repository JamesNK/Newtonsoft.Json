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
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Runtime.Versioning;
using Newtonsoft.Json.Serialization;
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
    public class Issue1798
    {
        public class NonSerializableException : Exception
        {
        }

        [Test]
        public void Test()
        {
            string nonSerializableJson = null;
            string serializableJson = null;

            try
            {
                throw new NonSerializableException();
            }
            catch (Exception ex)
            {
                nonSerializableJson = JsonConvert.SerializeObject(ex, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
            }

            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                serializableJson = JsonConvert.SerializeObject(ex, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
            }

            AssertNoTargetSite(nonSerializableJson);
            AssertNoTargetSite(serializableJson);
        }

        [Test]
        public void Test_DefaultContractResolver()
        {
            DefaultContractResolver resolver = new DefaultContractResolver();

            var objectContract = (JsonObjectContract) resolver.ResolveContract(typeof(NonSerializableException));
            Assert.IsFalse(objectContract.Properties.Contains("TargetSite"));

#if (PORTABLE40 || PORTABLE) && !(NETSTANDARD2_0 || NETSTANDARD1_3)
            objectContract = (JsonObjectContract) resolver.ResolveContract(typeof(Exception));
            Assert.IsFalse(objectContract.Properties.Contains("TargetSite"));
#else
            Assert.IsInstanceOf(typeof(JsonISerializableContract), resolver.ResolveContract(typeof(Exception)));
#endif
        }

        private void AssertNoTargetSite(string json)
        {
            JObject o = JObject.Parse(json);

            if (o.ContainsKey("TargetSite"))
            {
                Assert.Fail("JSON has TargetSite property.");
            }
        }
    }
}