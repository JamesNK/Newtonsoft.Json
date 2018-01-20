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
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Events;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
#if !NET20 && !PORTABLE40
using System.Xml.Linq;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class JsonSerializerCollectionsTests : TestFixtureBase
    {
#if !(NET35 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD2_0
        [Test]
        public void DeserializeConcurrentDictionaryWithNullValue()
        {
            const string key = "id";
            
            var jsonValue = $"{{\"{key}\":null}}";

            var deserializedObject = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(jsonValue);

            Assert.IsNull(deserializedObject[key]);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void SerializeConcurrentQueue()
        {
            ConcurrentQueue<int> queue1 = new ConcurrentQueue<int>();
            queue1.Enqueue(1);

            string output = JsonConvert.SerializeObject(queue1);
            Assert.AreEqual(@"[1]", output);

            ConcurrentQueue<int> queue2 = JsonConvert.DeserializeObject<ConcurrentQueue<int>>(output);
            int i;
            Assert.IsTrue(queue2.TryDequeue(out i));
            Assert.AreEqual(1, i);
        }

        [Test]
        public void SerializeConcurrentBag()
        {
            ConcurrentBag<int> bag1 = new ConcurrentBag<int>();
            bag1.Add(1);

            string output = JsonConvert.SerializeObject(bag1);
            Assert.AreEqual(@"[1]", output);

            ConcurrentBag<int> bag2 = JsonConvert.DeserializeObject<ConcurrentBag<int>>(output);
            int i;
            Assert.IsTrue(bag2.TryTake(out i));
            Assert.AreEqual(1, i);
        }

        [Test]
        public void SerializeConcurrentStack()
        {
            ConcurrentStack<int> stack1 = new ConcurrentStack<int>();
            stack1.Push(1);

            string output = JsonConvert.SerializeObject(stack1);
            Assert.AreEqual(@"[1]", output);

            ConcurrentStack<int> stack2 = JsonConvert.DeserializeObject<ConcurrentStack<int>>(output);
            int i;
            Assert.IsTrue(stack2.TryPop(out i));
            Assert.AreEqual(1, i);
        }

        [Test]
        public void SerializeConcurrentDictionary()
        {
            ConcurrentDictionary<int, int> dic1 = new ConcurrentDictionary<int, int>();
            dic1[1] = int.MaxValue;

            string output = JsonConvert.SerializeObject(dic1);
            Assert.AreEqual(@"{""1"":2147483647}", output);

            ConcurrentDictionary<int, int> dic2 = JsonConvert.DeserializeObject<ConcurrentDictionary<int, int>>(output);
            int i;
            Assert.IsTrue(dic2.TryGetValue(1, out i));
            Assert.AreEqual(int.MaxValue, i);
        }
#endif

        [Test]
        public void DoubleKey_WholeValue()
        {
            Dictionary<double, int> dictionary = new Dictionary<double, int> { { 1d, 1 } };
            string output = JsonConvert.SerializeObject(dictionary);
            Assert.AreEqual(@"{""1"":1}", output);

            Dictionary<double, int> deserializedValue = JsonConvert.DeserializeObject<Dictionary<double, int>>(output);
            Assert.AreEqual(1d, deserializedValue.First().Key);
        }

        [Test]
        public void DoubleKey_MaxValue()
        {
            Dictionary<double, int> dictionary = new Dictionary<double, int> { { double.MaxValue, 1 } };
            string output = JsonConvert.SerializeObject(dictionary);
            Assert.AreEqual(@"{""1.7976931348623157E+308"":1}", output);

            Dictionary<double, int> deserializedValue = JsonConvert.DeserializeObject<Dictionary<double, int>>(output);
            Assert.AreEqual(double.MaxValue, deserializedValue.First().Key);
        }

        [Test]
        public void FloatKey_MaxValue()
        {
            Dictionary<float, int> dictionary = new Dictionary<float, int> { { float.MaxValue, 1 } };
            string output = JsonConvert.SerializeObject(dictionary);
            Assert.AreEqual(@"{""3.40282347E+38"":1}", output);

            Dictionary<float, int> deserializedValue = JsonConvert.DeserializeObject<Dictionary<float, int>>(output);
            Assert.AreEqual(float.MaxValue, deserializedValue.First().Key);
        }

        public class TestCollectionPrivateParameterized : IEnumerable<int>
        {
            private readonly List<int> _bars;

            public TestCollectionPrivateParameterized()
            {
                _bars = new List<int>();
            }

            [JsonConstructor]
            private TestCollectionPrivateParameterized(IEnumerable<int> bars)
            {
                _bars = new List<int>(bars);
            }

            public void Add(int bar)
            {
                _bars.Add(bar);
            }

            public IEnumerator<int> GetEnumerator() => _bars.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Test]
        public void CollectionJsonConstructorPrivateParameterized()
        {
            TestCollectionPrivateParameterized c1 = new TestCollectionPrivateParameterized();
            c1.Add(0);
            c1.Add(1);
            c1.Add(2);
            string json = JsonConvert.SerializeObject(c1);
            TestCollectionPrivateParameterized c2 = JsonConvert.DeserializeObject<TestCollectionPrivateParameterized>(json);

            List<int> values = c2.ToList();

            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(0, values[0]);
            Assert.AreEqual(1, values[1]);
            Assert.AreEqual(2, values[2]);
        }

        public class TestCollectionPrivate : List<int>
        {
            [JsonConstructor]
            private TestCollectionPrivate()
            {
            }

            public static TestCollectionPrivate Create()
            {
                return new TestCollectionPrivate();
            }
        }

        [Test]
        public void CollectionJsonConstructorPrivate()
        {
            TestCollectionPrivate c1 = TestCollectionPrivate.Create();
            c1.Add(0);
            c1.Add(1);
            c1.Add(2);
            string json = JsonConvert.SerializeObject(c1);
            TestCollectionPrivate c2 = JsonConvert.DeserializeObject<TestCollectionPrivate>(json);

            List<int> values = c2.ToList();

            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(0, values[0]);
            Assert.AreEqual(1, values[1]);
            Assert.AreEqual(2, values[2]);
        }

        public class TestCollectionMultipleParameters : List<int>
        {
            [JsonConstructor]
            public TestCollectionMultipleParameters(string s1, string s2)
            {
            }
        }

        [Test]
        public void CollectionJsonConstructorMultipleParameters()
        {
            ExceptionAssert.Throws<JsonException>(
                () => JsonConvert.SerializeObject(new TestCollectionMultipleParameters(null, null)),
                "Constructor for 'Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+TestCollectionMultipleParameters' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Int32]'.");
        }

        public class TestCollectionBadIEnumerableParameter : List<int>
        {
            [JsonConstructor]
            public TestCollectionBadIEnumerableParameter(List<string> l)
            {
            }
        }

        [Test]
        public void CollectionJsonConstructorBadIEnumerableParameter()
        {
            ExceptionAssert.Throws<JsonException>(
                () => JsonConvert.SerializeObject(new TestCollectionBadIEnumerableParameter(null)),
                "Constructor for 'Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+TestCollectionBadIEnumerableParameter' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Int32]'.");
        }

#if !(DNXCORE50 || PORTABLE) || NETSTANDARD2_0
        public class TestCollectionNonGeneric : ArrayList
        {
            [JsonConstructor]
            public TestCollectionNonGeneric(IEnumerable l)
                : base(l.Cast<object>().ToList())
            {
            }
        }

        [Test]
        public void CollectionJsonConstructorNonGeneric()
        {
            string json = @"[1,2,3]";
            TestCollectionNonGeneric l = JsonConvert.DeserializeObject<TestCollectionNonGeneric>(json);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(1L, l[0]);
            Assert.AreEqual(2L, l[1]);
            Assert.AreEqual(3L, l[2]);
        }
#endif

        public class TestDictionaryPrivateParameterized : Dictionary<string, int>
        {
            public TestDictionaryPrivateParameterized()
            {
            }

            [JsonConstructor]
            private TestDictionaryPrivateParameterized(IEnumerable<KeyValuePair<string, int>> bars)
                : base(bars.ToDictionary(k => k.Key, k => k.Value))
            {
            }
        }

        [Test]
        public void DictionaryJsonConstructorPrivateParameterized()
        {
            TestDictionaryPrivateParameterized c1 = new TestDictionaryPrivateParameterized();
            c1.Add("zero", 0);
            c1.Add("one", 1);
            c1.Add("two", 2);
            string json = JsonConvert.SerializeObject(c1);
            TestDictionaryPrivateParameterized c2 = JsonConvert.DeserializeObject<TestDictionaryPrivateParameterized>(json);

            Assert.AreEqual(3, c2.Count);
            Assert.AreEqual(0, c2["zero"]);
            Assert.AreEqual(1, c2["one"]);
            Assert.AreEqual(2, c2["two"]);
        }

        public class TestDictionaryPrivate : Dictionary<string, int>
        {
            [JsonConstructor]
            private TestDictionaryPrivate()
            {
            }

            public static TestDictionaryPrivate Create()
            {
                return new TestDictionaryPrivate();
            }
        }

        [Test]
        public void DictionaryJsonConstructorPrivate()
        {
            TestDictionaryPrivate c1 = TestDictionaryPrivate.Create();
            c1.Add("zero", 0);
            c1.Add("one", 1);
            c1.Add("two", 2);
            string json = JsonConvert.SerializeObject(c1);
            TestDictionaryPrivate c2 = JsonConvert.DeserializeObject<TestDictionaryPrivate>(json);

            Assert.AreEqual(3, c2.Count);
            Assert.AreEqual(0, c2["zero"]);
            Assert.AreEqual(1, c2["one"]);
            Assert.AreEqual(2, c2["two"]);
        }

        public class TestDictionaryMultipleParameters : Dictionary<string, int>
        {
            [JsonConstructor]
            public TestDictionaryMultipleParameters(string s1, string s2)
            {
            }
        }

        [Test]
        public void DictionaryJsonConstructorMultipleParameters()
        {
            ExceptionAssert.Throws<JsonException>(
                () => JsonConvert.SerializeObject(new TestDictionaryMultipleParameters(null, null)),
                "Constructor for 'Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+TestDictionaryMultipleParameters' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Collections.Generic.KeyValuePair`2[System.String,System.Int32]]'.");
        }

        public class TestDictionaryBadIEnumerableParameter : Dictionary<string, int>
        {
            [JsonConstructor]
            public TestDictionaryBadIEnumerableParameter(Dictionary<string, string> l)
            {
            }
        }

        [Test]
        public void DictionaryJsonConstructorBadIEnumerableParameter()
        {
            ExceptionAssert.Throws<JsonException>(
                () => JsonConvert.SerializeObject(new TestDictionaryBadIEnumerableParameter(null)),
                "Constructor for 'Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+TestDictionaryBadIEnumerableParameter' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Collections.Generic.KeyValuePair`2[System.String,System.Int32]]'.");
        }

#if !(DNXCORE50 || PORTABLE) || NETSTANDARD2_0
        public class TestDictionaryNonGeneric : Hashtable
        {
            [JsonConstructor]
            public TestDictionaryNonGeneric(IDictionary d)
                : base(d)
            {
            }
        }

        [Test]
        public void DictionaryJsonConstructorNonGeneric()
        {
            string json = @"{'zero':0,'one':1,'two':2}";
            TestDictionaryNonGeneric d = JsonConvert.DeserializeObject<TestDictionaryNonGeneric>(json);

            Assert.AreEqual(3, d.Count);
            Assert.AreEqual(0L, d["zero"]);
            Assert.AreEqual(1L, d["one"]);
            Assert.AreEqual(2L, d["two"]);
        }
#endif

#if !(DNXCORE50) || NETSTANDARD2_0
        public class NameValueCollectionTestClass
        {
            public NameValueCollection Collection { get; set; }
        }

        [Test]
        public void DeserializeNameValueCollection()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<NameValueCollectionTestClass>("{Collection:[]}"),
                "Cannot create and populate list type System.Collections.Specialized.NameValueCollection. Path 'Collection', line 1, position 13.");
        }
