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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
#if !(NET20 || NET35 || NETFX_CORE || PORTABLE)
using System.Runtime.Serialization.Json;
#endif
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class DefaultValueHandlingTests : TestFixtureBase
    {
        public class MyClass
        {
            [JsonIgnore]
            public MyEnum Status { get; set; }

            private string _data;
            public string Data
            {
                get { return _data; }
                set
                {
                    _data = value;
                    if (_data != null && _data.StartsWith("Other"))
                    {
                        this.Status = MyEnum.Other;
                    }
                }
            }
        }

        public enum MyEnum
        {
            Default = 0,
            Other
        }

        [Test]
        public void PopulateWithJsonIgnoreAttribute()
        {
            string json = "{\"Data\":\"Other with some more text\"}";

            MyClass result = JsonConvert.DeserializeObject<MyClass>(json, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
            
            Assert.AreEqual(MyEnum.Other, result.Status);
        }

        [Test]
        public void Include()
        {
            Invoice invoice = new Invoice
            {
                Company = "Acme Ltd.",
                Amount = 50.0m,
                Paid = false,
                FollowUpDays = 30,
                FollowUpEmailAddress = string.Empty,
                PaidDate = null
            };

            string included = JsonConvert.SerializeObject(invoice,
                Formatting.Indented,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Include });

            Assert.AreEqual(@"{
  ""Company"": ""Acme Ltd."",
  ""Amount"": 50.0,
  ""Paid"": false,
  ""PaidDate"": null,
  ""FollowUpDays"": 30,
  ""FollowUpEmailAddress"": """"
}", included);
        }

        [Test]
        public void SerializeInvoice()
        {
            Invoice invoice = new Invoice
            {
                Company = "Acme Ltd.",
                Amount = 50.0m,
                Paid = false,
                FollowUpDays = 30,
                FollowUpEmailAddress = string.Empty,
                PaidDate = null
            };

            string included = JsonConvert.SerializeObject(invoice,
                Formatting.Indented,
                new JsonSerializerSettings { });

            Assert.AreEqual(@"{
  ""Company"": ""Acme Ltd."",
  ""Amount"": 50.0,
  ""Paid"": false,
  ""PaidDate"": null,
  ""FollowUpDays"": 30,
  ""FollowUpEmailAddress"": """"
}", included);

            string ignored = JsonConvert.SerializeObject(invoice,
                Formatting.Indented,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            Assert.AreEqual(@"{
  ""Company"": ""Acme Ltd."",
  ""Amount"": 50.0
}", ignored);
        }

        [Test]
        public void SerializeDefaultValueAttributeTest()
        {
            string json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass(),
                Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            Assert.AreEqual(@"{""TestField1"":0,""TestProperty1"":null}", json);

            json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = int.MinValue, TestProperty1 = "NotDefault" },
                Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            Assert.AreEqual(@"{""TestField1"":-2147483648,""TestProperty1"":""NotDefault""}", json);

            json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "NotDefault" },
                Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            Assert.AreEqual(@"{""TestProperty1"":""NotDefault""}", json);

            json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "TestProperty1Value" },
                Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            Assert.AreEqual(@"{}", json);
        }

        [Test]
        public void DeserializeDefaultValueAttributeTest()
        {
            string json = "{}";

            DefaultValueAttributeTestClass c = JsonConvert.DeserializeObject<DefaultValueAttributeTestClass>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            });
            Assert.AreEqual("TestProperty1Value", c.TestProperty1);

            c = JsonConvert.DeserializeObject<DefaultValueAttributeTestClass>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
            Assert.AreEqual("TestProperty1Value", c.TestProperty1);
        }

        public class DefaultHandler
        {
            [DefaultValue(-1)]
            public int field1;

            [DefaultValue("default")]
            public string field2;
        }

        [Test]
        public void DeserializeIgnoreAndPopulate()
        {
            DefaultHandler c1 = JsonConvert.DeserializeObject<DefaultHandler>("{}", new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
            Assert.AreEqual(-1, c1.field1);
            Assert.AreEqual("default", c1.field2);

            DefaultHandler c2 = JsonConvert.DeserializeObject<DefaultHandler>("{'field1':-1,'field2':'default'}", new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
            Assert.AreEqual(-1, c2.field1);
            Assert.AreEqual("default", c2.field2);
        }

        [JsonObject]
        public class NetworkUser
        {
            [JsonProperty(PropertyName = "userId")]
            [DefaultValue(-1)]
            public long GlobalId { get; set; }

            [JsonProperty(PropertyName = "age")]
            [DefaultValue(0)]
            public int Age { get; set; }

            [JsonProperty(PropertyName = "amount")]
            [DefaultValue(0.0)]
            public decimal Amount { get; set; }

            [JsonProperty(PropertyName = "floatUserId")]
            [DefaultValue(-1.0d)]
            public float FloatGlobalId { get; set; }

            [JsonProperty(PropertyName = "firstName")]
            public string Firstname { get; set; }

            [JsonProperty(PropertyName = "lastName")]
            public string Lastname { get; set; }

            public NetworkUser()
            {
                GlobalId = -1;
                FloatGlobalId = -1.0f;
                Amount = 0.0m;
                Age = 0;
            }
        }

        [Test]
        public void IgnoreNumberTypeDifferencesWithDefaultValue()
        {
            NetworkUser user = new NetworkUser
            {
                Firstname = "blub"
            };

            string json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

            Assert.AreEqual(@"{""firstName"":""blub""}", json);
        }

        [Test]
        public void ApproxEquals()
        {
            Assert.IsTrue(MathUtils.ApproxEquals(0.0, 0.0));
            Assert.IsTrue(MathUtils.ApproxEquals(1000.0, 1000.0000000000001));

            Assert.IsFalse(MathUtils.ApproxEquals(1000.0, 1000.000000000001));
            Assert.IsFalse(MathUtils.ApproxEquals(0.0, 0.00001));
        }

#if !NET20
        [Test]
        public void EmitDefaultValueTest()
        {
            EmitDefaultValueClass c = new EmitDefaultValueClass();

#if !(NET20 || NET35 || NETFX_CORE || PORTABLE)
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(EmitDefaultValueClass));

            MemoryStream ms = new MemoryStream();
            jsonSerializer.WriteObject(ms, c);

            Assert.AreEqual("{}", Encoding.UTF8.GetString(ms.ToArray()));
#endif

            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual("{}", json);
        }
#endif

        [Test]
        public void DefaultValueHandlingPropertyTest()
        {
            DefaultValueHandlingPropertyClass c = new DefaultValueHandlingPropertyClass();

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            Assert.AreEqual(@"{
  ""IntInclude"": 0,
  ""IntDefault"": 0
}", json);

            json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            Assert.AreEqual(@"{
  ""IntInclude"": 0
}", json);

            json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include
            });

            Assert.AreEqual(@"{
  ""IntInclude"": 0,
  ""IntDefault"": 0
}", json);
        }

        [Test]
        public void DeserializeWithIgnore()
        {
            string json = @"{'Value':null,'IntValue1':1,'IntValue2':0,'IntValue3':null}";

            var o = JsonConvert.DeserializeObject<DefaultValueHandlingDeserializeHolder>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            Assert.AreEqual(int.MaxValue, o.IntValue1);
            Assert.AreEqual(int.MinValue, o.IntValue2);
            Assert.AreEqual(int.MaxValue, o.IntValue3);
            Assert.AreEqual("Derp!", o.ClassValue.Derp);
        }

        [Test]
        public void DeserializeWithPopulate()
        {
            string json = @"{}";

            var o = JsonConvert.DeserializeObject<DefaultValueHandlingDeserializePopulate>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            });

            Assert.AreEqual(1, o.IntValue1);
            Assert.AreEqual(0, o.IntValue2);
            Assert.AreEqual(null, o.ClassValue);
        }

