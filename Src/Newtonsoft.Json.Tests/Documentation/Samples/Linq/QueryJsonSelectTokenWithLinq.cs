using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class QueryJsonSelectTokenWithLinq
    {
        public void Example()
        {
            #region Usage
            JObject o = JObject.Parse(@"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }");

            IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();

            Console.WriteLine(string.Join(", ", storeNames));
            // Lambton Quay, Willis Street

            IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name"))
                .Where(n => n != null).ToList();

            Console.WriteLine(string.Join(", ", firstProductNames));
            // Headlight Fluid

            decimal totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));

            Console.WriteLine(totalPrice);
            // 149.95
            #endregion
        }
    }
}