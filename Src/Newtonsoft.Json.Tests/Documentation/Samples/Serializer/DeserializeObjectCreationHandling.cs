using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class DeserializeObjectCreationHandling
    {
        #region Types
        public class UserViewModel
        {
            public string Name { get; set; }
            public IList<string> Offices { get; private set; }

            public UserViewModel()
            {
                Offices = new List<string>
                {
                    "Auckland",
                    "Wellington",
                    "Christchurch"
                };
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            string json = @"{
              'Name': 'James',
              'Offices': [
                'Auckland',
                'Wellington',
                'Christchurch'
              ]
            }";

            UserViewModel model1 = JsonConvert.DeserializeObject<UserViewModel>(json);

            foreach (string office in model1.Offices)
            {
                Console.WriteLine(office);
            }
            // Auckland
            // Wellington
            // Christchurch
            // Auckland
            // Wellington
            // Christchurch

            UserViewModel model2 = JsonConvert.DeserializeObject<UserViewModel>(json, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });

            foreach (string office in model2.Offices)
            {
                Console.WriteLine(office);
            }
            // Auckland
            // Wellington
            // Christchurch
            #endregion
        }
    }
}