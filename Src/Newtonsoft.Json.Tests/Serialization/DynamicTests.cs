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

#if !(NET35 || NET20 || PORTABLE40)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class DynamicTests : TestFixtureBase
    {
        [Test]
        public void SerializeDynamicObject()
        {
            TestDynamicObject dynamicObject = new TestDynamicObject();
            dynamicObject.Explicit = true;

            dynamic d = dynamicObject;
            d.Int = 1;
            d.Decimal = 99.9d;
            d.ChildObject = new DynamicChildObject();

            Dictionary<string, object> values = new Dictionary<string, object>();

            IContractResolver c = DefaultContractResolver.Instance;
            JsonDynamicContract dynamicContract = (JsonDynamicContract)c.ResolveContract(dynamicObject.GetType());

            foreach (string memberName in dynamicObject.GetDynamicMemberNames())
            {
                object value;
                dynamicContract.TryGetMember(dynamicObject, memberName, out value);

                values.Add(memberName, value);
            }

            Assert.AreEqual(d.Int, values["Int"]);
            Assert.AreEqual(d.Decimal, values["Decimal"]);
            Assert.AreEqual(d.ChildObject, values["ChildObject"]);

            string json = JsonConvert.SerializeObject(dynamicObject, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Explicit"": true,
  ""Decimal"": 99.9,
  ""Int"": 1,
  ""ChildObject"": {
    ""Text"": null,
    ""Integer"": 0
  }
}", json);

            TestDynamicObject newDynamicObject = JsonConvert.DeserializeObject<TestDynamicObject>(json);
            Assert.AreEqual(true, newDynamicObject.Explicit);

            d = newDynamicObject;

            Assert.AreEqual(99.9, d.Decimal);
            Assert.AreEqual(1, d.Int);
            Assert.AreEqual(dynamicObject.ChildObject.Integer, d.ChildObject.Integer);
            Assert.AreEqual(dynamicObject.ChildObject.Text, d.ChildObject.Text);
        }

#if !(PORTABLE || PORTABLE40)
        [Test]
        public void SerializeDynamicObjectWithObjectTracking()
        {
            dynamic o = new ExpandoObject();
            o.Text = "Text!";
            o.Integer = int.MaxValue;
            o.DynamicChildObject = new DynamicChildObject
            {
                Integer = int.MinValue,
                Text = "Child text!"
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
            });

            string dynamicChildObjectTypeName = ReflectionUtils.GetTypeName(typeof(DynamicChildObject), FormatterAssemblyStyle.Full, null);
            string expandoObjectTypeName = ReflectionUtils.GetTypeName(typeof(ExpandoObject), FormatterAssemblyStyle.Full, null);

            StringAssert.AreEqual(@"{
  ""$type"": """ + expandoObjectTypeName + @""",
  ""Text"": ""Text!"",
  ""Integer"": 2147483647,
  ""DynamicChildObject"": {
    ""$type"": """ + dynamicChildObjectTypeName + @""",
    ""Text"": ""Child text!"",
    ""Integer"": -2147483648
  }
}", json);

            dynamic n = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
            });

            CustomAssert.IsInstanceOfType(typeof(ExpandoObject), n);
            Assert.AreEqual("Text!", n.Text);
            Assert.AreEqual(int.MaxValue, n.Integer);

            CustomAssert.IsInstanceOfType(typeof(DynamicChildObject), n.DynamicChildObject);
            Assert.AreEqual("Child text!", n.DynamicChildObject.Text);
            Assert.AreEqual(int.MinValue, n.DynamicChildObject.Integer);
        }
