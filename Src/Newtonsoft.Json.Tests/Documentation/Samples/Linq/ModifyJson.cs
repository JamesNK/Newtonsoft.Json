using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class ModifyJson
    {
        public void Example()
        {
            #region Usage
            string json = @"{
              'channel': {
                'title': 'Star Wars',
                'link': 'http://www.starwars.com',
                'description': 'Star Wars blog.',
                'obsolete': 'Obsolete value',
                'item': []
              }
            }";

            JObject rss = JObject.Parse(json);

            JObject channel = (JObject)rss["channel"];

            channel["title"] = ((string)channel["title"]).ToUpper();
            channel["description"] = ((string)channel["description"]).ToUpper();

            channel.Property("obsolete").Remove();

            channel.Property("description").AddAfterSelf(new JProperty("new", "New value"));

            JArray item = (JArray)channel["item"];
            item.Add("Item 1");
            item.Add("Item 2");

            Console.WriteLine(rss.ToString());
            // {
            //   "channel": {
            //     "title": "STAR WARS",
            //     "link": "http://www.starwars.com",
            //     "description": "STAR WARS BLOG.",
            //     "new": "New value",
            //     "item": [
            //       "Item 1",
            //       "Item 2"
            //     ]
            //   }
            // }
            #endregion
        }
    }
}