#endif

#if !(NET35 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD2_0
        public class SomeObject
        {
            public string Text1 { get; set; }
        }

        public class CustomConcurrentDictionary : ConcurrentDictionary<string, List<SomeObject>>
        {
            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                ((IDictionary)this).Add("key2", new List<SomeObject>
                {
                    new SomeObject
                    {
                        Text1 = "value2"
                    }
                });
            }
        }

        [Test]
        public void SerializeCustomConcurrentDictionary()
        {
            IDictionary d = new CustomConcurrentDictionary();
            d.Add("key", new List<SomeObject>
            {
                new SomeObject
                {
                    Text1 = "value1"
                }
            });

            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            Assert.AreEqual(@"{
  ""key"": [
    {
      ""Text1"": ""value1""
    }
  ]
}", json);

            CustomConcurrentDictionary d2 = JsonConvert.DeserializeObject<CustomConcurrentDictionary>(json);

            Assert.AreEqual(2, d2.Count);
            Assert.AreEqual("value1", d2["key"][0].Text1);
            Assert.AreEqual("value2", d2["key2"][0].Text1);
        }
#endif

        [Test]
        public void NonZeroBasedArray()
        {
            var onebasedArray = Array.CreateInstance(typeof(string), new[] { 3 }, new[] { 2 });

            for (var i = onebasedArray.GetLowerBound(0); i <= onebasedArray.GetUpperBound(0); i++)
            {
                onebasedArray.SetValue(i.ToString(CultureInfo.InvariantCulture), new[] { i, });
            }

            string output = JsonConvert.SerializeObject(onebasedArray, Formatting.Indented);

            StringAssert.AreEqual(@"[
  ""2"",
  ""3"",
  ""4""
]", output);
        }

        [Test]
        public void NonZeroBasedMultiArray()
        {
            // lets create a two dimensional array, each rank is 1-based of with a capacity of 4.
            var onebasedArray = Array.CreateInstance(typeof(string), new[] { 3, 3 }, new[] { 1, 2 });

            // Iterate of the array elements and assign a random double
            for (var i = onebasedArray.GetLowerBound(0); i <= onebasedArray.GetUpperBound(0); i++)
            {
                for (var j = onebasedArray.GetLowerBound(1); j <= onebasedArray.GetUpperBound(1); j++)
                {
                    onebasedArray.SetValue(i + "_" + j, new[] { i, j });
                }
            }

            // Now lets try and serialize the Array
            string output = JsonConvert.SerializeObject(onebasedArray, Formatting.Indented);

            StringAssert.AreEqual(@"[
  [
    ""1_2"",
    ""1_3"",
    ""1_4""
  ],
  [
    ""2_2"",
    ""2_3"",
    ""2_4""
  ],
  [
    ""3_2"",
    ""3_3"",
    ""3_4""
  ]
]", output);
        }

        [Test]
        public void MultiDObjectArray()
        {
            object[,] myOtherArray =
            {
                { new KeyValuePair<string, double>("my value", 0.8), "foobar" },
                { true, 0.4d },
                { 0.05f, 6 }
            };

            string myOtherArrayAsString = JsonConvert.SerializeObject(myOtherArray, Formatting.Indented);

            StringAssert.AreEqual(@"[
  [
    {
      ""Key"": ""my value"",
      ""Value"": 0.8
    },
    ""foobar""
  ],
  [
    true,
    0.4
  ],
  [
    0.05,
    6
  ]
]", myOtherArrayAsString);

            JObject o = JObject.Parse(@"{
              ""Key"": ""my value"",
              ""Value"": 0.8
            }");

            object[,] myOtherResult = JsonConvert.DeserializeObject<object[,]>(myOtherArrayAsString);
            Assert.IsTrue(JToken.DeepEquals(o, (JToken)myOtherResult[0, 0]));
            Assert.AreEqual("foobar", myOtherResult[0, 1]);

            Assert.AreEqual(true, myOtherResult[1, 0]);
            Assert.AreEqual(0.4, myOtherResult[1, 1]);

            Assert.AreEqual(0.05, myOtherResult[2, 0]);
            Assert.AreEqual(6L, myOtherResult[2, 1]);
        }

        public class EnumerableClass<T> : IEnumerable<T>
        {
            private readonly IList<T> _values;

            public EnumerableClass(IEnumerable<T> values)
            {
                _values = new List<T>(values);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Test]
        public void DeserializeIEnumerableFromConstructor()
        {
            string json = @"[
  1,
  2,
  null
]";

            var result = JsonConvert.DeserializeObject<EnumerableClass<int?>>(json);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(1, result.ElementAt(0));
            Assert.AreEqual(2, result.ElementAt(1));
            Assert.AreEqual(null, result.ElementAt(2));
        }

        public class EnumerableClassFailure<T> : IEnumerable<T>
        {
            private readonly IList<T> _values;

            public EnumerableClassFailure()
            {
                _values = new List<T>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Test]
        public void DeserializeIEnumerableFromConstructor_Failure()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<EnumerableClassFailure<string>>(json), "Cannot create and populate list type Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+EnumerableClassFailure`1[System.String]. Path '', line 1, position 1.");
        }

        public class PrivateDefaultCtorList<T> : List<T>
        {
            private PrivateDefaultCtorList()
            {
            }
        }

        [Test]
        public void DeserializePrivateListCtor()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]"), "Unable to find a constructor to use for type Newtonsoft.Json.Tests.Serialization.JsonSerializerCollectionsTests+PrivateDefaultCtorList`1[System.Int32]. Path '', line 1, position 1.");

            var list = JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]", new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });

            Assert.AreEqual(2, list.Count);
        }

        public class PrivateDefaultCtorWithIEnumerableCtorList<T> : List<T>
        {
            private PrivateDefaultCtorWithIEnumerableCtorList()
            {
            }

            public PrivateDefaultCtorWithIEnumerableCtorList(IEnumerable<T> values)
                : base(values)
            {
                Add(default(T));
            }
        }

        [Test]
        public void DeserializePrivateListConstructor()
        {
            var list = JsonConvert.DeserializeObject<PrivateDefaultCtorWithIEnumerableCtorList<int>>("[1,2]");

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(0, list[2]);
        }

        [Test]
        public void DeserializeNonIsoDateDictionaryKey()
        {
            Dictionary<DateTime, string> d = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(@"{""04/28/2013 00:00:00"":""test""}");

            Assert.AreEqual(1, d.Count);

            DateTime key = DateTime.Parse("04/28/2013 00:00:00", CultureInfo.InvariantCulture);
            Assert.AreEqual("test", d[key]);
        }

        [Test]
        public void DeserializeNonGenericList()
        {
            IList l = JsonConvert.DeserializeObject<IList>("['string!']");

            Assert.AreEqual(typeof(List<object>), l.GetType());
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual("string!", l[0]);
        }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
        [Test]
        public void DeserializeReadOnlyListInterface()
        {
            IReadOnlyList<int> list = JsonConvert.DeserializeObject<IReadOnlyList<int>>("[1,2,3]");

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void DeserializeReadOnlyCollectionInterface()
        {
            IReadOnlyCollection<int> list = JsonConvert.DeserializeObject<IReadOnlyCollection<int>>("[1,2,3]");

            Assert.AreEqual(3, list.Count);

            Assert.AreEqual(1, list.ElementAt(0));
            Assert.AreEqual(2, list.ElementAt(1));
            Assert.AreEqual(3, list.ElementAt(2));
        }

        [Test]
        public void DeserializeReadOnlyCollection()
        {
            ReadOnlyCollection<int> list = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>("[1,2,3]");

            Assert.AreEqual(3, list.Count);

            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void DeserializeReadOnlyDictionaryInterface()
        {
            IReadOnlyDictionary<string, int> dic = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, int>>("{'one':1,'two':2}");

            Assert.AreEqual(2, dic.Count);

            Assert.AreEqual(1, dic["one"]);
            Assert.AreEqual(2, dic["two"]);

            CustomAssert.IsInstanceOfType(typeof(ReadOnlyDictionary<string, int>), dic);
        }

        [Test]
        public void DeserializeReadOnlyDictionary()
        {
            ReadOnlyDictionary<string, int> dic = JsonConvert.DeserializeObject<ReadOnlyDictionary<string, int>>("{'one':1,'two':2}");

            Assert.AreEqual(2, dic.Count);

            Assert.AreEqual(1, dic["one"]);
            Assert.AreEqual(2, dic["two"]);
        }

        public class CustomReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            private readonly IDictionary<TKey, TValue> _dictionary;

            public CustomReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public bool ContainsKey(TKey key)
            {
                return _dictionary.ContainsKey(key);
            }

            public IEnumerable<TKey> Keys
            {
                get { return _dictionary.Keys; }
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public IEnumerable<TValue> Values
            {
                get { return _dictionary.Values; }
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }
        }

        [Test]
        public void SerializeCustomReadOnlyDictionary()
        {
            IDictionary<string, int> d = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 }
            };

            CustomReadOnlyDictionary<string, int> dic = new CustomReadOnlyDictionary<string, int>(d);

            string json = JsonConvert.SerializeObject(dic, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""one"": 1,
  ""two"": 2
}", json);
        }

        public class CustomReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            private readonly IList<T> _values;

            public CustomReadOnlyCollection(IList<T> values)
            {
                _values = values;
            }

            public int Count
            {
                get { return _values.Count; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _values.GetEnumerator();
            }
        }

        [Test]
        public void SerializeCustomReadOnlyCollection()
        {
            IList<int> l = new List<int>
            {
                1,
                2,
                3
            };

            CustomReadOnlyCollection<int> list = new CustomReadOnlyCollection<int>(l);

            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            StringAssert.AreEqual(@"[
  1,
  2,
  3
]", json);
        }
#endif

        [Test]
        public void TestEscapeDictionaryStrings()
        {
            const string s = @"host\user";
            string serialized = JsonConvert.SerializeObject(s);
            Assert.AreEqual(@"""host\\user""", serialized);

            Dictionary<int, object> d1 = new Dictionary<int, object>();
            d1.Add(5, s);
            Assert.AreEqual(@"{""5"":""host\\user""}", JsonConvert.SerializeObject(d1));

            Dictionary<string, object> d2 = new Dictionary<string, object>();
            d2.Add(s, 5);
            Assert.AreEqual(@"{""host\\user"":5}", JsonConvert.SerializeObject(d2));
        }

        public class GenericListTestClass
        {
            public List<string> GenericList { get; set; }

            public GenericListTestClass()
            {
                GenericList = new List<string>();
            }
        }

        [Test]
        public void DeserializeExistingGenericList()
        {
            GenericListTestClass c = new GenericListTestClass();
            c.GenericList.Add("1");
            c.GenericList.Add("2");

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            GenericListTestClass newValue = JsonConvert.DeserializeObject<GenericListTestClass>(json);
            Assert.AreEqual(2, newValue.GenericList.Count);
            Assert.AreEqual(typeof(List<string>), newValue.GenericList.GetType());
        }

        [Test]
        public void DeserializeSimpleKeyValuePair()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("key1", "value1"));
            list.Add(new KeyValuePair<string, string>("key2", "value2"));

            string json = JsonConvert.SerializeObject(list);

            Assert.AreEqual(@"[{""Key"":""key1"",""Value"":""value1""},{""Key"":""key2"",""Value"":""value2""}]", json);

            List<KeyValuePair<string, string>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("key1", result[0].Key);
            Assert.AreEqual("value1", result[0].Value);
            Assert.AreEqual("key2", result[1].Key);
            Assert.AreEqual("value2", result[1].Value);
        }

        [Test]
        public void DeserializeComplexKeyValuePair()
        {
            DateTime dateTime = new DateTime(2000, 12, 1, 23, 1, 1, DateTimeKind.Utc);

            List<KeyValuePair<string, WagePerson>> list = new List<KeyValuePair<string, WagePerson>>();
            list.Add(new KeyValuePair<string, WagePerson>("key1", new WagePerson
            {
                BirthDate = dateTime,
                Department = "Department1",
                LastModified = dateTime,
                HourlyWage = 1
            }));
            list.Add(new KeyValuePair<string, WagePerson>("key2", new WagePerson
            {
                BirthDate = dateTime,
                Department = "Department2",
                LastModified = dateTime,
                HourlyWage = 2
            }));

            string json = JsonConvert.SerializeObject(list, Formatting.Indented);

            StringAssert.AreEqual(@"[
  {
    ""Key"": ""key1"",
    ""Value"": {
      ""HourlyWage"": 1.0,
      ""Name"": null,
      ""BirthDate"": ""2000-12-01T23:01:01Z"",
      ""LastModified"": ""2000-12-01T23:01:01Z""
    }
  },
  {
    ""Key"": ""key2"",
    ""Value"": {
      ""HourlyWage"": 2.0,
      ""Name"": null,
      ""BirthDate"": ""2000-12-01T23:01:01Z"",
      ""LastModified"": ""2000-12-01T23:01:01Z""
    }
  }
]", json);

            List<KeyValuePair<string, WagePerson>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, WagePerson>>>(json);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("key1", result[0].Key);
            Assert.AreEqual(1, result[0].Value.HourlyWage);
            Assert.AreEqual("key2", result[1].Key);
            Assert.AreEqual(2, result[1].Value.HourlyWage);
        }

        [Test]
        public void StringListAppenderConverterTest()
        {
            Movie p = new Movie();
            p.ReleaseCountries = new List<string> { "Existing" };

            JsonConvert.PopulateObject("{'ReleaseCountries':['Appended']}", p, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringListAppenderConverter() }
            });

            Assert.AreEqual(2, p.ReleaseCountries.Count);
            Assert.AreEqual("Existing", p.ReleaseCountries[0]);
            Assert.AreEqual("Appended", p.ReleaseCountries[1]);
        }

        [Test]
        public void StringAppenderConverterTest()
        {
            Movie p = new Movie();
            p.Name = "Existing,";

            JsonConvert.PopulateObject("{'Name':'Appended'}", p, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringAppenderConverter() }
            });

            Assert.AreEqual("Existing,Appended", p.Name);
        }

        [Test]
        public void DeserializeIDictionary()
        {
            IDictionary dictionary = JsonConvert.DeserializeObject<IDictionary>("{'name':'value!'}");
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual("value!", dictionary["name"]);
        }

        [Test]
        public void DeserializeIList()
        {
            IList list = JsonConvert.DeserializeObject<IList>("['1', 'two', 'III']");
            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void NullableValueGenericDictionary()
        {
            IDictionary<string, int?> v1 = new Dictionary<string, int?>
            {
                { "First", 1 },
                { "Second", null },
                { "Third", 3 }
            };

            string json = JsonConvert.SerializeObject(v1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""First"": 1,
  ""Second"": null,
  ""Third"": 3
}", json);

            IDictionary<string, int?> v2 = JsonConvert.DeserializeObject<IDictionary<string, int?>>(json);
            Assert.AreEqual(3, v2.Count);
            Assert.AreEqual(1, v2["First"]);
            Assert.AreEqual(null, v2["Second"]);
            Assert.AreEqual(3, v2["Third"]);
        }

#if !(NET35 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD2_0
        [Test]
        public void DeserializeConcurrentDictionary()
        {
            IDictionary<string, TestObjects.Component> components = new Dictionary<string, TestObjects.Component>
            {
                { "Key!", new TestObjects.Component() }
            };
            GameObject go = new GameObject
            {
                Components = new ConcurrentDictionary<string, TestObjects.Component>(components),
                Id = "Id!",
                Name = "Name!"
            };

            string originalJson = JsonConvert.SerializeObject(go, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Components"": {
    ""Key!"": {}
  },
  ""Id"": ""Id!"",
  ""Name"": ""Name!""
}", originalJson);

            GameObject newObject = JsonConvert.DeserializeObject<GameObject>(originalJson);

            Assert.AreEqual(1, newObject.Components.Count);
            Assert.AreEqual("Id!", newObject.Id);
            Assert.AreEqual("Name!", newObject.Name);
        }
#endif

        [Test]
        public void DeserializeKeyValuePairArray()
        {
            string json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

            IList<KeyValuePair<string, IList<string>>> values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("aaa", values[0].Key);
            Assert.AreEqual(2, values[0].Value.Count);
            Assert.AreEqual("1", values[0].Value[0]);
            Assert.AreEqual("2", values[0].Value[1]);
            Assert.AreEqual("bbb", values[1].Key);
            Assert.AreEqual(2, values[1].Value.Count);
            Assert.AreEqual("3", values[1].Value[0]);
            Assert.AreEqual("4", values[1].Value[1]);
        }

        [Test]
        public void DeserializeNullableKeyValuePairArray()
        {
            string json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, null, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

            IList<KeyValuePair<string, IList<string>>?> values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>?>>(json);

            Assert.AreEqual(3, values.Count);
            Assert.AreEqual("aaa", values[0].Value.Key);
            Assert.AreEqual(2, values[0].Value.Value.Count);
            Assert.AreEqual("1", values[0].Value.Value[0]);
            Assert.AreEqual("2", values[0].Value.Value[1]);
            Assert.AreEqual(null, values[1]);
            Assert.AreEqual("bbb", values[2].Value.Key);
            Assert.AreEqual(2, values[2].Value.Value.Count);
            Assert.AreEqual("3", values[2].Value.Value[0]);
            Assert.AreEqual("4", values[2].Value.Value[1]);
        }

        [Test]
        public void DeserializeNullToNonNullableKeyValuePairArray()
        {
            string json = @"[ null ]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json); }, "Cannot convert null value to KeyValuePair. Path '[0]', line 1, position 6.");
        }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
        public class PopulateReadOnlyTestClass
        {
            public IList<int> NonReadOnlyList { get; set; }
            public IDictionary<string, int> NonReadOnlyDictionary { get; set; }

            public IList<int> Array { get; set; }

            public IList<int> List { get; set; }
            public IDictionary<string, int> Dictionary { get; set; }

            public IReadOnlyCollection<int> IReadOnlyCollection { get; set; }
            public ReadOnlyCollection<int> ReadOnlyCollection { get; set; }
            public IReadOnlyList<int> IReadOnlyList { get; set; }

            public IReadOnlyDictionary<string, int> IReadOnlyDictionary { get; set; }
            public ReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; }

            public PopulateReadOnlyTestClass()
            {
                NonReadOnlyList = new List<int> { 1 };
                NonReadOnlyDictionary = new Dictionary<string, int> { { "first", 2 } };

                Array = new[] { 3 };

                List = new ReadOnlyCollection<int>(new[] { 4 });
                Dictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "first", 5 } });

                IReadOnlyCollection = new ReadOnlyCollection<int>(new[] { 6 });
                ReadOnlyCollection = new ReadOnlyCollection<int>(new[] { 7 });
                IReadOnlyList = new ReadOnlyCollection<int>(new[] { 8 });

                IReadOnlyDictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "first", 9 } });
                ReadOnlyDictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "first", 10 } });
            }
        }

        [Test]
        public void SerializeReadOnlyCollections()
        {
            PopulateReadOnlyTestClass c1 = new PopulateReadOnlyTestClass();

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""NonReadOnlyList"": [
    1
  ],
  ""NonReadOnlyDictionary"": {
    ""first"": 2
  },
  ""Array"": [
    3
  ],
  ""List"": [
    4
  ],
  ""Dictionary"": {
    ""first"": 5
  },
  ""IReadOnlyCollection"": [
    6
  ],
  ""ReadOnlyCollection"": [
    7
  ],
  ""IReadOnlyList"": [
    8
  ],
  ""IReadOnlyDictionary"": {
    ""first"": 9
  },
  ""ReadOnlyDictionary"": {
    ""first"": 10
  }
}", json);
        }

        [Test]
        public void PopulateReadOnlyCollections()
        {
            string json = @"{
  ""NonReadOnlyList"": [
    11
  ],
  ""NonReadOnlyDictionary"": {
    ""first"": 12
  },
  ""Array"": [
    13
  ],
  ""List"": [
    14
  ],
  ""Dictionary"": {
    ""first"": 15
  },
  ""IReadOnlyCollection"": [
    16
  ],
  ""ReadOnlyCollection"": [
    17
  ],
  ""IReadOnlyList"": [
    18
  ],
  ""IReadOnlyDictionary"": {
    ""first"": 19
  },
  ""ReadOnlyDictionary"": {
    ""first"": 20
  }
}";

            var c2 = JsonConvert.DeserializeObject<PopulateReadOnlyTestClass>(json);

            Assert.AreEqual(1, c2.NonReadOnlyDictionary.Count);
            Assert.AreEqual(12, c2.NonReadOnlyDictionary["first"]);

            Assert.AreEqual(2, c2.NonReadOnlyList.Count);
            Assert.AreEqual(1, c2.NonReadOnlyList[0]);
            Assert.AreEqual(11, c2.NonReadOnlyList[1]);

            Assert.AreEqual(1, c2.Array.Count);
            Assert.AreEqual(13, c2.Array[0]);
        }
#endif

        [Test]
        public void SerializeArray2D()
        {
            Array2D aa = new Array2D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[,] { { 1, 1 }, { 1, 2 }, { 2, 1 }, { 2, 2 } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}", json);
        }

        [Test]
        public void SerializeArray3D()
        {
            Array3D aa = new Array3D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[,,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}", json);
        }

        [Test]
        public void SerializeArray3DWithConverter()
        {
            Array3DWithConverter aa = new Array3DWithConverter();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[,,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

            string json = JsonConvert.SerializeObject(aa, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Before"": ""Before!"",
  ""Coordinates"": [
    [
      [
        1.0,
        1.0,
        1.0
      ],
      [
        1.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        1.0,
        2.0,
        1.0
      ],
      [
        1.0,
        2.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        1.0,
        1.0
      ],
      [
        2.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        2.0,
        1.0
      ],
      [
        2.0,
        2.0,
        2.0
      ]
    ]
  ],
  ""After"": ""After!""
}", json);
        }

        [Test]
        public void DeserializeArray3DWithConverter()
        {
            string json = @"{
  ""Before"": ""Before!"",
  ""Coordinates"": [
    [
      [
        1.0,
        1.0,
        1.0
      ],
      [
        1.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        1.0,
        2.0,
        1.0
      ],
      [
        1.0,
        2.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        1.0,
        1.0
      ],
      [
        2.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        2.0,
        1.0
      ],
      [
        2.0,
        2.0,
        2.0
      ]
    ]
  ],
  ""After"": ""After!""
}";

            Array3DWithConverter aa = JsonConvert.DeserializeObject<Array3DWithConverter>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(4, aa.Coordinates.GetLength(0));
            Assert.AreEqual(2, aa.Coordinates.GetLength(1));
            Assert.AreEqual(3, aa.Coordinates.GetLength(2));
            Assert.AreEqual(1, aa.Coordinates[0, 0, 0]);
            Assert.AreEqual(2, aa.Coordinates[1, 1, 1]);
        }

        [Test]
        public void DeserializeArray2D()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}";

            Array2D aa = JsonConvert.DeserializeObject<Array2D>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(4, aa.Coordinates.GetLength(0));
            Assert.AreEqual(2, aa.Coordinates.GetLength(1));
            Assert.AreEqual(1, aa.Coordinates[0, 0]);
            Assert.AreEqual(2, aa.Coordinates[1, 1]);

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void DeserializeArray2D_WithTooManyItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2,3],[2,1],[2,2]],""After"":""After!""}";

            ExceptionAssert.Throws<Exception>(() => JsonConvert.DeserializeObject<Array2D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Test]
        public void DeserializeArray2D_WithTooFewItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1],[2,1],[2,2]],""After"":""After!""}";

            ExceptionAssert.Throws<Exception>(() => JsonConvert.DeserializeObject<Array2D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Test]
        public void DeserializeArray3D()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(4, aa.Coordinates.GetLength(0));
            Assert.AreEqual(2, aa.Coordinates.GetLength(1));
            Assert.AreEqual(3, aa.Coordinates.GetLength(2));
            Assert.AreEqual(1, aa.Coordinates[0, 0, 0]);
            Assert.AreEqual(2, aa.Coordinates[1, 1, 1]);

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void DeserializeArray3D_WithTooManyItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            ExceptionAssert.Throws<Exception>(() => JsonConvert.DeserializeObject<Array3D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Test]
        public void DeserializeArray3D_WithBadItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],{}]],""After"":""After!""}";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Array3D>(json), "Unexpected token when deserializing multidimensional array: StartObject. Path 'Coordinates[3][1]', line 1, position 99.");
        }

        [Test]
        public void DeserializeArray3D_WithTooFewItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            ExceptionAssert.Throws<Exception>(() => JsonConvert.DeserializeObject<Array3D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Test]
        public void SerializeEmpty3DArray()
        {
            Array3D aa = new Array3D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new int[0, 0, 0];

            string json = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}", json);
        }

        [Test]
        public void DeserializeEmpty3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(0, aa.Coordinates.GetLength(0));
            Assert.AreEqual(0, aa.Coordinates.GetLength(1));
            Assert.AreEqual(0, aa.Coordinates.GetLength(2));

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void DeserializeIncomplete3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/[1/*hi*/,/*hi*/1/*hi*/,1]/*hi*/,/*hi*/[1,1";

            ExceptionAssert.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
        }

        [Test]
        public void DeserializeIncompleteNotTopLevel3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/";

            ExceptionAssert.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
        }

        [Test]
        public void DeserializeNull3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":null,""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(null, aa.Coordinates);

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void DeserializeSemiEmpty3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[]],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(1, aa.Coordinates.GetLength(0));
            Assert.AreEqual(0, aa.Coordinates.GetLength(1));
            Assert.AreEqual(0, aa.Coordinates.GetLength(2));

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void SerializeReferenceTracked3DArray()
        {
            Event1 e1 = new Event1
            {
                EventName = "EventName!"
            };
            Event1[,] array1 = new[,] { { e1, e1 }, { e1, e1 } };
            IList<Event1[,]> values1 = new List<Event1[,]>
            {
                array1,
                array1
            };

            string json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""$id"": ""2"",
      ""$values"": [
        [
          {
            ""$id"": ""3"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$ref"": ""3""
          }
        ],
        [
          {
            ""$ref"": ""3""
          },
          {
            ""$ref"": ""3""
          }
        ]
      ]
    },
    {
      ""$ref"": ""2""
    }
  ]
}", json);
        }

        [Test]
        public void SerializeTypeName3DArray()
        {
            Event1 e1 = new Event1
            {
                EventName = "EventName!"
            };
            Event1[,] array1 = new[,] { { e1, e1 }, { e1, e1 } };
            IList<Event1[,]> values1 = new List<Event1[,]>
            {
                array1,
                array1
            };

            string json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"{
  ""$type"": """ + ReflectionUtils.GetTypeName(typeof(List<Event1[,]>), 0, DefaultSerializationBinder.Instance) + @""",
  ""$values"": [
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1[,], Newtonsoft.Json.Tests"",
      ""$values"": [
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ]
      ]
    },
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1[,], Newtonsoft.Json.Tests"",
      ""$values"": [
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Events.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ]
      ]
    }
  ]
}", json);

            IList<Event1[,]> values2 = (IList<Event1[,]>)JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Assert.AreEqual(2, values2.Count);
            Assert.AreEqual("EventName!", values2[0][0, 0].EventName);
        }

        [Test]
        public void PrimitiveValuesInObjectArray()
        {
            string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",null],""type"":""rpc"",""tid"":2}";

            ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

            Assert.AreEqual("Router", o.Action);
            Assert.AreEqual("Navigate", o.Method);
            Assert.AreEqual(2, o.Data.Length);
            Assert.AreEqual("dashboard", o.Data[0]);
            Assert.AreEqual(null, o.Data[1]);
        }

        [Test]
        public void ComplexValuesInObjectArray()
        {
            string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",[""id"", 1, ""teststring"", ""test""],{""one"":1}],""type"":""rpc"",""tid"":2}";

            ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

            Assert.AreEqual("Router", o.Action);
            Assert.AreEqual("Navigate", o.Method);
            Assert.AreEqual(3, o.Data.Length);
            Assert.AreEqual("dashboard", o.Data[0]);
            CustomAssert.IsInstanceOfType(typeof(JArray), o.Data[1]);
            Assert.AreEqual(4, ((JArray)o.Data[1]).Count);
            CustomAssert.IsInstanceOfType(typeof(JObject), o.Data[2]);
            Assert.AreEqual(1, ((JObject)o.Data[2]).Count);
            Assert.AreEqual(1, (int)((JObject)o.Data[2])["one"]);
        }

#if !(DNXCORE50) || NETSTANDARD2_0
        [Test]
        public void SerializeArrayAsArrayList()
        {
            string jsonText = @"[3, ""somestring"",[1,2,3],{}]";
            ArrayList o = JsonConvert.DeserializeObject<ArrayList>(jsonText);

            Assert.AreEqual(4, o.Count);
            Assert.AreEqual(3, ((JArray)o[2]).Count);
            Assert.AreEqual(0, ((JObject)o[3]).Count);
        }
#endif

        [Test]
        public void SerializeMemberGenericList()
        {
            Name name = new Name("The Idiot in Next To Me");

            PhoneNumber p1 = new PhoneNumber("555-1212");
            PhoneNumber p2 = new PhoneNumber("444-1212");

            name.pNumbers.Add(p1);
            name.pNumbers.Add(p2);

            string json = JsonConvert.SerializeObject(name, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""personsName"": ""The Idiot in Next To Me"",
  ""pNumbers"": [
    {
      ""phoneNumber"": ""555-1212""
    },
    {
      ""phoneNumber"": ""444-1212""
    }
  ]
}", json);

            Name newName = JsonConvert.DeserializeObject<Name>(json);

            Assert.AreEqual("The Idiot in Next To Me", newName.personsName);

            // not passed in as part of the constructor but assigned to pNumbers property
            Assert.AreEqual(2, newName.pNumbers.Count);
            Assert.AreEqual("555-1212", newName.pNumbers[0].phoneNumber);
            Assert.AreEqual("444-1212", newName.pNumbers[1].phoneNumber);
        }

        [TestFixture]
        public class MultipleDefinedPropertySerialization
        {
            [Test]
            public void SerializePropertyDefinedInMultipleInterfaces()
            {
                const string propertyValue = "value";

                var list = new List<ITestInterface> { new TestClass { Property = propertyValue } };

                var json = JsonConvert.SerializeObject(list);

                StringAssert.AreEqual($"[{{\"Property\":\"{propertyValue}\"}}]", json);
            }

            public interface IFirstInterface
            {
                string Property { get; set; }
            }

            public interface ISecondInterface
            {
                string Property { get; set; }
            }

            public interface ITestInterface : IFirstInterface, ISecondInterface
            {
            }

            public class TestClass : ITestInterface
            {
                public string Property { get; set; }
            }
        }

        [Test]
        public void CustomCollectionSerialization()
        {
            ProductCollection collection = new ProductCollection()
            {
                new Product() { Name = "Test1" },
                new Product() { Name = "Test2" },
                new Product() { Name = "Test3" }
            };

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            StringWriter sw = new StringWriter();

            jsonSerializer.Serialize(sw, collection);

            Assert.AreEqual(@"[{""Name"":""Test1"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test2"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test3"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null}]",
                sw.GetStringBuilder().ToString());

            ProductCollection collectionNew = (ProductCollection)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(sw.GetStringBuilder().ToString())), typeof(ProductCollection));

            CollectionAssert.AreEqual(collection, collectionNew);
        }

        [Test]
        public void GenericCollectionInheritance()
        {
            string json;

            GenericClass<GenericItem<string>, string> foo1 = new GenericClass<GenericItem<string>, string>();
            foo1.Items.Add(new GenericItem<string> { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo1 });
            Assert.AreEqual(@"{""selectList"":[{""Value"":""Hello""}]}", json);

            GenericClass<NonGenericItem, string> foo2 = new GenericClass<NonGenericItem, string>();
            foo2.Items.Add(new NonGenericItem { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo2 });
            Assert.AreEqual(@"{""selectList"":[{""Value"":""Hello""}]}", json);

            NonGenericClass foo3 = new NonGenericClass();
            foo3.Items.Add(new NonGenericItem { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo3 });
            Assert.AreEqual(@"{""selectList"":[{""Value"":""Hello""}]}", json);
        }

        [Test]
        public void InheritedListSerialize()
        {
            Article a1 = new Article("a1");
            Article a2 = new Article("a2");

            ArticleCollection articles1 = new ArticleCollection();
            articles1.Add(a1);
            articles1.Add(a2);

            string jsonText = JsonConvert.SerializeObject(articles1);

            ArticleCollection articles2 = JsonConvert.DeserializeObject<ArticleCollection>(jsonText);

            Assert.AreEqual(articles1.Count, articles2.Count);
            Assert.AreEqual(articles1[0].Name, articles2[0].Name);
        }

        [Test]
        public void ReadOnlyCollectionSerialize()
        {
            ReadOnlyCollection<int> r1 = new ReadOnlyCollection<int>(new int[] { 0, 1, 2, 3, 4 });

            string jsonText = JsonConvert.SerializeObject(r1);

            Assert.AreEqual("[0,1,2,3,4]", jsonText);

            ReadOnlyCollection<int> r2 = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>(jsonText);

            CollectionAssert.AreEqual(r1, r2);
        }

        [Test]
        public void SerializeGenericList()
        {
            Product p1 = new Product
            {
                Name = "Product 1",
                Price = 99.95m,
                ExpiryDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            };
            Product p2 = new Product
            {
                Name = "Product 2",
                Price = 12.50m,
                ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            };

            List<Product> products = new List<Product>();
            products.Add(p1);
            products.Add(p2);

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);
            //[
            //  {
            //    "Name": "Product 1",
            //    "ExpiryDate": "\/Date(978048000000)\/",
            //    "Price": 99.95,
            //    "Sizes": null
            //  },
            //  {
            //    "Name": "Product 2",
            //    "ExpiryDate": "\/Date(1248998400000)\/",
            //    "Price": 12.50,
            //    "Sizes": null
            //  }
            //]

            StringAssert.AreEqual(@"[
  {
    ""Name"": ""Product 1"",
    ""ExpiryDate"": ""2000-12-29T00:00:00Z"",
    ""Price"": 99.95,
    ""Sizes"": null
  },
  {
    ""Name"": ""Product 2"",
    ""ExpiryDate"": ""2009-07-31T00:00:00Z"",
    ""Price"": 12.50,
    ""Sizes"": null
  }
]", json);
        }

        [Test]
        public void DeserializeGenericList()
        {
            string json = @"[
        {
          ""Name"": ""Product 1"",
          ""ExpiryDate"": ""\/Date(978048000000)\/"",
          ""Price"": 99.95,
          ""Sizes"": null
        },
        {
          ""Name"": ""Product 2"",
          ""ExpiryDate"": ""\/Date(1248998400000)\/"",
          ""Price"": 12.50,
          ""Sizes"": null
        }
      ]";

            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);

            Product p1 = products[0];

            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("Product 1", p1.Name);
        }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
        [Test]
        public void ReadOnlyIntegerList()
        {
            ReadOnlyIntegerList l = new ReadOnlyIntegerList(new List<int>
            {
                1,
                2,
                3,
                int.MaxValue
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            StringAssert.AreEqual(@"[
  1,
  2,
  3,
  2147483647
]", json);
        }
#endif

#if !DNXCORE50 || NETSTANDARD2_0
        [Test]
        public void EmptyStringInHashtableIsDeserialized()
        {
            string externalJson = @"{""$type"":""System.Collections.Hashtable, mscorlib"",""testkey"":""""}";

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            JsonConvert.SerializeObject(new Hashtable { { "testkey", "" } }, settings);
            Hashtable deserializeTest2 = JsonConvert.DeserializeObject<Hashtable>(externalJson, settings);

            Assert.AreEqual(deserializeTest2["testkey"], "");
        }
#endif

        [Test]
        public void DeserializeCollectionWithConstructorArrayArgument()
        {
            var v = new ReadOnlyCollectionWithArrayArgument<double>(new[] { -0.014147478859765236, -0.011419606805541858, -0.010038461483676238 });
            var json = JsonConvert.SerializeObject(v);

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<ReadOnlyCollectionWithArrayArgument<double>>(json);
            }, "Unable to find a constructor to use for type Newtonsoft.Json.Tests.Serialization.ReadOnlyCollectionWithArrayArgument`1[System.Double]. Path '', line 1, position 1.");
        }