#if !NET20
        [Test]
        public void EmitDefaultValueIgnoreAndPopulate()
        {
            string str = "{}";
            TestClass obj = JsonConvert.DeserializeObject<TestClass>(str, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });

            Assert.AreEqual("fff", obj.Field1);
        }
#endif
    }

#if !NET20
    [DataContract]
    public class TestClass
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DataMember(EmitDefaultValue = false)]
        [DefaultValue("fff")]
        public string Field1 { set; get; }
    }
#endif

    public class DefaultValueHandlingDeserialize
    {
        public string Derp { get; set; }
    }

    public class DefaultValueHandlingDeserializeHolder
    {
        public DefaultValueHandlingDeserializeHolder()
        {
            ClassValue = new DefaultValueHandlingDeserialize
            {
                Derp = "Derp!"
            };
            IntValue1 = int.MaxValue;
            IntValue2 = int.MinValue;
            IntValue3 = int.MaxValue;
        }

        [DefaultValue(1)]
        public int IntValue1 { get; set; }

        public int IntValue2 { get; set; }

        [DefaultValue(null)]
        public int IntValue3 { get; set; }

        public DefaultValueHandlingDeserialize ClassValue { get; set; }
    }

    public class DefaultValueHandlingDeserializePopulate
    {
        public DefaultValueHandlingDeserializePopulate()
        {
            ClassValue = new DefaultValueHandlingDeserialize
            {
                Derp = "Derp!"
            };
            IntValue1 = int.MaxValue;
            IntValue2 = int.MinValue;
        }

        [DefaultValue(1)]
        public int IntValue1 { get; set; }

        public int IntValue2 { get; set; }
        public DefaultValueHandlingDeserialize ClassValue { get; set; }
    }

    public struct DefaultStruct
    {
        public string Default { get; set; }
    }

    public class DefaultValueHandlingPropertyClass
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int IntIgnore { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int IntInclude { get; set; }

        [JsonProperty]
        public int IntDefault { get; set; }
    }

#if !NET20
    [DataContract]
    public class EmitDefaultValueClass
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Guid { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TimeSpan TimeSpan { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTimeOffset DateTimeOffset { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public decimal Decimal { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Integer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double Double { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Boolean { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DefaultStruct Struct { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public StringComparison Enum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? NullableGuid { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TimeSpan? NullableTimeSpan { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? NullableDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTimeOffset? NullableDateTimeOffset { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public decimal? NullableDecimal { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? NullableInteger { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double? NullableDouble { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? NullableBoolean { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DefaultStruct? NullableStruct { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public StringComparison? NullableEnum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Object { get; set; }
    }
#endif
}