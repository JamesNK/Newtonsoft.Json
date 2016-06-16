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

#if !(NET35 || NET20 || NETFX_CORE || DNXCORE50)
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class FSharpTests : TestFixtureBase
    {
        [Test]
        public void List()
        {
            FSharpList<int> l = ListModule.OfSeq(new List<int> { 1, 2, 3 });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            StringAssert.AreEqual(@"[
  1,
  2,
  3
]", json);

            FSharpList<int> l2 = JsonConvert.DeserializeObject<FSharpList<int>>(json);

            Assert.AreEqual(l.Length, l2.Length);
            CollectionAssert.AreEquivalent(l, l2);
        }

        [Test]
        public void Set()
        {
            FSharpSet<int> l = SetModule.OfSeq(new List<int> { 1, 2, 3 });

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);

            StringAssert.AreEqual(@"[
  1,
  2,
  3
]", json);

            FSharpSet<int> l2 = JsonConvert.DeserializeObject<FSharpSet<int>>(json);

            Assert.AreEqual(l.Count, l2.Count);
            CollectionAssert.AreEquivalent(l, l2);
        }

        [Test]
        public void Map()
        {
            FSharpMap<string, int> m1 = MapModule.OfSeq(new List<Tuple<string, int>> { Tuple.Create("one", 1), Tuple.Create("II", 2), Tuple.Create("3", 3) });

            string json = JsonConvert.SerializeObject(m1, Formatting.Indented);

            FSharpMap<string, int> m2 = JsonConvert.DeserializeObject<FSharpMap<string, int>>(json);

            Assert.AreEqual(m1.Count, m2.Count);
            Assert.AreEqual(1, m2["one"]);
            Assert.AreEqual(2, m2["II"]);
            Assert.AreEqual(3, m2["3"]);
        }
    }
}

#endif