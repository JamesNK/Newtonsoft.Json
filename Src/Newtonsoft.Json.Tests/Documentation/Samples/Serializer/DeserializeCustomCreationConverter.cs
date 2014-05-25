using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class DeserializeCustomCreationConverter
    {
        #region Types
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime BirthDate { get; set; }
        }

        public class Employee : Person
        {
            public string Department { get; set; }
            public string JobTitle { get; set; }
        }

        public class PersonConverter : CustomCreationConverter<Person>
        {
            public override Person Create(Type objectType)
            {
                return new Employee();
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            string json = @"{
              'Department': 'Furniture',
              'JobTitle': 'Carpenter',
              'FirstName': 'John',
              'LastName': 'Joinery',
              'BirthDate': '1983-02-02T00:00:00'
            }";

            Person person = JsonConvert.DeserializeObject<Person>(json, new PersonConverter());

            Console.WriteLine(person.GetType().Name);
            // Employee

            Employee employee = (Employee)person;

            Console.WriteLine(employee.JobTitle);
            // Capenter
            #endregion
        }
    }
}