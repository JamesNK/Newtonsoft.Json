using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class SerializeContractResolver
    {
        #region Types
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string FullName
            {
                get { return FirstName + " " + LastName; }
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Person person = new Person
            {
                FirstName = "Sarah",
                LastName = "Security"
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            Console.WriteLine(json);
            // {
            //   "firstName": "Sarah",
            //   "lastName": "Security",
            //   "fullName": "Sarah Security"
            // }
            #endregion
        }
    }
}