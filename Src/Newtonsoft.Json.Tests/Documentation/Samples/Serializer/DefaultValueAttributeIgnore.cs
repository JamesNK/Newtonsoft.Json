using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class DefaultValueAttributeIgnore
    {
        #region Types
        public class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            [DefaultValue(" ")]
            public string FullName
            {
                get { return FirstName + " " + LastName; }
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Customer customer = new Customer();

            string jsonIncludeDefaultValues = JsonConvert.SerializeObject(customer, Formatting.Indented);

            Console.WriteLine(jsonIncludeDefaultValues);
            // {
            //   "FirstName": null,
            //   "LastName": null,
            //   "FullName": " "
            // }

            string jsonIgnoreDefaultValues = JsonConvert.SerializeObject(customer, Formatting.Indented, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            Console.WriteLine(jsonIgnoreDefaultValues);
            // {}
            #endregion
        }
    }
}