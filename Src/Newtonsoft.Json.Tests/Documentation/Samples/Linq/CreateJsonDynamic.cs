using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateJsonDynamic
    {
        public void Example()
        {
            #region Usage
            dynamic product = new JObject();
            product.ProductName = "Elbow Grease";
            product.Enabled = true;
            product.Price = 4.90m;
            product.StockCount = 9000;
            product.StockValue = 44100;
            product.Tags = new JArray("Real", "OnSale");

            Console.WriteLine(product.ToString());
            // {
            //   "ProductName": "Elbow Grease",
            //   "Enabled": true,
            //   "Price": 4.90,
            //   "StockCount": 9000,
            //   "StockValue": 44100,
            //   "Tags": [
            //     "Real",
            //     "OnSale"
            //   ]
            // }
            #endregion
        }
    }
}