#if !NET20 && !PORTABLE40
        [Test]
        public void NonDefaultConstructor_DuplicateKeyInDictionary_Replace()
        {
            string json = @"{ ""user"":""bpan"", ""Person"":{ ""groups"":""replaced!"", ""domain"":""adm"", ""mail"":""bpan@sdu.dk"", ""sn"":""Pan"", ""gn"":""Benzhi"", ""cn"":""Benzhi Pan"", ""eo"":""BQHLJaVTMr0eWsi1jaIut4Ls/pSuMeNEmsWfWsfKo="", ""guid"":""9A38CE8E5B288942A8DA415CF5E687"", ""employeenumber"":""2674"", ""omk1"":""930"", ""language"":""da"" }, ""XMLResponce"":""<?xml version='1.0' encoding='iso-8859-1' ?>\n<cas:serviceResponse xmlns:cas='http://www.yale.edu/tp/cas'>\n\t<cas:authenticationSuccess>\n\t\t<cas:user>bpan</cas:user>\n\t\t<norEduPerson>\n\t\t\t<groups>FNC-PRI-APP-SUNDB-EDOR-A,FNC-RI-APP-SUB-EDITOR-B</groups>\n\t\t\t<domain>adm</domain>\n\t\t\t<mail>bpan@sdu.dk</mail>\n\t\t\t<sn>Pan</sn>\n\t\t\t<gn>Benzhi</gn>\n\t\t\t<cn>Benzhi Pan</cn>\n\t\t\t<eo>BQHLJaVTMr0eWsi1jaIut4Lsfr/pSuMeNEmsWfWsfKo=</eo>\n\t\t\t<guid>9A38CE8E5B288942A8DA415C2C687</guid>\n\t\t\t<employeenumber>274</employeenumber>\n\t\t\t<omk1>930</omk1>\n\t\t\t<language>da</language>\n\t\t</norEduPerson>\n\t</cas:authenticationSuccess>\n</cas:serviceResponse>\n"", ""Language"":1, ""Groups"":[ ""FNC-PRI-APP-SNDB-EDOR-A"", ""FNC-PI-APP-SUNDB-EDOR-B"" ], ""Domain"":""adm"", ""Mail"":""bpan@sdu.dk"", ""Surname"":""Pan"", ""Givenname"":""Benzhi"", ""CommonName"":""Benzhi Pan"", ""OrganizationName"":null }";

            var result = JsonConvert.DeserializeObject<CASResponce>(json);

            Assert.AreEqual("replaced!", result.Person["groups"]);
        }
