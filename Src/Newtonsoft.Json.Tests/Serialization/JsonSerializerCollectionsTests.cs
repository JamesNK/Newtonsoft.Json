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
#if !(NET35 || NET20 || PORTABLE || ASPNETCORE50 || PORTABLE40)
using System.Collections.Concurrent;
#endif
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
using Newtonsoft.Json.Linq;
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
    public class JsonSerializerCollectionsTests : TestFixtureBase
    {
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

#if !(NET35 || NET20 || PORTABLE || ASPNETCORE50 || PORTABLE40)
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
            aa.Coordinates = new[, ,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}", json);
        }

        [Test]
        public void SerializeArray3DWithConverter()
        {
            Array3DWithConverter aa = new Array3DWithConverter();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[, ,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

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
  ""$type"": ""System.Collections.Generic.List`1[[Newtonsoft.Json.Tests.TestObjects.Event1[,], Newtonsoft.Json.Tests]], mscorlib"",
  ""$values"": [
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1[,], Newtonsoft.Json.Tests"",
      ""$values"": [
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ]
      ]
    },
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1[,], Newtonsoft.Json.Tests"",
      ""$values"": [
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Event1, Newtonsoft.Json.Tests"",
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

#if !(NETFX_CORE || ASPNETCORE50)
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

            Console.WriteLine(products.Count);
            // 2

            Product p1 = products[0];

            Console.WriteLine(p1.Name);
            // Product 1

            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("Product 1", products[0].Name);
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
        public int[, ,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3DWithConverter
    {
        public string Before { get; set; }

        [JsonProperty(ItemConverterType = typeof(IntToFloatConverter))]
        public int[, ,] Coordinates { get; set; }

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
                yield break;
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