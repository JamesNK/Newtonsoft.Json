﻿#region License
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class ShouldSerializeTests : TestFixtureBase
    {
        public class A
        {
        }

        public class B
        {
            public A A { get; set; }

            public virtual bool ShouldSerializeA()
            {
                return false;
            }
        }

        [Test]
        public void VirtualShouldSerializeSimple()
        {
            string json = JsonConvert.SerializeObject(new B());

            Assert.AreEqual("{}", json);
        }

        [Test]
        public void VirtualShouldSerialize()
        {
            var setFoo = new Foo2()
            {
                name = Guid.NewGuid().ToString(),
                myBar = new Bar2()
                {
                    name = Guid.NewGuid().ToString(),
                    myBaz = new Baz1[]
                    {
                        new Baz1()
                        {
                            name = Guid.NewGuid().ToString(),
                            myFrob = new Frob1[]
                            {
                                new Frob1 { name = Guid.NewGuid().ToString() }
                            }
                        },
                        new Baz1()
                        {
                            name = Guid.NewGuid().ToString(),
                            myFrob = new Frob1[]
                            {
                                new Frob1 { name = Guid.NewGuid().ToString() }
                            }
                        },
                        new Baz1()
                        {
                            name = Guid.NewGuid().ToString(),
                            myFrob = new Frob1[]
                            {
                                new Frob1 { name = Guid.NewGuid().ToString() }
                            }
                        },
                    }
                }
            };

            var setFooJson = Serialize(setFoo);
            var deserializedSetFoo = JsonConvert.DeserializeObject<Foo2>(setFooJson);

            Assert.AreEqual(setFoo.name, deserializedSetFoo.name);
            Assert.IsNotNull(deserializedSetFoo.myBar);
            Assert.AreEqual(setFoo.myBar.name, deserializedSetFoo.myBar.name);
            Assert.IsNotNull(deserializedSetFoo.myBar.myBaz);
            Assert.AreEqual(setFoo.myBar.myBaz.Length, deserializedSetFoo.myBar.myBaz.Length);
            Assert.AreEqual(setFoo.myBar.myBaz[0].name, deserializedSetFoo.myBar.myBaz[0].name);
            Assert.IsNotNull(deserializedSetFoo.myBar.myBaz[0].myFrob[0]);
            Assert.AreEqual(setFoo.myBar.myBaz[0].myFrob[0].name, deserializedSetFoo.myBar.myBaz[0].myFrob[0].name);
            Assert.AreEqual(setFoo.myBar.myBaz[1].name, deserializedSetFoo.myBar.myBaz[1].name);
            Assert.IsNotNull(deserializedSetFoo.myBar.myBaz[2].myFrob[0]);
            Assert.AreEqual(setFoo.myBar.myBaz[1].myFrob[0].name, deserializedSetFoo.myBar.myBaz[1].myFrob[0].name);
            Assert.AreEqual(setFoo.myBar.myBaz[2].name, deserializedSetFoo.myBar.myBaz[2].name);
            Assert.IsNotNull(deserializedSetFoo.myBar.myBaz[2].myFrob[0]);
            Assert.AreEqual(setFoo.myBar.myBaz[2].myFrob[0].name, deserializedSetFoo.myBar.myBaz[2].myFrob[0].name);

            Assert.AreEqual(true, setFoo.myBar.ShouldSerializemyBazCalled);
        }

        private string Serialize(Foo2 f)
        {
            //Code copied from JsonConvert.SerializeObject(), with addition of trace writing
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
            var traceWriter = new MemoryTraceWriter();
            jsonSerializer.TraceWriter = traceWriter;

            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.None;
                jsonSerializer.Serialize(jsonWriter, f, typeof(Foo2));
            }

            return sw.ToString();
        }

        [Test]
        public void ShouldSerializeTest()
        {
            ShouldSerializeTestClass c = new ShouldSerializeTestClass();
            c.Name = "James";
            c.Age = 27;

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Age"": 27
}", json);

            c._shouldSerializeName = true;
            json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""James"",
  ""Age"": 27
}", json);

            ShouldSerializeTestClass deserialized = JsonConvert.DeserializeObject<ShouldSerializeTestClass>(json);
            Assert.AreEqual("James", deserialized.Name);
            Assert.AreEqual(27, deserialized.Age);
        }

        [Test]
        public void ShouldSerializeExample()
        {
            Employee joe = new Employee();
            joe.Name = "Joe Employee";
            Employee mike = new Employee();
            mike.Name = "Mike Manager";

            joe.Manager = mike;
            mike.Manager = mike;

            string json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented);
            // [
            //   {
            //     "Name": "Joe Employee",
            //     "Manager": {
            //       "Name": "Mike Manager"
            //     }
            //   },
            //   {
            //     "Name": "Mike Manager"
            //   }
            // ]

            StringAssert.AreEqual(@"[
  {
    ""Name"": ""Joe Employee"",
    ""Manager"": {
      ""Name"": ""Mike Manager""
    }
  },
  {
    ""Name"": ""Mike Manager""
  }
]", json);
        }

        [Test]
        public void SpecifiedTest()
        {
            SpecifiedTestClass c = new SpecifiedTestClass();
            c.Name = "James";
            c.Age = 27;
            c.NameSpecified = false;

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Age"": 27
}", json);

            SpecifiedTestClass deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json);
            Assert.IsNull(deserialized.Name);
            Assert.IsFalse(deserialized.NameSpecified);
            Assert.IsFalse(deserialized.WeightSpecified);
            Assert.IsFalse(deserialized.HeightSpecified);
            Assert.IsFalse(deserialized.FavoriteNumberSpecified);
            Assert.AreEqual(27, deserialized.Age);

            c.NameSpecified = true;
            c.WeightSpecified = true;
            c.HeightSpecified = true;
            c.FavoriteNumber = 23;
            json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""James"",
  ""Age"": 27,
  ""Weight"": 0,
  ""Height"": 0,
  ""FavoriteNumber"": 23
}", json);

            deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json);
            Assert.AreEqual("James", deserialized.Name);
            Assert.IsTrue(deserialized.NameSpecified);
            Assert.IsTrue(deserialized.WeightSpecified);
            Assert.IsTrue(deserialized.HeightSpecified);
            Assert.IsTrue(deserialized.FavoriteNumberSpecified);
            Assert.AreEqual(27, deserialized.Age);
            Assert.AreEqual(23, deserialized.FavoriteNumber);
        }

        //    [Test]
        //    public void XmlSerializerSpecifiedTrueTest()
        //    {
        //      XmlSerializer s = new XmlSerializer(typeof(OptionalOrder));

        //      StringWriter sw = new StringWriter();
        //      s.Serialize(sw, new OptionalOrder() { FirstOrder = "First", FirstOrderSpecified = true });

        //      Console.WriteLine(sw.ToString());

        //      string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
        //<OptionalOrder xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
        //  <FirstOrder>First</FirstOrder>
        //</OptionalOrder>";

        //      OptionalOrder o = (OptionalOrder)s.Deserialize(new StringReader(xml));
        //      Console.WriteLine(o.FirstOrder);
        //      Console.WriteLine(o.FirstOrderSpecified);
        //    }

        //    [Test]
        //    public void XmlSerializerSpecifiedFalseTest()
        //    {
        //      XmlSerializer s = new XmlSerializer(typeof(OptionalOrder));

        //      StringWriter sw = new StringWriter();
        //      s.Serialize(sw, new OptionalOrder() { FirstOrder = "First", FirstOrderSpecified = false });

        //      Console.WriteLine(sw.ToString());

        //      //      string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
        //      //<OptionalOrder xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
        //      //  <FirstOrder>First</FirstOrder>
        //      //</OptionalOrder>";

        //      //      OptionalOrder o = (OptionalOrder)s.Deserialize(new StringReader(xml));
        //      //      Console.WriteLine(o.FirstOrder);
        //      //      Console.WriteLine(o.FirstOrderSpecified);
        //    }

        public class OptionalOrder
        {
            // This field shouldn't be serialized 
            // if it is uninitialized.
            public string FirstOrder;
            // Use the XmlIgnoreAttribute to ignore the 
            // special field named "FirstOrderSpecified".
            [System.Xml.Serialization.XmlIgnoreAttribute]
            public bool FirstOrderSpecified;
        }

        public class FamilyDetails
        {
            public string Name { get; set; }
            public int NumberOfChildren { get; set; }

            [JsonIgnore]
            public bool NumberOfChildrenSpecified { get; set; }
        }

        [Test]
        public void SpecifiedExample()
        {
            FamilyDetails joe = new FamilyDetails();
            joe.Name = "Joe Family Details";
            joe.NumberOfChildren = 4;
            joe.NumberOfChildrenSpecified = true;

            FamilyDetails martha = new FamilyDetails();
            martha.Name = "Martha Family Details";
            martha.NumberOfChildren = 3;
            martha.NumberOfChildrenSpecified = false;

            string json = JsonConvert.SerializeObject(new[] { joe, martha }, Formatting.Indented);
            //[
            //  {
            //    "Name": "Joe Family Details",
            //    "NumberOfChildren": 4
            //  },
            //  {
            //    "Name": "Martha Family Details"
            //  }
            //]

            StringAssert.AreEqual(@"[
  {
    ""Name"": ""Joe Family Details"",
    ""NumberOfChildren"": 4
  },
  {
    ""Name"": ""Martha Family Details""
  }
]", json);

            string mikeString = "{\"Name\": \"Mike Person\"}";
            FamilyDetails mike = JsonConvert.DeserializeObject<FamilyDetails>(mikeString);

            Assert.AreEqual(false, mike.NumberOfChildrenSpecified);

            string mikeFullDisclosureString = "{\"Name\": \"Mike Person\", \"NumberOfChildren\": \"0\"}";
            mike = JsonConvert.DeserializeObject<FamilyDetails>(mikeFullDisclosureString);

            Assert.AreEqual(true, mike.NumberOfChildrenSpecified);
        }

        [Test]
        public void ShouldSerializeInheritedClassTest()
        {
            NewEmployee joe = new NewEmployee();
            joe.Name = "Joe Employee";
            joe.Age = 100;

            Employee mike = new Employee();
            mike.Name = "Mike Manager";
            mike.Manager = mike;

            joe.Manager = mike;

            //StringWriter sw = new StringWriter();

            //XmlSerializer x = new XmlSerializer(typeof(NewEmployee));
            //x.Serialize(sw, joe);

            //Console.WriteLine(sw);

            //JavaScriptSerializer s = new JavaScriptSerializer();
            //Console.WriteLine(s.Serialize(new {html = @"<script>hi</script>; & ! ^ * ( ) ! @ # $ % ^ ' "" - , . / ; : [ { } ] ; ' - _ = + ? ` ~ \ |"}));

            string json = JsonConvert.SerializeObject(joe, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Age"": 100,
  ""Name"": ""Joe Employee"",
  ""Manager"": {
    ""Name"": ""Mike Manager""
  }
}", json);
        }

        [Test]
        public void ShouldDeserialize_True()
        {
            string json = @"{'HasName':true,'Name':'Name!'}";

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            ShouldDeserializeTestClass c = JsonConvert.DeserializeObject<ShouldDeserializeTestClass>(json, new JsonSerializerSettings
            {
                ContractResolver = ShouldDeserializeContractResolver.Instance,
                TraceWriter = traceWriter
            });

            Assert.AreEqual(null, c.ExtensionData);
            Assert.AreEqual(true, c.HasName);
            Assert.AreEqual("Name!", c.Name);

            Assert.IsTrue(traceWriter.GetTraceMessages().Any(m => m.EndsWith("Verbose ShouldDeserialize result for property 'Name' on Newtonsoft.Json.Tests.Serialization.ShouldDeserializeTestClass: True. Path 'Name'.")));
        }

        [Test]
        public void ShouldDeserialize_False()
        {
            string json = @"{'HasName':false,'Name':'Name!'}";

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            ShouldDeserializeTestClass c = JsonConvert.DeserializeObject<ShouldDeserializeTestClass>(json, new JsonSerializerSettings
            {
                ContractResolver = ShouldDeserializeContractResolver.Instance,
                TraceWriter = traceWriter
            });

            Assert.AreEqual(1, c.ExtensionData.Count);
            Assert.AreEqual("Name!", (string)c.ExtensionData["Name"]);
            Assert.AreEqual(false, c.HasName);
            Assert.AreEqual(null, c.Name);

            Assert.IsTrue(traceWriter.GetTraceMessages().Any(m => m.EndsWith("Verbose ShouldDeserialize result for property 'Name' on Newtonsoft.Json.Tests.Serialization.ShouldDeserializeTestClass: False. Path 'Name'.")));
        }

        public class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }

            public bool ShouldSerializeManager()
            {
                return (Manager != this);
            }
        }

        public class NewEmployee : Employee
        {
            public int Age { get; set; }

            public bool ShouldSerializeName()
            {
                return false;
            }
        }
    }

    public class ShouldSerializeTestClass
    {
        internal bool _shouldSerializeName;

        public string Name { get; set; }
        public int Age { get; set; }

        public void ShouldSerializeAge()
        {
            // dummy. should never be used because it doesn't return bool
        }

        public bool ShouldSerializeName()
        {
            return _shouldSerializeName;
        }
    }

    public class SpecifiedTestClass
    {
        private bool _nameSpecified;

        public string Name { get; set; }
        public int Age { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public int FavoriteNumber { get; set; }

        // dummy. should never be used because it isn't of type bool
        [JsonIgnore]
        public long AgeSpecified { get; set; }

        [JsonIgnore]
        public bool NameSpecified
        {
            get { return _nameSpecified; }
            set { _nameSpecified = value; }
        }

        [JsonIgnore]
        public bool WeightSpecified;

        [JsonIgnore]
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool HeightSpecified;

        [JsonIgnore]
        public bool FavoriteNumberSpecified
        {
            // get only example
            get { return FavoriteNumber != 0; }
        }
    }

    public class Foo2
    {
        private Bar2 myBarField;

        public Bar2 myBar
        {
            get { return myBarField; }
            set { myBarField = value; }
        }

        private string nameField;

        public string name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        public virtual bool ShouldSerializemyBar()
        {
            return (myBar != null);
        }

        public virtual bool ShouldSerializename()
        {
            return (name != null);
        }
    }

    public class Bar2
    {
        [JsonIgnore]
        public bool ShouldSerializemyBazCalled { get; set; }

        private Baz1[] myBazField;

        public Baz1[] myBaz
        {
            get { return myBazField; }
            set { myBazField = value; }
        }

        private string nameField;

        public string name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        public virtual bool ShouldSerializemyBaz()
        {
            ShouldSerializemyBazCalled = true;
            return (myBaz != null);
        }

        public virtual bool ShouldSerializename()
        {
            return (name != null);
        }
    }

    public class Baz1
    {
        private Frob1[] myFrobField;

        public Frob1[] myFrob
        {
            get { return myFrobField; }
            set { myFrobField = value; }
        }

        private string nameField;

        public string name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        public virtual bool ShouldSerializename()
        {
            return (name != null);
        }

        public virtual bool ShouldSerializemyFrob()
        {
            return (myFrob != null);
        }
    }

    public class Frob1
    {
        private string nameField;

        public string name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        public virtual bool ShouldSerializename()
        {
            return (name != null);
        }
    }

    public class ShouldDeserializeContractResolver : DefaultContractResolver
    {
        public static new readonly ShouldDeserializeContractResolver Instance = new ShouldDeserializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            MethodInfo shouldDeserializeMethodInfo = member.DeclaringType.GetMethod("ShouldDeserialize" + member.Name);

            if (shouldDeserializeMethodInfo != null)
            {
                property.ShouldDeserialize = o => { return (bool)shouldDeserializeMethodInfo.Invoke(o, null); };
            }

            return property;
        }
    }

    public class ShouldDeserializeTestClass
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }

        public bool HasName { get; set; }
        public string Name { get; set; }

        public bool ShouldDeserializeName()
        {
            return HasName;
        }
    }
}