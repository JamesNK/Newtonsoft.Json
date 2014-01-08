using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class ParseJsonObject
    {
        public void Example()
        {
            #region Usage
            string json = @"{
              CPU: 'Intel',
              Drives: [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }";

            JObject o = JObject.Parse(json);

            Console.WriteLine(o.ToString());
            // {
            //   "CPU": "Intel",
            //   "Drives": [
            //     "DVD read/writer",
            //     "500 gigabyte hard drive"
            //   ]
            // }
            #endregion
        }
    }
}