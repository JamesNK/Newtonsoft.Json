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

#if !DNXCORE50 || NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
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
    public class SerializeWithLinq : TestFixtureBase
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

        [Test]
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
                    Body = @"<h3>Title!</h3><p>Content!</p>"
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
            //     "BodyHtml": "&lt;h3&gt;Title!&lt;/h3&gt;&lt;p&gt;Content!&lt;/p&gt;"
            //   }
            // ]
            #endregion

            StringAssert.AreEqual(@"[
  {
    ""Title"": ""Json.NET is awesome!"",
    ""Author"": {
      ""Name"": ""James Newton-King"",
      ""Twitter"": ""JamesNK""
    },
    ""Date"": ""2013-01-23T19:30:00"",
    ""BodyHtml"": ""&lt;h3&gt;Title!&lt;/h3&gt;&lt;p&gt;Content!&lt;/p&gt;""
  }
]", blogPostsArray.ToString());
        }
    }
}

#endif