using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class DeserializeWithLinq
    {
        #region Types
        public class BlogPost
        {
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string AuthorTwitter { get; set; }
            public string Body { get; set; }
            public DateTime PostedDate { get; set; }
        }
        #endregion

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

            JArray blogPostArray = JArray.Parse(json);

            IList<BlogPost> blogPosts = blogPostArray.Select(p => new BlogPost
            {
                Title = (string)p["Title"],
                AuthorName = (string)p["Author"]["Name"],
                AuthorTwitter = (string)p["Author"]["Twitter"],
                PostedDate = (DateTime)p["Date"],
                Body = HttpUtility.HtmlDecode((string)p["BodyHtml"])
            }).ToList();

            Console.WriteLine(blogPosts[0].Body);
            // <h3>Title!</h3>
            // <p>Content!</p>
            #endregion
        }
    }
}