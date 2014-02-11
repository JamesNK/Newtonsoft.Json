using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class SpecialPropertyHandlingTests : TestFixtureBase
    {
        [Test]
        public void DeserializeArraysWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                @"Cannot preserve reference to array or readonly list, or list created from a non-default constructor: System.String[][]. Path '$values', line 3, position 15.",
                () =>
                {
                    JsonConvert.DeserializeObject<string[][]>(json,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.All,
                            SpecialPropertyHandling = SpecialPropertyHandling.ReadAhead
                        });
                });
        }

        [Test]
        public void SerializeRefNull()
        {
            var reference = new Dictionary<string, object>();
            reference.Add("blah", "blah!");
            reference.Add("$ref", null);
            reference.Add("$id", null);

            var child = new Dictionary<string, object>();
            child.Add("_id", 2);
            child.Add("Name", "Isabell");
            child.Add("Father", reference);

            string json = JsonConvert.SerializeObject(child);

            Console.WriteLine(json);

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new JsonSerializerSettings
            {
                SpecialPropertyHandling = SpecialPropertyHandling.ReadAhead
            });

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, ((JObject)result["Father"]).Count);
            Assert.AreEqual("blah!", (string)((JObject)result["Father"])["blah"]);
        }

        [Test]
        public void DeserializeEmployeeReference()
        {
            string json = @"[
  {
    ""Name"": ""Mike Manager"",
    ""$id"": ""1"",
    ""Manager"": null
  },
  {
    ""Name"": ""Joe User"",
    ""$id"": ""2"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]";

            List<EmployeeReference> employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json, new JsonSerializerSettings
            {
                SpecialPropertyHandling = SpecialPropertyHandling.ReadAhead
            });

            Assert.AreEqual(2, employees.Count);
            Assert.AreEqual("Mike Manager", employees[0].Name);
            Assert.AreEqual("Joe User", employees[1].Name);
            Assert.AreEqual(employees[0], employees[1].Manager);
        }

#if !(PORTABLE || PORTABLE40)
        [Test]
        public void DeserializeGenericObjectListWithTypeName()
        {
            string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
            string personRef = typeof(Person).AssemblyQualifiedName;

            string json = @"[
  {
    ""Name"": ""Bob"",
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""Name"": null,
    ""$type"": """ + personRef + @""",
    ""BirthDate"": ""\/Date(978134400000)\/"",
    ""LastModified"": ""\/Date(978134400000)\/""
  },
  ""String!"",
  -2147483648
]";

            List<object> values = (List<object>)JsonConvert.DeserializeObject(json, typeof(List<object>), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                SpecialPropertyHandling = SpecialPropertyHandling.ReadAhead
            });

            Assert.AreEqual(4, values.Count);

            EmployeeReference e = (EmployeeReference)values[0];
            Person p = (Person)values[1];

            Assert.AreEqual("Bob", e.Name);
            Assert.AreEqual("Frank", e.Manager.Name);

            Assert.AreEqual(null, p.Name);
            Assert.AreEqual(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.BirthDate);
            Assert.AreEqual(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.LastModified);

            Assert.AreEqual("String!", values[2]);
            Assert.AreEqual((long)int.MinValue, values[3]);
        }

        [Test]
        public void WriteListTypeNameForProperty()
        {
            string listRef = ReflectionUtils.GetTypeName(typeof(List<int>), FormatterAssemblyStyle.Simple, null);

            TypeNameHandlingTests.TypeNameProperty typeNameProperty = new TypeNameHandlingTests.TypeNameProperty
            {
                Name = "Name!",
                Value = new List<int> { 1, 2, 3, 4, 5 }
            };

            string json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

            Assert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + listRef + @""",
    ""$values"": [
      1,
      2,
      3,
      4,
      5
    ]
  }
}", json);

            TypeNameHandlingTests.TypeNameProperty deserialized = JsonConvert.DeserializeObject<TypeNameHandlingTests.TypeNameProperty>(json, new JsonSerializerSettings
            {
                SpecialPropertyHandling = SpecialPropertyHandling.ReadAhead
            });
            Assert.AreEqual("Name!", deserialized.Name);
            CustomAssert.IsInstanceOfType(typeof(List<int>), deserialized.Value);

            List<int> nested = (List<int>)deserialized.Value;
            Assert.AreEqual(5, nested.Count);
            Assert.AreEqual(1, nested[0]);
            Assert.AreEqual(2, nested[1]);
            Assert.AreEqual(3, nested[2]);
            Assert.AreEqual(4, nested[3]);
            Assert.AreEqual(5, nested[4]);
        }
#endif
    }
}