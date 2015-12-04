#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    [TestFixture]
    public class CreateJsonDeclaratively : TestFixtureBase
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
            return new List<Post>
            {
                new Post
                {
                    Title = "Title!",
                    Categories = new List<string>
                    {
                        "Category1"
                    },
                    Description = "Description!",
                    Link = "Link!"
                }
            };
        }

        [Test]
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

            Assert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Title!"",
        ""description"": ""Description!"",
        ""link"": ""Link!"",
        ""category"": [
          ""Category1""
        ]
      }
    ]
  }
}", rss.ToString());
        }
    }
}