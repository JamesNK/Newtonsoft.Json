using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Converters
{
  public class CustomCreationConverterTests : TestFixtureBase
  {
    public interface IPerson
    {
      string FirstName { get; set; }
      string LastName { get; set; }
      DateTime BirthDate { get; set; }
    }

    public class Employee : IPerson
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTime BirthDate { get; set; }

      public string Department { get; set; }
      public string JobTitle { get; set; }
    }

    public class PersonConverter : CustomCreationConverter<IPerson>
    {
      public override IPerson Create(Type objectType)
      {
        return new Employee();
      }
    }

    public void DeserializeObject()
    {
      string json = JsonConvert.SerializeObject(new List<Employee>
        {
          new Employee
            {
              BirthDate = new DateTime(1977, 12, 30, 1, 1, 1, DateTimeKind.Utc),
              FirstName = "Maurice",
              LastName = "Moss",
              Department = "IT",
              JobTitle = "Support"
            },
          new Employee
            {
              BirthDate = new DateTime(1978, 3, 15, 1, 1, 1, DateTimeKind.Utc),
              FirstName = "Jen",
              LastName = "Barber",
              Department = "IT",
              JobTitle = "Manager"
            }
        }, Formatting.Indented);

      //[
      //  {
      //    "FirstName": "Maurice",
      //    "LastName": "Moss",
      //    "BirthDate": "\/Date(252291661000)\/",
      //    "Department": "IT",
      //    "JobTitle": "Support"
      //  },
      //  {
      //    "FirstName": "Jen",
      //    "LastName": "Barber",
      //    "BirthDate": "\/Date(258771661000)\/",
      //    "Department": "IT",
      //    "JobTitle": "Manager"
      //  }
      //]

      List<IPerson> people = JsonConvert.DeserializeObject<List<IPerson>>(json, new PersonConverter());

      IPerson person = people[0];

      Console.WriteLine(person.GetType());
      // Newtonsoft.Json.Tests.Employee

      Console.WriteLine(person.FirstName);
      // Maurice

      Employee employee = (Employee)person;

      Console.WriteLine(employee.JobTitle);
      // Support
    }

    public class MyClass
    {
      public string Value { get; set; }

      [JsonConverter(typeof(MyThingConverter))]
      public IThing Thing { get; set; }
    }

    public interface IThing
    {
      int Number { get; }
    }

    public class MyThing : IThing
    {
      public int Number { get; set; }
    }

    public class MyThingConverter : CustomCreationConverter<IThing>
    {
      public override IThing Create(Type objectType)
      {
        return new MyThing();
      }
    }

    [Test]
    public void AssertDoesDeserialize()
    {
      const string json = @"{
""Value"": ""A value"",
""Thing"": {
""Number"": 123
}
}
";
      MyClass myClass = JsonConvert.DeserializeObject<MyClass>(json);
      Assert.IsNotNull(myClass);
      Assert.AreEqual("A value", myClass.Value);
      Assert.IsNotNull(myClass.Thing);
      Assert.AreEqual(123, myClass.Thing.Number);
    }

    [Test]
    public void AssertShouldSerializeTest()
    {
      MyClass myClass = new MyClass
      {
        Value = "Foo",
        Thing = new MyThing { Number = 456, }
      };
      string json = JsonConvert.SerializeObject(myClass); // <-- Exception here

      const string expected = @"{""Value"":""Foo"",""Thing"":{""Number"":456}}";
      Assert.AreEqual(expected, json);
    }
  }
}