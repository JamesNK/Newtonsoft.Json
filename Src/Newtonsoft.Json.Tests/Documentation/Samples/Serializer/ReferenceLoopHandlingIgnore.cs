using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class ReferenceLoopHandlingIgnore
    {
        #region Types
        public class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            string json = JsonConvert.SerializeObject(joe, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            Console.WriteLine(json);
            // {
            //   "Name": "Joe User",
            //   "Manager": {
            //     "Name": "Mike Manager"
            //   }
            // }
            #endregion
        }
    }
}