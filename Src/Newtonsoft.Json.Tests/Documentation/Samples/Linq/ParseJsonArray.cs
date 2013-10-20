using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class ParseJsonArray
    {
        public void Example()
        {
            #region Usage
            string json = @"[
              'Small',
              'Medium',
              'Large'
            ]";

            JArray a = JArray.Parse(json);

            Console.WriteLine(a.ToString());
            // [
            //   "Small",
            //   "Medium",
            //   "Large"
            // ]
            #endregion
        }
    }
}