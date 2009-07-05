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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class TypeNameHandlingTests : TestFixtureBase
  {
    [Test]
    public void WriteTypeNameForObjects()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

      EmployeeReference employee = new EmployeeReference();

      string json = JsonConvert.SerializeObject(employee, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });

      Assert.AreEqual(@"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": null,
  ""Manager"": null
}", json);
    }

    [Test]
    public void DeserializeTypeName()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

      string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      object employee = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });

      Assert.IsInstanceOfType(typeof(EmployeeReference), employee);
      Assert.AreEqual("Name!", ((EmployeeReference)employee).Name);
    }

    [Test]
    public void SerializeGenericObjectListWithTypeName()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
      string personRef = typeof(Person).AssemblyQualifiedName;

      List<object> values = new List<object>
        {
          new EmployeeReference
            {
              Name = "Bob",
              Manager = new EmployeeReference {Name = "Frank"}
            },
          new Person
            {
              Department = "Department",
              BirthDate = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc),
              LastModified = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc)
            },
          "String!",
          int.MinValue
        };

      string json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });

      Assert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
    ""BirthDate"": ""\/Date(978134400000)\/"",
    ""LastModified"": ""\/Date(978134400000)\/""
  },
  ""String!"",
  -2147483648
]", json);
    }

    [Test]
    public void DeserializeGenericObjectListWithTypeName()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
      string personRef = typeof(Person).AssemblyQualifiedName;

      string json = @"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
    ""BirthDate"": ""\/Date(978134400000)\/"",
    ""LastModified"": ""\/Date(978134400000)\/""
  },
  ""String!"",
  -2147483648
]";

      List<object> values = (List<object>)JsonConvert.DeserializeObject(json, typeof(List<object>), new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
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
      Assert.AreEqual(int.MinValue, values[3]);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException))]
    public void DeserializeWithBadTypeName()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

      string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      JsonConvert.DeserializeObject(json, typeof(Person), new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });
    }

    [Test]
    public void DeserializeTypeNameWithNoTypeNameHandling()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

      string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      JObject o = (JObject)JsonConvert.DeserializeObject(json);

      Assert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Manager"": null
}", o.ToString());
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Type specified in JSON 'Newtonsoft.Json.Tests.TestObjects.Employee' was not resolved.")]
    public void DeserializeTypeNameOnly()
    {
      string json = @"{
  ""$id"": ""1"",
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Employee"",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });
    }
  }
}
