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

#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class ImmutableCollectionsTests : TestFixtureBase
    {
        #region List
        [Test]
        public void SerializeList()
        {
            ImmutableList<string> l = ImmutableList.CreateRange(new List<string>
            {
                "One",
                "II",
                "3"
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
        }

        [Test]
        public void DeserializeList()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ImmutableList<string> l = JsonConvert.DeserializeObject<ImmutableList<string>>(json);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("One", l[0]);
            Assert.AreEqual("II", l[1]);
            Assert.AreEqual("3", l[2]);
        }

        [Test]
        public void DeserializeListInterface()
        {
            string json = @"[
        'Volibear',
        'Teemo',
        'Katarina'
      ]";

            // what sorcery is this?!
            IImmutableList<string> champions = JsonConvert.DeserializeObject<IImmutableList<string>>(json);

            Console.WriteLine(champions[0]);
            // Volibear

            Assert.AreEqual(3, champions.Count);
            Assert.AreEqual("Volibear", champions[0]);
            Assert.AreEqual("Teemo", champions[1]);
            Assert.AreEqual("Katarina", champions[2]);
        }
        #endregion

        #region Array
        [Test]
        public void SerializeArray()
        {
            ImmutableArray<string> l = ImmutableArray.CreateRange(new List<string>
                {
                  "One",
                  "II",
                  "3"
                });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
        }

        [Test]
        public void DeserializeArray()
        {
            string json = @"[
          ""One"",
          ""II"",
          ""3""
        ]";

            ImmutableArray<string> l = JsonConvert.DeserializeObject<ImmutableArray<string>>(json);

            Assert.AreEqual(3, l.Length);
            Assert.AreEqual("One", l[0]);
            Assert.AreEqual("II", l[1]);
            Assert.AreEqual("3", l[2]);
        }

        [Test]
        public void SerializeDefaultArray()
        {
            ExceptionAssert.Throws<NullReferenceException>("Object reference not set to an instance of an object.", () =>
                JsonConvert.SerializeObject(default(ImmutableArray<int>), Formatting.Indented));
        }
        #endregion

        #region Queue
        [Test]
        public void SerializeQueue()
        {
            ImmutableQueue<string> l = ImmutableQueue.CreateRange(new List<string>
            {
                "One",
                "II",
                "3"
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
        }

        [Test]
        public void DeserializeQueue()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ImmutableQueue<string> l = JsonConvert.DeserializeObject<ImmutableQueue<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.AreEqual("One", l.ElementAt(0));
            Assert.AreEqual("II", l.ElementAt(1));
            Assert.AreEqual("3", l.ElementAt(2));
        }

        [Test]
        public void DeserializeQueueInterface()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            IImmutableQueue<string> l = JsonConvert.DeserializeObject<IImmutableQueue<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.AreEqual("One", l.ElementAt(0));
            Assert.AreEqual("II", l.ElementAt(1));
            Assert.AreEqual("3", l.ElementAt(2));
        }
        #endregion

        #region Stack
        [Test]
        public void SerializeStack()
        {
            ImmutableStack<string> l = ImmutableStack.CreateRange(new List<string>
            {
                "One",
                "II",
                "3"
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"[
  ""3"",
  ""II"",
  ""One""
]", json);
        }

        [Test]
        public void DeserializeStack()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ImmutableStack<string> l = JsonConvert.DeserializeObject<ImmutableStack<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.AreEqual("3", l.ElementAt(0));
            Assert.AreEqual("II", l.ElementAt(1));
            Assert.AreEqual("One", l.ElementAt(2));
        }

        [Test]
        public void DeserializeStackInterface()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            IImmutableStack<string> l = JsonConvert.DeserializeObject<IImmutableStack<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.AreEqual("3", l.ElementAt(0));
            Assert.AreEqual("II", l.ElementAt(1));
            Assert.AreEqual("One", l.ElementAt(2));
        }
        #endregion

        #region HashSet
        [Test]
        public void SerializeHashSet()
        {
            ImmutableHashSet<string> l = ImmutableHashSet.CreateRange(new List<string>
            {
                "One",
                "II",
                "3"
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            JArray a = JArray.Parse(json);
            Assert.AreEqual(3, a.Count);
            Assert.IsTrue(a.Any(t => t.DeepEquals("One")));
            Assert.IsTrue(a.Any(t => t.DeepEquals("II")));
            Assert.IsTrue(a.Any(t => t.DeepEquals("3")));
        }

        [Test]
        public void DeserializeHashSet()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ImmutableHashSet<string> l = JsonConvert.DeserializeObject<ImmutableHashSet<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.IsTrue(l.Contains("3"));
            Assert.IsTrue(l.Contains("II"));
            Assert.IsTrue(l.Contains("One"));
        }

        [Test]
        public void DeserializeHashSetInterface()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            IImmutableSet<string> l = JsonConvert.DeserializeObject<IImmutableSet<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.IsTrue(l.Contains("3"));
            Assert.IsTrue(l.Contains("II"));
            Assert.IsTrue(l.Contains("One"));
        }
        #endregion

        #region SortedSet
        [Test]
        public void SerializeSortedSet()
        {
            ImmutableSortedSet<string> l = ImmutableSortedSet.CreateRange(new List<string>
            {
                "One",
                "II",
                "3"
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"[
  ""3"",
  ""II"",
  ""One""
]", json);
        }

        [Test]
        public void DeserializeSortedSet()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            ImmutableSortedSet<string> l = JsonConvert.DeserializeObject<ImmutableSortedSet<string>>(json);

            Assert.AreEqual(3, l.Count());
            Assert.IsTrue(l.Contains("3"));
            Assert.IsTrue(l.Contains("II"));
            Assert.IsTrue(l.Contains("One"));
        }
        #endregion

        #region Dictionary
        [Test]
        public void SerializeDictionary()
        {
            ImmutableDictionary<int, string> l = ImmutableDictionary.CreateRange(new Dictionary<int, string>
            {
                { 1, "One" },
                { 2, "II" },
                { 3, "3" }
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            JObject a = JObject.Parse(json);
            Assert.AreEqual(3, a.Count);
            Assert.AreEqual("One", (string)a["1"]);
            Assert.AreEqual("II", (string)a["2"]);
            Assert.AreEqual("3", (string)a["3"]);
        }

        [Test]
        public void DeserializeDictionary()
        {
            string json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

            ImmutableDictionary<int, string> l = JsonConvert.DeserializeObject<ImmutableDictionary<int, string>>(json);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("One", l[1]);
            Assert.AreEqual("II", l[2]);
            Assert.AreEqual("3", l[3]);
        }

        [Test]
        public void DeserializeDictionaryInterface()
        {
            string json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

            IImmutableDictionary<int, string> l = JsonConvert.DeserializeObject<IImmutableDictionary<int, string>>(json);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("One", l[1]);
            Assert.AreEqual("II", l[2]);
            Assert.AreEqual("3", l[3]);
        }
        #endregion

        #region SortedDictionary
        [Test]
        public void SerializeSortedDictionary()
        {
            ImmutableSortedDictionary<int, string> l = ImmutableSortedDictionary.CreateRange(new SortedDictionary<int, string>
            {
                { 1, "One" },
                { 2, "II" },
                { 3, "3" }
            });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            Assert.AreEqual(@"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}", json);
        }

        [Test]
        public void DeserializeSortedDictionary()
        {
            string json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

            ImmutableSortedDictionary<int, string> l = JsonConvert.DeserializeObject<ImmutableSortedDictionary<int, string>>(json);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("One", l[1]);
            Assert.AreEqual("II", l[2]);
            Assert.AreEqual("3", l[3]);
        }
        #endregion
    }
}

#endif