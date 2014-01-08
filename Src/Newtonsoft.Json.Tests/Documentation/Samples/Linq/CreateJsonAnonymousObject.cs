using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateJsonAnonymousObject
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

        public void Example()
        {
            #region Usage
            List<Post> posts = new List<Post>
            {
                new Post
                {
                    Title = "Episode VII",
                    Description = "Episode VII production",
                    Categories = new List<string>
                    {
                        "episode-vii",
                        "movie"
                    },
                    Link = "episode-vii-production.aspx"
                }
            };

            JObject o = JObject.FromObject(new
            {
                channel = new
                {
                    title = "Star Wars",
                    link = "http://www.starwars.com",
                    description = "Star Wars blog.",
                    item =
                        from p in posts
                        orderby p.Title
                        select new
                        {
                            title = p.Title,
                            description = p.Description,
                            link = p.Link,
                            category = p.Categories
                        }
                }
            });

            Console.WriteLine(o.ToString());
            // {
            //   "channel": {
            //     "title": "Star Wars",
            //     "link": "http://www.starwars.com",
            //     "description": "Star Wars blog.",
            //     "item": [
            //       {
            //         "title": "Episode VII",
            //         "description": "Episode VII production",
            //         "link": "episode-vii-production.aspx",
            //         "category": [
            //           "episode-vii",
            //           "movie"
            //         ]
            //       }
            //     ]
            //   }
            // }
            #endregion
        }
    }
}