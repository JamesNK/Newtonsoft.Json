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

#if !(NET20 || NET35 || NET40 || PORTABLE40 || PORTABLE) || NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
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
    public class Issue1597 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            string json = @"{
    ""wish"": 264,
    ""collect"": 7498,
    ""doing"": 385,
    ""on_hold"": 285,
    ""dropped"": 221
}";

            IReadOnlyDictionary<CollectionStatus, int> o = JsonConvert.DeserializeObject<IReadOnlyDictionary<CollectionStatus, int>>(json);

            Assert.AreEqual(264, o[CollectionStatus.Wish]);
            Assert.AreEqual(7498, o[CollectionStatus.Collect]);
            Assert.AreEqual(385, o[CollectionStatus.Doing]);
            Assert.AreEqual(285, o[CollectionStatus.OnHold]);
            Assert.AreEqual(221, o[CollectionStatus.Dropped]);
        }

        [Test]
        public void Test_WithNumbers()
        {
            string json = @"{
    ""0"": 264,
    ""1"": 7498,
    ""2"": 385,
    ""3"": 285,
    ""4"": 221
}";

            IReadOnlyDictionary<CollectionStatus, int> o = JsonConvert.DeserializeObject<IReadOnlyDictionary<CollectionStatus, int>>(json);

            Assert.AreEqual(264, o[CollectionStatus.Wish]);
            Assert.AreEqual(7498, o[CollectionStatus.Collect]);
            Assert.AreEqual(385, o[CollectionStatus.Doing]);
            Assert.AreEqual(285, o[CollectionStatus.OnHold]);
            Assert.AreEqual(221, o[CollectionStatus.Dropped]);
        }

        [Test]
        public void Test_Serialize()
        {
            Dictionary<CollectionStatus, int> o = new Dictionary<CollectionStatus, int>();
            o[CollectionStatus.Wish] = 264;
            o[CollectionStatus.Collect] = 7498;
            o[CollectionStatus.Doing] = 385;
            o[CollectionStatus.OnHold] = 285;
            o[CollectionStatus.Dropped] = 221;
            o[(CollectionStatus)int.MaxValue] = int.MaxValue;

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Wish"": 264,
  ""Collect"": 7498,
  ""Doing"": 385,
  ""on_hold"": 285,
  ""Dropped"": 221,
  ""2147483647"": 2147483647
}", json);
        }

        public enum CollectionStatus
        {
            Wish,
            Collect,
            Doing,
            [EnumMember(Value = "on_hold")]
            OnHold,
            Dropped
        }
    }
}
#endif