#endif

        [Test]
        public void GenericIListAndOverrideConstructor()
        {
            MyClass deserialized = JsonConvert.DeserializeObject<MyClass>(@"[""apple"", ""monkey"", ""goose""]");

            Assert.AreEqual("apple", deserialized[0]);
            Assert.AreEqual("monkey", deserialized[1]);
            Assert.AreEqual("goose", deserialized[2]);
        }

#if !(PORTABLE || PORTABLE40)
        [Test]
        public void DeserializeCultureInfoKey()
        {
            string json = @"{ ""en-US"": ""Hi"", ""sv-SE"": ""Hej"" }";

            Dictionary<CultureInfo, string> values = JsonConvert.DeserializeObject<Dictionary<CultureInfo, string>>(json);
            Assert.AreEqual(2, values.Count);
        }
#endif

        [Test]
        public void DeserializeConstructorWithReadonlyArrayProperty()
        {
            string json = @"{""Endpoint"":""http://localhost"",""Name"":""account1"",""Dimensions"":[{""Key"":""Endpoint"",""Value"":""http://localhost""},{""Key"":""Name"",""Value"":""account1""}]}";

            AccountInfo values = JsonConvert.DeserializeObject<AccountInfo>(json);
            Assert.AreEqual("http://localhost", values.Endpoint);
            Assert.AreEqual("account1", values.Name);
            Assert.AreEqual(2, values.Dimensions.Length);
        }

        public sealed class AccountInfo
        {
            private KeyValuePair<string, string>[] metricDimensions;

            public AccountInfo(string endpoint, string name)
            {
                this.Endpoint = endpoint;
                this.Name = name;
            }

            public string Endpoint { get; }

            public string Name { get; }

            public KeyValuePair<string, string>[] Dimensions =>
                this.metricDimensions ?? (this.metricDimensions = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Endpoint", this.Endpoint.ToString()),
                    new KeyValuePair<string, string>("Name", this.Name)
                });
        }

        public class MyClass : IList<string>
        {
            private List<string> _storage;

            [JsonConstructor]
            private MyClass()
            {
                _storage = new List<string>();
            }

            public MyClass(IEnumerable<string> source)
            {
                _storage = new List<string>(source);
            }

            //Below is generated by VS to implement IList<string>
            public string this[int index]
            {
                get
                {
                    return ((IList<string>)_storage)[index];
                }

                set
                {
                    ((IList<string>)_storage)[index] = value;
                }
            }

            public int Count
            {
                get
                {
                    return ((IList<string>)_storage).Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return ((IList<string>)_storage).IsReadOnly;
                }
            }

            public void Add(string item)
            {
                ((IList<string>)_storage).Add(item);
            }

            public void Clear()
            {
                ((IList<string>)_storage).Clear();
            }

            public bool Contains(string item)
            {
                return ((IList<string>)_storage).Contains(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                ((IList<string>)_storage).CopyTo(array, arrayIndex);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return ((IList<string>)_storage).GetEnumerator();
            }

            public int IndexOf(string item)
            {
                return ((IList<string>)_storage).IndexOf(item);
            }

            public void Insert(int index, string item)
            {
                ((IList<string>)_storage).Insert(index, item);
            }

            public bool Remove(string item)
            {
                return ((IList<string>)_storage).Remove(item);
            }

            public void RemoveAt(int index)
            {
                ((IList<string>)_storage).RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList<string>)_storage).GetEnumerator();
            }
        }
    }

