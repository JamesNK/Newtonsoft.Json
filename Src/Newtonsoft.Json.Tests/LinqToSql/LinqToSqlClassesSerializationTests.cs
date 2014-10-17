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

#if !(ASPNETCORE50 || NETFX_CORE || PORTABLE40 || PORTABLE40)
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.LinqToSql;
using NUnit.Framework;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;
using System.Data.Linq.Mapping;

namespace Newtonsoft.Json.Tests.LinqToSql
{
    public class LinqToSqlClassesSerializationTests : TestFixtureBase
    {
        [Test]
        public void Serialize()
        {
            Role role = new Role();
            role.Name = "Role1";
            role.RoleId = new Guid("67EA92B7-4BD3-4718-BD75-3C7EDF800B34");

            Person person = new Person();
            person.FirstName = "FirstName!";
            person.LastName = "LastName!";
            person.PersonId = new Guid("7AA027AA-C995-4986-908D-999D8063599F");
            person.PersonRoles.Add(new PersonRole
            {
                PersonRoleId = new Guid("B012DD41-71DF-4839-B8D5-D1333FB886BC"),
                Role = role
            });

            person.Department = new Department
            {
                DepartmentId = new Guid("08F68BF9-929B-4434-BC47-C9489D22112B"),
                Name = "Name!"
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            StringAssert.AreEqual(@"{
  ""first_name"": ""FirstName!"",
  ""LastName"": ""LastName!"",
  ""PersonId"": ""7aa027aa-c995-4986-908d-999d8063599f"",
  ""DepartmentId"": ""08f68bf9-929b-4434-bc47-c9489d22112b"",
  ""PersonRoles"": [
    {
      ""PersonId"": ""7aa027aa-c995-4986-908d-999d8063599f"",
      ""RoleId"": ""67ea92b7-4bd3-4718-bd75-3c7edf800b34"",
      ""PersonRoleId"": ""b012dd41-71df-4839-b8d5-d1333fb886bc"",
      ""Role"": {
        ""Name"": ""Role1"",
        ""RoleId"": ""t5LqZ9NLGEe9dTx+34ALNA==""
      }
    }
  ],
  ""Department"": {
    ""DepartmentId"": ""08f68bf9-929b-4434-bc47-c9489d22112b"",
    ""Name"": ""!emaN""
  }
}", json);
        }

        [Test]
        public void Deserialize()
        {
            string json = @"{
  ""first_name"": ""FirstName!"",
  ""LastName"": ""LastName!"",
  ""PersonId"": ""7aa027aa-c995-4986-908d-999d8063599f"",
  ""PersonRoles"": [
    {
      ""PersonId"": ""7aa027aa-c995-4986-908d-999d8063599f"",
      ""RoleId"": ""67ea92b7-4bd3-4718-bd75-3c7edf800b34"",
      ""PersonRoleId"": ""b012dd41-71df-4839-b8d5-d1333fb886bc"",
      ""Role"": {
        ""Name"": ""Role1"",
        ""RoleId"": ""t5LqZ9NLGEe9dTx+34ALNA==""
      }
    }
  ],
  ""Department"": {
    ""DepartmentId"": ""08f68bf9-929b-4434-bc47-c9489d22112b"",
    ""Name"": ""!emaN""
  }
}";

            Person person = JsonConvert.DeserializeObject<Person>(json);
            Assert.IsNotNull(person);

            Assert.AreEqual(new Guid("7AA027AA-C995-4986-908D-999D8063599F"), person.PersonId);
            Assert.AreEqual("FirstName!", person.FirstName);
            Assert.AreEqual("LastName!", person.LastName);
            Assert.AreEqual(1, person.PersonRoles.Count);
            Assert.AreEqual(person.PersonId, person.PersonRoles[0].PersonId);
            Assert.AreEqual(new Guid("67EA92B7-4BD3-4718-BD75-3C7EDF800B34"), person.PersonRoles[0].RoleId);
            Assert.IsNotNull(person.PersonRoles[0].Role);
            Assert.AreEqual(1, person.PersonRoles[0].Role.PersonRoles.Count);

            Assert.AreEqual("Name!", person.Department.Name);

            TableAttribute tableAttribute = JsonTypeReflector.GetAttribute<TableAttribute>(typeof(Person));
            Assert.AreEqual("", tableAttribute.Name);

            ColumnAttribute columnAttribute = JsonTypeReflector.GetAttribute<ColumnAttribute>(typeof(Person).GetProperty("FirstName"));
            Assert.AreEqual("_FirstName", columnAttribute.Storage);
        }
    }
}
#endif