#endif

        [Test]
        public void NoPublicDefaultConstructor()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                var settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                var json = @"{
  ""contributors"": null
}";

                JsonConvert.DeserializeObject<DynamicObject>(json, settings);
            }, "Unable to find a default constructor to use for type System.Dynamic.DynamicObject. Path 'contributors', line 2, position 17.");
        }

        public class DictionaryDynamicObject : DynamicObject
        {
            public IDictionary<string, object> Values { get; private set; }

            protected DictionaryDynamicObject()
            {
                Values = new Dictionary<string, object>();
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Values[binder.Name] = value;
                return true;
            }
        }

        [Test]
        public void AllowNonPublicDefaultConstructor()
        {
            var settings = new JsonSerializerSettings();
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;

            var json = @"{
  ""contributors"": null,
  ""retweeted"": false,
  ""text"": ""Guys SX4 diesel is launched.what are your plans?catch us at #facebook http://bit.ly/dV3H1a #auto #car #maruti #india #delhi"",
  ""in_reply_to_user_id_str"": null,
  ""retweet_count"": 0,
  ""geo"": null,
  ""id_str"": ""40678260320768000"",
  ""in_reply_to_status_id"": null,
  ""source"": ""<a href=\""http://www.tweetdeck.com\"" rel=\""nofollow\"">TweetDeck</a>"",
  ""created_at"": ""Thu Feb 24 07:43:47 +0000 2011"",
  ""place"": null,
  ""coordinates"": null,
  ""truncated"": false,
  ""favorited"": false,
  ""user"": {
    ""profile_background_image_url"": ""http://a1.twimg.com/profile_background_images/206944715/twitter_bg.jpg"",
    ""url"": ""http://bit.ly/dcFwWC"",
    ""screen_name"": ""marutisuzukisx4"",
    ""verified"": false,
    ""friends_count"": 45,
    ""description"": ""This is the Official Maruti Suzuki SX4 Twitter ID! Men are Back - mail us on social (at) sx4bymaruti (dot) com"",
    ""follow_request_sent"": null,
    ""time_zone"": ""Chennai"",
    ""profile_text_color"": ""333333"",
    ""location"": ""India"",
    ""notifications"": null,
    ""profile_sidebar_fill_color"": ""efefef"",
    ""id_str"": ""196143889"",
    ""contributors_enabled"": false,
    ""lang"": ""en"",
    ""profile_background_tile"": false,
    ""created_at"": ""Tue Sep 28 12:55:15 +0000 2010"",
    ""followers_count"": 117,
    ""show_all_inline_media"": true,
    ""listed_count"": 1,
    ""geo_enabled"": true,
    ""profile_link_color"": ""009999"",
    ""profile_sidebar_border_color"": ""eeeeee"",
    ""protected"": false,
    ""name"": ""Maruti Suzuki SX4"",
    ""statuses_count"": 637,
    ""following"": null,
    ""profile_use_background_image"": true,
    ""profile_image_url"": ""http://a3.twimg.com/profile_images/1170694644/Slide1_normal.JPG"",
    ""id"": 196143889,
    ""is_translator"": false,
    ""utc_offset"": 19800,
    ""favourites_count"": 0,
    ""profile_background_color"": ""131516""
  },
  ""in_reply_to_screen_name"": null,
  ""id"": 40678260320768000,
  ""in_reply_to_status_id_str"": null,
  ""in_reply_to_user_id"": null
}";

            DictionaryDynamicObject foo = JsonConvert.DeserializeObject<DictionaryDynamicObject>(json, settings);

            Assert.AreEqual(false, foo.Values["retweeted"]);
        }

        [Test]
        public void SerializeDynamicObjectWithNullValueHandlingIgnore()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Int = int.MaxValue;
            o.ChildObject = null; // Tests an explicitly defined property of a dynamic object with a null value.
            o.DynamicChildObject = null; // vs. a completely dynamic defined property.

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            StringAssert.AreEqual(@"{
  ""Explicit"": false,
  ""Text"": ""Text!"",
  ""Int"": 2147483647
}", json);
        }

        [Test]
        public void SerializeDynamicObjectWithNullValueHandlingInclude()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Int = int.MaxValue;
            o.ChildObject = null; // Tests an explicitly defined property of a dynamic object with a null value.
            o.DynamicChildObject = null; // vs. a completely dynamic defined property.

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
            });

            StringAssert.AreEqual(@"{
  ""Explicit"": false,
  ""Text"": ""Text!"",
  ""DynamicChildObject"": null,
  ""Int"": 2147483647,
  ""ChildObject"": null
}", json);
        }

        [Test]
        public void SerializeDynamicObjectWithDefaultValueHandlingIgnore()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Int = int.MaxValue;
            o.IntDefault = 0;
            o.NUllableIntDefault = default(int?);
            o.ChildObject = null; // Tests an explicitly defined property of a dynamic object with a null value.
            o.DynamicChildObject = null; // vs. a completely dynamic defined property.

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });

            StringAssert.AreEqual(@"{
  ""Text"": ""Text!"",
  ""Int"": 2147483647
}", json);
        }
    }

    public class DynamicChildObject
    {
        public string Text { get; set; }
        public int Integer { get; set; }
    }

    public class TestDynamicObject : DynamicObject
    {
        private readonly Dictionary<string, object> _members;

        public int Int;

        [JsonProperty]
        public bool Explicit;

        public DynamicChildObject ChildObject { get; set; }

        internal Dictionary<string, object> Members
        {
            get { return _members; }
        }

        public TestDynamicObject()
        {
            _members = new Dictionary<string, object>();
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _members.Keys.Union(new[] { "Int", "ChildObject" });
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            Type targetType = binder.Type;

            if (targetType == typeof(IDictionary<string, object>) ||
                targetType == typeof(IDictionary))
            {
                result = new Dictionary<string, object>(_members);
                return true;
            }
            else
            {
                return base.TryConvert(binder, out result);
            }
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return _members.Remove(binder.Name);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _members.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _members[binder.Name] = value;
            return true;
        }
    }

    public class ErrorSettingDynamicObject : DynamicObject
    {
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }
    }
}

#endif