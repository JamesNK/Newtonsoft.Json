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

#if !(NET35 || NET20 || PORTABLE || ASPNETCORE50)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using File = System.IO.File;

namespace Newtonsoft.Json.Tests.Documentation
{
    public static class File
    {
        public static StreamReader OpenText(string path)
        {
            return null;
        }

        public static StreamWriter CreateText(string path)
        {
            return null;
        }

        public static void WriteAllText(string path, string contents)
        {
            Console.WriteLine(contents);
        }

        public static string ReadAllText(string path)
        {
            return null;
        }
    }

    public class LinqToJsonTests
    {
        public void LinqToJsonBasic()
        {
            #region LinqToJsonBasic
            JObject o = JObject.Parse(@"{
              'CPU': 'Intel',
              'Drives': [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }");

            string cpu = (string)o["CPU"];
            // Intel

            string firstDrive = (string)o["Drives"][0];
            // DVD read/writer

            IList<string> allDrives = o["Drives"].Select(t => (string)t).ToList();
            // DVD read/writer
            // 500 gigabyte hard drive
            #endregion
        }

        public void LinqToJsonCreateNormal()
        {
            #region LinqToJsonCreateNormal
            JArray array = new JArray();
            JValue text = new JValue("Manual text");
            JValue date = new JValue(new DateTime(2000, 5, 23));

            array.Add(text);
            array.Add(date);

            string json = array.ToString();
            // [
            //   "Manual text",
            //   "2000-05-23T00:00:00"
            // ]
            #endregion
        }

        public class Post
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public IList<string> Categories { get; set; }
        }

        private List<Post> GetPosts()
        {
            return null;
        }

        public void LinqToJsonCreateDeclaratively()
        {
            #region LinqToJsonCreateDeclaratively
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

            //{
            //  "channel": {
            //    "title": "James Newton-King",
            //    "link": "http://james.newtonking.com",
            //    "description": "James Newton-King's blog.",
            //    "item": [
            //      {
            //        "title": "Json.NET 1.3 + New license + Now on CodePlex",
            //        "description": "Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
            //        "link": "http://james.newtonking.com/projects/json-net.aspx",
            //        "category": [
            //          "Json.NET",
            //          "CodePlex"
            //        ]
            //      },
            //      {
            //        "title": "LINQ to JSON beta",
            //        "description": "Annoucing LINQ to JSON",
            //        "link": "http://james.newtonking.com/projects/json-net.aspx",
            //        "category": [
            //          "Json.NET",
            //          "LINQ"
            //        ]
            //      }
            //    ]
            //  }
            //}
            #endregion
        }

        public void LinqToJsonCreateFromObject()
        {
            List<Post> posts = null;

            #region LinqToJsonCreateFromObject
            JObject o = JObject.FromObject(new
            {
                channel = new
                {
                    title = "James Newton-King",
                    link = "http://james.newtonking.com",
                    description = "James Newton-King's blog.",
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
            #endregion
        }

        public void LinqToJsonCreateParse()
        {
            #region LinqToJsonCreateParse
            string json = @"{
              CPU: 'Intel',
              Drives: [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }";

            JObject o = JObject.Parse(json);
            #endregion
        }

        public void LinqToJsonCreateParseArray()
        {
            #region LinqToJsonCreateParseArray
            string json = @"[
              'Small',
              'Medium',
              'Large'
            ]";

            JArray a = JArray.Parse(json);
            #endregion
        }

        public void LinqToJsonReadObject()
        {
            #region LinqToJsonReadObject
            using (StreamReader reader = File.OpenText(@"c:\person.json"))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                // do stuff
            }
            #endregion
        }

        public void LinqToJsonSimpleQuerying()
        {
            #region LinqToJsonSimpleQuerying
            string json = @"{
              'channel': {
                'title': 'James Newton-King',
                'link': 'http://james.newtonking.com',
                'description': 'James Newton-King's blog.',
                'item': [
                  {
                    'title': 'Json.NET 1.3 + New license + Now on CodePlex',
                    'description': 'Annoucing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'CodePlex'
                    ]
                  },
                  {
                    'title': 'LINQ to JSON beta',
                    'description': 'Annoucing LINQ to JSON',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'LINQ'
                    ]
                  }
                ]
              }
            }";

            JObject rss = JObject.Parse(json);

            string rssTitle = (string)rss["channel"]["title"];
            // James Newton-King

            string itemTitle = (string)rss["channel"]["item"][0]["title"];
            // Json.NET 1.3 + New license + Now on CodePlex

            JArray categories = (JArray)rss["channel"]["item"][0]["categories"];
            // ["Json.NET", "CodePlex"]

            IList<string> categoriesText = categories.Select(c => (string)c).ToList();
            // Json.NET
            // CodePlex
            #endregion
        }

        public void LinqToJsonQuerying()
        {
            JObject rss = new JObject();

            #region LinqToJsonQuerying
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
                group c by c
                into g
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

        #region LinqToJsonDeserializeObject
        public class Shortie
        {
            public string Original { get; set; }
            public string Shortened { get; set; }
            public string Short { get; set; }
            public ShortieException Error { get; set; }
        }

        public class ShortieException
        {
            public int Code { get; set; }
            public string ErrorMessage { get; set; }
        }
        #endregion

        public void LinqToJsonDeserializeExample()
        {
            #region LinqToJsonDeserializeExample
            string jsonText = @"{
              'short': {
                'original': 'http://www.foo.com/',
                'short': 'krehqk',
                'error': {
                  'code':0,
                  'msg':'No action taken'
                }
            }";

            JObject json = JObject.Parse(jsonText);

            Shortie shortie = new Shortie
            {
                Original = (string)json["short"]["original"],
                Short = (string)json["short"]["short"],
                Error = new ShortieException
                {
                    Code = (int)json["short"]["error"]["code"],
                    ErrorMessage = (string)json["short"]["error"]["msg"]
                }
            };

            Console.WriteLine(shortie.Original);
            // http://www.foo.com/

            Console.WriteLine(shortie.Error.ErrorMessage);
            // No action taken
            #endregion
        }

        public void SelectTokenSimple()
        {
            JObject o = new JObject();

            #region SelectTokenSimple
            string name = (string)o.SelectToken("Manufacturers[0].Name");
            #endregion
        }

        public void SelectTokenComplex()
        {
            #region SelectTokenComplex
            JObject o = JObject.Parse(@"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }");

            string name = (string)o.SelectToken("Manufacturers[0].Name");
            // Acme Co

            decimal productPrice = (decimal)o.SelectToken("Manufacturers[0].Products[0].Price");
            // 50

            string productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
            // Elbow Grease
            #endregion
        }

        public void SelectTokenLinq()
        {
            JObject o = new JObject();

            #region SelectTokenLinq
            IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();
            // Lambton Quay
            // Willis Street

            IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name")).ToList();
            // null
            // Headlight Fluid

            decimal totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));
            // 149.95
            #endregion
        }
    }
}

#endif