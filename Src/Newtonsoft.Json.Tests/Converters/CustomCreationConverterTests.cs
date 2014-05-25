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
using Newtonsoft.Json.Converters;
#if !NETFX_CORE
using NUnit.Framework;

#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
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

        internal interface IRange<T>
        {
            T First { get; }
            T Last { get; }
        }

        internal class Range<T> : IRange<T>
        {
            public T First { get; set; }
            public T Last { get; set; }
        }

        internal class NullInterfaceTestClass
        {
            public virtual Guid Id { get; set; }
            public virtual int? Year { get; set; }
            public virtual string Company { get; set; }
            public virtual IRange<decimal> DecimalRange { get; set; }
            public virtual IRange<int> IntRange { get; set; }
            public virtual IRange<decimal> NullDecimalRange { get; set; }
        }

        internal class DecimalRangeConverter : CustomCreationConverter<IRange<decimal>>
        {
            public override IRange<decimal> Create(Type objectType)
            {
                return new Range<decimal>();
            }
        }

        internal class IntRangeConverter : CustomCreationConverter<IRange<int>>
        {
            public override IRange<int> Create(Type objectType)
            {
                return new Range<int>();
            }
        }

        [Test]
        public void DeserializeAndConvertNullValue()
        {
            NullInterfaceTestClass initial = new NullInterfaceTestClass
            {
                Company = "Company!",
                DecimalRange = new Range<decimal> { First = 0, Last = 1 },
                Id = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
                IntRange = new Range<int> { First = int.MinValue, Last = int.MaxValue },
                Year = 2010,
                NullDecimalRange = null
            };

            string json = JsonConvert.SerializeObject(initial, Formatting.Indented);

            Assert.AreEqual(@"{
  ""Id"": ""00000001-0002-0003-0405-060708090a0b"",
  ""Year"": 2010,
  ""Company"": ""Company!"",
  ""DecimalRange"": {
    ""First"": 0.0,
    ""Last"": 1.0
  },
  ""IntRange"": {
    ""First"": -2147483648,
    ""Last"": 2147483647
  },
  ""NullDecimalRange"": null
}", json);

            NullInterfaceTestClass deserialized = JsonConvert.DeserializeObject<NullInterfaceTestClass>(
                json, new IntRangeConverter(), new DecimalRangeConverter());

            Assert.AreEqual("Company!", deserialized.Company);
            Assert.AreEqual(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11), deserialized.Id);
            Assert.AreEqual(0, deserialized.DecimalRange.First);
            Assert.AreEqual(1, deserialized.DecimalRange.Last);
            Assert.AreEqual(int.MinValue, deserialized.IntRange.First);
            Assert.AreEqual(int.MaxValue, deserialized.IntRange.Last);
            Assert.AreEqual(null, deserialized.NullDecimalRange);
            Assert.AreEqual(2010, deserialized.Year);
        }
    }
}