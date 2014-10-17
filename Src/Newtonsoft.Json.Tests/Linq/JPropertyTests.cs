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
using System.ComponentModel;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Linq;
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
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JPropertyTests : TestFixtureBase
    {
        [Test]
        public void NullValue()
        {
            JProperty p = new JProperty("TestProperty", null);
            Assert.IsNotNull(p.Value);
            Assert.AreEqual(JTokenType.Null, p.Value.Type);
            Assert.AreEqual(p, p.Value.Parent);

            p.Value = null;
            Assert.IsNotNull(p.Value);
            Assert.AreEqual(JTokenType.Null, p.Value.Type);
            Assert.AreEqual(p, p.Value.Parent);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Test]
        public void ListChanged()
        {
            JProperty p = new JProperty("TestProperty", null);
            IBindingList l = p;

            ListChangedType? listChangedType = null;
            int? index = null;

            l.ListChanged += (sender, args) =>
            {
                listChangedType = args.ListChangedType;
                index = args.NewIndex;
            };

            p.Value = 1;

            Assert.AreEqual(ListChangedType.ItemChanged, listChangedType.Value);
            Assert.AreEqual(0, index.Value);
        }
#endif

        [Test]
        public void IListCount()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            Assert.AreEqual(1, l.Count);
        }

        [Test]
        public void IListClear()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Clear(); }, "Cannot add or remove items from Newtonsoft.Json.Linq.JProperty.");
        }

        [Test]
        public void IListAdd()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Add(null); }, "Newtonsoft.Json.Linq.JProperty cannot have multiple values.");
        }

        [Test]
        public void IListRemove()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Remove(p.Value); }, "Cannot add or remove items from Newtonsoft.Json.Linq.JProperty.");
        }

        [Test]
        public void IListRemoveAt()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.RemoveAt(0); }, "Cannot add or remove items from Newtonsoft.Json.Linq.JProperty.");
        }

        [Test]
        public void JPropertyLinq()
        {
            JProperty p = new JProperty("TestProperty", null);
            IList l = p;

            List<JToken> result = l.Cast<JToken>().ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void JPropertyDeepEquals()
        {
            JProperty p1 = new JProperty("TestProperty", null);
            JProperty p2 = new JProperty("TestProperty", null);

            Assert.AreEqual(true, JToken.DeepEquals(p1, p2));
        }

        [Test]
        public void JPropertyIndexOf()
        {
            JValue v = new JValue(1);
            JProperty p1 = new JProperty("TestProperty", v);

            IList l1 = p1;
            Assert.AreEqual(0, l1.IndexOf(v));

            IList<JToken> l2 = p1;
            Assert.AreEqual(0, l2.IndexOf(v));
        }

        [Test]
        public void JPropertyContains()
        {
            JValue v = new JValue(1);
            JProperty p = new JProperty("TestProperty", v);

            Assert.AreEqual(true, p.Contains(v));
            Assert.AreEqual(false, p.Contains(new JValue(1)));
        }

        [Test]
        public void Load()
        {
            JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
            reader.Read();

            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            JProperty property = JProperty.Load(reader);
            Assert.AreEqual("propertyname", property.Name);
            Assert.IsTrue(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
            reader.Read();

            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            property = JProperty.Load(reader);
            Assert.AreEqual("propertyname", property.Name);
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), property.Value));

            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public void MultiContentConstructor()
        {
            JProperty p = new JProperty("error", new List<string> { "one", "two" });
            JArray a = (JArray)p.Value;

            Assert.AreEqual(a.Count, 2);
            Assert.AreEqual("one", (string)a[0]);
            Assert.AreEqual("two", (string)a[1]);
        }

        [Test]
        public void IListGenericAdd()
        {
            IList<JToken> t = new JProperty("error", new List<string> { "one", "two" });

            ExceptionAssert.Throws<JsonException>(() => { t.Add(1); }, "Newtonsoft.Json.Linq.JProperty cannot have multiple values.");
        }
    }
}