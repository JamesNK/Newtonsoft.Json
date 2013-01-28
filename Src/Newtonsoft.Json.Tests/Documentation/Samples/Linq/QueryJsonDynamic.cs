using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class QueryJsonDynamic
  {
    public void Example()
    {
      #region Usage
      string json = @"[
        {
          'Title': 'Json.NET is awesome!',
          'Author': {
            'Name': 'James Newton-King',
            'Twitter': '@JamesNK',
            'Picture': '/jamesnk.png'
          },
          'Date': '2013-01-23T19:30:00',
          'BodyHtml': '&lt;h3&gt;Title!&lt;/h3&gt;\r\n&lt;p&gt;Content!&lt;/p&gt;'
        }
      ]";

      dynamic blogPosts = JArray.Parse(json);

      dynamic blogPost = blogPosts[0];

      string title = blogPost.Title;

      Console.WriteLine(title);
      // Json.NET is awesome!

      string author = blogPost.Author.Name;

      Console.WriteLine(author);
      // James Newton-King

      DateTime postDate = blogPost.Date;

      Console.WriteLine(postDate);
      // 23/01/2013 7:30:00 p.m.
      #endregion
    }
  }
}