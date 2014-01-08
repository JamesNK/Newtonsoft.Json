using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class SerializeWithLinq
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
            IList<BlogPost> blogPosts = new List<BlogPost>
            {
                new BlogPost
                {
                    Title = "Json.NET is awesome!",
                    AuthorName = "James Newton-King",
                    AuthorTwitter = "JamesNK",
                    PostedDate = new DateTime(2013, 1, 23, 19, 30, 0),
                    Body = @"<h3>Title!</h3>
                       <p>Content!</p>"
                }
            };

            JArray blogPostsArray = new JArray(
                blogPosts.Select(p => new JObject
                {
                    { "Title", p.Title },
                    {
                        "Author", new JObject
                        {
                            { "Name", p.AuthorName },
                            { "Twitter", p.AuthorTwitter }
                        }
                    },
                    { "Date", p.PostedDate },
                    { "BodyHtml", HttpUtility.HtmlEncode(p.Body) },
                })
                );

            Console.WriteLine(blogPostsArray.ToString());
            // [
            //   {
            //     "Title": "Json.NET is awesome!",
            //     "Author": {
            //       "Name": "James Newton-King",
            //       "Twitter": "JamesNK"
            //     },
            //     "Date": "2013-01-23T19:30:00",
            //     "BodyHtml": "&lt;h3&gt;Title!&lt;/h3&gt;\r\n&lt;p&gt;Content!&lt;/p&gt;"
            //   }
            // ]
            #endregion
        }
    }
}