using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class JValueCast
    {
        public void Example()
        {
            #region Usage
            JValue v1 = new JValue("1");
            int i = (int)v1;

            Console.WriteLine(i);
            // 1

            JValue v2 = new JValue(true);
            bool b = (bool)v2;

            Console.WriteLine(b);
            // true

            JValue v3 = new JValue("19.95");
            decimal d = (decimal)v3;

            Console.WriteLine(d);
            // 19.95

            JValue v4 = new JValue(new DateTime(2013, 1, 21));
            string s = (string)v4;

            Console.WriteLine(s);
            // 01/21/2013 00:00:00

            JValue v5 = new JValue("http://www.bing.com");
            Uri u = (Uri)v5;

            Console.WriteLine(u);
            // http://www.bing.com/

            JValue v6 = new JValue((object)null);
            u = (Uri)v6;

            Console.WriteLine((u != null) ? u.ToString() : "{null}");
            // {null}

            DateTime? dt = (DateTime?)v6;

            Console.WriteLine((dt != null) ? dt.ToString() : "{null}");
            // {null}
            #endregion
        }
    }
}