#if !NET20 && !PORTABLE40
    public class CASResponce
    {
        //<?xml version='1.0' encoding='iso-8859-1' ?>
        //<cas:serviceResponse xmlns:cas='http://www.yale.edu/tp/cas'>
        //    <cas:authenticationSuccess>
        //        <cas:user>and</cas:user>
        //        <norEduPerson>
        //            <groups>IT-service-OD,USR-IT-service,IT-service-udvikling</groups>
        //            <domain>adm</domain>
        //            <mail>and@sdu.dk</mail>
        //            <sn>And</sn>
        //            <gn>Anders</gn>
        //            <cn>Anders And</cn>
        //            <eo>QQT3tKSKjCxQSGsDiR8HTP9L5VsojBvOYyjOu8pwLMA=</eo>
        //            <guid>DE423352CC763649B8F2ECF1DA304750</guid>
        //            <language>da</language>  
        //        </norEduPerson>
        //    </cas:authenticationSuccess>
        //</cas:serviceResponse>

        // NemID
        //<cas:serviceResponse xmlns:cas="http://www.yale.edu/tp/cas">
        //  <cas:authenticationSuccess>
        //      <cas:user>
        //          2903851921
        //      </cas:user>
        //  </cas:authenticationSuccess>
        //</cas:serviceResponse>


        //WAYF
        //<cas:serviceResponse xmlns:cas="http://www.yale.edu/tp/cas">
        //  <cas:authenticationSuccess>
        //     <cas:user>
        //          jj@testidp.wayf.dk
        //     </cas:user>
        //  <norEduPerson>
        //     <sn>Jensen</sn>
        //     <gn>Jens</gn>
        //     <cn>Jens farmer</cn>
        //      <eduPersonPrincipalName>jj @testidp.wayf.dk</eduPersonPrincipalName>
        //        <mail>jens.jensen @institution.dk</mail>
        //        <organizationName>Institution</organizationName>
        //        <eduPersonAssurance>2</eduPersonAssurance>
        //        <schacPersonalUniqueID>urn:mace:terena.org:schac:personalUniqueID:dk:CPR:0708741234</schacPersonalUniqueID>
        //        <eduPersonScopedAffiliation>student @course1.testidp.wayf.dk</eduPersonScopedAffiliation>
        //        <eduPersonScopedAffiliation>staff @course1.testidp.wayf.dk</eduPersonScopedAffiliation>
        //        <eduPersonScopedAffiliation>staff @course1.testidp.wsayf.dk</eduPersonScopedAffiliation>
        //        <preferredLanguage>en</preferredLanguage>
        //        <eduPersonEntitlement>test</eduPersonEntitlement>
        //        <eduPersonPrimaryAffiliation>student</eduPersonPrimaryAffiliation>
        //        <schacCountryOfCitizenship>DK</schacCountryOfCitizenship>
        //        <eduPersonTargetedID>WAYF-DK-7a86d1c3b69a9639d7650b64f2eb773bd21a8c6d</eduPersonTargetedID>
        //        <schacHomeOrganization>testidp.wayf.dk</schacHomeOrganization>
        //        <givenName>Jens</givenName>
        //      <o>Institution</o>
        //     <idp>https://testbridge.wayf.dk</idp>
        //  </norEduPerson>
        // </cas:authenticationSuccess>
        //</cas:serviceResponse>


        public enum ssoLanguage
        {
            Unknown,
            Danish,
            English
        }


        public CASResponce(string xmlResponce)
        {
            this.Domain = "";
            this.Mail = "";
            this.Surname = "";
            this.Givenname = "";
            this.CommonName = "";

            ParseReplyXML(xmlResponce);
            ExtractGroups();
            ExtractLanguage();
        }

        private void ExtractGroups()
        {
            this.Groups = new List<string>();
            if (this.Person.ContainsKey("groups"))
            {
                string groupsString = this.Person["groups"];
                string[] stringList = groupsString.Split(',');

                foreach (string group in stringList)
                {
                    this.Groups.Add(group);
                }
            }

        }

        private void ExtractLanguage()
        {
            if (Person.ContainsKey("language"))
            {
                switch (Person["language"].Trim())
                {
                    case "da":
                        this.Language = ssoLanguage.Danish;
                        break;
                    case "en":
                        this.Language = ssoLanguage.English;
                        break;
                    default:
                        this.Language = ssoLanguage.Unknown;
                        break;
                }
            }
            else
            {
                this.Language = ssoLanguage.Unknown;
            }
        }




        private void ParseReplyXML(string xmlString)
        {
            try
            {
                System.Xml.Linq.XDocument xDoc = XDocument.Parse(xmlString);

                var root = xDoc.Root;

                string ns = "http://www.yale.edu/tp/cas";

                XElement auth = root.Element(XName.Get("authenticationSuccess", ns));

                if (auth == null)
                    auth = root.Element(XName.Get("authenticationFailure", ns));

                XElement xNodeUser = auth.Element(XName.Get("user", ns));

                XElement eduPers = auth.Element(XName.Get("norEduPerson", ""));

                string casUser = "";
                Dictionary<string, string> eduPerson = new Dictionary<string, string>();

                if (xNodeUser != null)
                {
                    casUser = xNodeUser.Value;

                    if (eduPers != null)
                    {
                        foreach (XElement xPersonValue in eduPers.Elements())
                        {
                            if (!eduPerson.ContainsKey(xPersonValue.Name.LocalName))
                            {
                                eduPerson.Add(xPersonValue.Name.LocalName, xPersonValue.Value);
                            }
                            else
                            {
                                eduPerson[xPersonValue.Name.LocalName] = eduPerson[xPersonValue.Name.LocalName] + ";" + xPersonValue.Value;
                            }
                        }
                    }
                }

                if (casUser.Trim() != "")
                {
                    this.user = casUser;
                }

                if (eduPerson.ContainsKey("domain"))
                    this.Domain = eduPerson["domain"];
                if (eduPerson.ContainsKey("organizationName"))
                    this.OrganizationName = eduPerson["organizationName"];
                if (eduPerson.ContainsKey("mail"))
                    this.Mail = eduPerson["mail"];
                if (eduPerson.ContainsKey("sn"))
                    this.Surname = eduPerson["sn"];
                if (eduPerson.ContainsKey("gn"))
                    this.Givenname = eduPerson["gn"];
                if (eduPerson.ContainsKey("cn"))
                    this.CommonName = eduPerson["cn"];

                this.Person = eduPerson;
                this.XMLResponce = xmlString;
            }
            catch
            {
                this.user = "";

            }
        }

        /// <summary>
        /// Fast felt der altid findes.
        /// </summary>
        public string user { get; private set; }

        /// <summary>
        /// Person type som dictionary indeholdende de ekstra informationer returneret ved login.
        /// </summary>
        public Dictionary<string, string> Person { get; private set; }

        /// <summary>
        /// Den oprindelige xml returneret fra CAS.
        /// </summary>
        public string XMLResponce { get; private set; }

        /// <summary>
        /// Det sprog der benyttes i SSO. Muligheder er da eller en.
        /// </summary>
        public ssoLanguage Language { get; private set; }

        /// <summary>
        /// Liste af grupper som man er medlem af. Kun udvalgt iblandt dem der blev puttet ind i systemet.
        /// </summary>
        public List<string> Groups { get; private set; }

        public string Domain { get; private set; }

        public string Mail { get; private set; }

        public string Surname { get; private set; }

        public string Givenname { get; private set; }

        public string CommonName { get; private set; }

        public string OrganizationName { get; private set; }

    }
