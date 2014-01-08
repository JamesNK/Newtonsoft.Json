using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateJsonDeclaratively
    {
        #region Types
        public class Post
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public IList<string> Categories { get; set; }
        }
        #endregion

        private List<Post> GetPosts()
        {
            return null;
        }

        public void Example()
        {
            #region Usage
            List<Post> posts = GetPosts();

            JObject rss =
                new JObject(
                    new JProperty("channel",
                        new JObject(
                            new JProperty("title", "James Newton-King"),
                            new JProperty("link", "http://james.newtonking.com"),
                            new JProperty("description", "James Newton-King's blog."),
                            new JProperty("item",
                                new JArray(
                                    from p in posts
                                    orderby p.Title
                                    select new JObject(
                                        new JProperty("title", p.Title),
                                        new JProperty("description", p.Description),
                                        new JProperty("link", p.Link),
                                        new JProperty("category",
                                            new JArray(
                                                from c in p.Categories
                                                select new JValue(c)))))))));

            Console.WriteLine(rss.ToString());

            // {
            //   "channel": {
            //     "title": "James Newton-King",
            //     "link": "http://james.newtonking.com",
            //     "description": "James Newton-King's blog.",
            //     "item": [
            //       {
            //         "title": "Json.NET 1.3 + New license + Now on CodePlex",
            //         "description": "Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
            //         "link": "http://james.newtonking.com/projects/json-net.aspx",
            //         "category": [
            //           "Json.NET",
            //           "CodePlex"
            //         ]
            //       },
            //       {
            //         "title": "LINQ to JSON beta",
            //         "description": "Annoucing LINQ to JSON",
            //         "link": "http://james.newtonking.com/projects/json-net.aspx",
            //         "category": [
            //           "Json.NET",
            //           "LINQ"
            //         ]
            //       }
            //     ]
            //   }
            // }
            #endregion
        }
    }
}