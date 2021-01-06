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

#if (NET45 || NET50)
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Collections;
using System;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2450
    {
        [Test]
        public void Test()
        {
            var resolver = new DefaultContractResolver();
            JsonContract contract;

            contract = resolver.ResolveContract(typeof(Dict));
            Assert.IsTrue(contract is JsonDictionaryContract);

            contract = resolver.ResolveContract(typeof(Dict?));
            Assert.IsTrue(contract is JsonDictionaryContract);
        }

        [Test]
        public void Test_Serialize()
        {
            Dict d = new Dict(new Dictionary<string, object>
            {
                ["prop1"] = 1,
                ["prop2"] = 2
            });

            string json = JsonConvert.SerializeObject(d);
            Assert.AreEqual(@"{""prop1"":1,""prop2"":2}", json);
        }

        [Test]
        public void Test_Deserialize()
        {
            string json = @"{""prop1"":1,""prop2"":2}";

            var d = JsonConvert.DeserializeObject<Dict?>(json);
            Assert.AreEqual(1, d.Value["prop1"]);
            Assert.AreEqual(2, d.Value["prop2"]);
        }

        public struct Dict : IReadOnlyDictionary<string, object>
        {
            private readonly IDictionary<string, object> _dict;
            public Dict(IDictionary<string, object> dict) => _dict = dict;

            public object this[string key] => _dict[key];
            public IEnumerable<string> Keys => _dict.Keys;
            public IEnumerable<object> Values => _dict.Values;
            public int Count => _dict.Count;
            public bool ContainsKey(string key) => _dict.ContainsKey(key);
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dict.GetEnumerator();
            public bool TryGetValue(string key, out object value) => _dict.TryGetValue(key, out value);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
#endif