using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class DeserializeDictionary
    {
        public void Example()
        {
            #region Usage
            string json = @"{
              'href': '/account/login.aspx',
              'target': '_blank'
            }";

            Dictionary<string, string> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Console.WriteLine(htmlAttributes["href"]);
            // /account/login.aspx

            Console.WriteLine(htmlAttributes["target"]);
            // _blank
            #endregion
        }
    }
}