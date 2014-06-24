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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class DemoTests : TestFixtureBase
    {
        public class HtmlColor
        {
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }
        }

        [Test]
        public void JsonConverter()
        {
            HtmlColor red = new HtmlColor
            {
                Red = 255,
                Green = 0,
                Blue = 0
            };

            string json = JsonConvert.SerializeObject(red, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            // {
            //   "Red": 255,
            //   "Green": 0,
            //   "Blue": 0
            // }

            json = JsonConvert.SerializeObject(red, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new HtmlColorConverter() }
            });
            // "#FF0000"

            HtmlColor r2 = JsonConvert.DeserializeObject<HtmlColor>(json, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new HtmlColorConverter() }
            });
            Assert.AreEqual(255, r2.Red);
            Assert.AreEqual(0, r2.Green);
            Assert.AreEqual(0, r2.Blue);

            Console.WriteLine(json);
        }

        public class PersonDemo
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Job { get; set; }
        }

        public class Session
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        [Test]
        public void GenerateSchema()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(Session));

            // {
            //   "type": "object",
            //   "properties": {
            //     "Name": {
            //       "required": true,
            //       "type": [
            //         "string",
            //         "null"
            //       ]
            //     },
            //     "Date": {
            //       "required": true,
            //       "type": "string"
            //     }
            //   }
            // }
        }

        public class HtmlColorConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // create hex string from value
                HtmlColor color = (HtmlColor)value;
                string hexString = color.Red.ToString("X2")
                    + color.Green.ToString("X2")
                    + color.Blue.ToString("X2");

                // write value to json
                writer.WriteValue("#" + hexString);
            }

            //public override object ReadJson(JsonReader reader, Type objectType,
            //    object existingValue, JsonSerializer serializer)
            //{
            //    throw new NotImplementedException();
            //}

            public override object ReadJson(JsonReader reader, Type objectType,
                object existingValue, JsonSerializer serializer)
            {
                // get hex string
                string hexString = (string)reader.Value;
                hexString = hexString.TrimStart('#');

                // build html color from hex
                return new HtmlColor
                {
                    Red = Convert.ToInt32(hexString.Substring(0, 2), 16),
                    Green = Convert.ToInt32(hexString.Substring(2, 2), 16),
                    Blue = Convert.ToInt32(hexString.Substring(4, 2), 16)
                };
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(HtmlColor);
            }
        }
    }
}