using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class QueryJson
  {
    public void Example()
    {
      #region Usage
      string json = @"{
        'channel': {
          'title': 'James Newton-King',
          'link': 'http://james.newtonking.com',
          'description': 'James Newton-King\'s blog.',
          'item': [
            {
              'title': 'Json.NET 1.3 + New license + Now on CodePlex',
              'description': 'Annoucing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
              'link': 'http://james.newtonking.com/projects/json-net.aspx',
              'category': [
                'Json.NET',
                'CodePlex'
              ]
            },
            {
              'title': 'LINQ to JSON beta',
              'description': 'Annoucing LINQ to JSON',
              'link': 'http://james.newtonking.com/projects/json-net.aspx',
              'category': [
                'Json.NET',
                'LINQ'
              ]
            }
          ]
        }
      }";
      
      JObject rss = JObject.Parse(json);
      
      string rssTitle = (string)rss["channel"]["title"];

      Console.WriteLine(rssTitle);
      // James Newton-King
      
      string itemTitle = (string)rss["channel"]["item"][0]["title"];

      Console.WriteLine(itemTitle);
      // Json.NET 1.3 + New license + Now on CodePlex

      JArray categories = (JArray)rss["channel"]["item"][0]["category"];

      Console.WriteLine(categories);
      // [
      //   "Json.NET",
      //   "CodePlex"
      // ]

      IList<string> categoriesText = categories.Select(c => (string)c).ToList();

      Console.WriteLine(string.Join(", ", categoriesText));
      // Json.NET, CodePlex
      #endregion
    }
  }
}
