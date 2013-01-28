using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class QueryJsonLinq
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

      var postTitles =
        from p in rss["channel"]["item"]
        select (string)p["title"];

      foreach (var item in postTitles)
      {
        Console.WriteLine(item);
      }
      //LINQ to JSON beta
      //Json.NET 1.3 + New license + Now on CodePlex

      var categories =
        from c in rss["channel"]["item"].Children()["category"].Values<string>()
        group c by c into g
        orderby g.Count() descending
        select new { Category = g.Key, Count = g.Count() };

      foreach (var c in categories)
      {
        Console.WriteLine(c.Category + " - Count: " + c.Count);
      }
      //Json.NET - Count: 2
      //LINQ - Count: 1
      //CodePlex - Count: 1
      #endregion
    }
  }
}