#endif

    public class ReadOnlyCollectionWithArrayArgument<T> : IList<T>
    {
        private readonly IList<T> _values;

        public ReadOnlyCollectionWithArrayArgument(T[] args)
        {
            _values = args ?? (IList<T>)new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
    public class ReadOnlyIntegerList : IReadOnlyCollection<int>
    {
        private readonly List<int> _list;

        public ReadOnlyIntegerList(List<int> l)
        {
            _list = l;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
#endif

    public class Array2D
    {
        public string Before { get; set; }
        public int[,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3D
    {
        public string Before { get; set; }
        public int[,,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3DWithConverter
    {
        public string Before { get; set; }

        [JsonProperty(ItemConverterType = typeof(IntToFloatConverter))]
        public int[,,] Coordinates { get; set; }

        public string After { get; set; }
    }

    public class GenericItem<T>
    {
        public T Value { get; set; }
    }

    public class NonGenericItem : GenericItem<string>
    {
    }

    public class GenericClass<T, TValue> : IEnumerable<T>
        where T : GenericItem<TValue>, new()
    {
        public IList<T> Items { get; set; }

        public GenericClass()
        {
            Items = new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Items != null)
            {
                foreach (T item in Items)
                {
                    yield return item;
                }
            }
            else
            {
                yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NonGenericClass : GenericClass<GenericItem<string>, string>
    {
    }

    public class StringListAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> existingStrings = (List<string>)existingValue;
            List<string> newStrings = new List<string>(existingStrings);

            reader.Read();

            while (reader.TokenType != JsonToken.EndArray)
            {
                string s = (string)reader.Value;
                newStrings.Add(s);

                reader.Read();
            }

            return newStrings;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>));
        }
    }

    public class StringAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string existingString = (string)existingValue;
            string newString = existingString + (string)reader.Value;

            return newString;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(string));
        }
    }
}