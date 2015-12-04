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
using System.Text;
using Newtonsoft.Json.Converters;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class DeserializeCustomCreationConverter : TestFixtureBase
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

        [Test]
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
            // Carpenter
            #endregion

            Assert.AreEqual("Carpenter", employee.JobTitle);
        }
    }
}