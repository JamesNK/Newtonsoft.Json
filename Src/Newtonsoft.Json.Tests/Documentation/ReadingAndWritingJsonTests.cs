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

#if !(NET35 || NET20 || PORTABLE)
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
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json.Tests.Documentation
{
    [TestFixture]
    public class ReadingAndWritingJsonTests : TestFixtureBase
    {
        public void ReadingAndWritingJsonText()
        {
            #region ReadingAndWritingJsonText
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("CPU");
                writer.WriteValue("Intel");
                writer.WritePropertyName("PSU");
                writer.WriteValue("500W");
                writer.WritePropertyName("Drives");
                writer.WriteStartArray();
                writer.WriteValue("DVD read/writer");
                writer.WriteComment("(broken)");
                writer.WriteValue("500 gigabyte hard drive");
                writer.WriteValue("200 gigabype hard drive");
                writer.WriteEnd();
                writer.WriteEndObject();
            }

            // {
            //   "CPU": "Intel",
            //   "PSU": "500W",
            //   "Drives": [
            //     "DVD read/writer"
            //     /*(broken)*/,
            //     "500 gigabyte hard drive",
            //     "200 gigabype hard drive"
            //   ]
            // }
            #endregion
        }

        [Test]
        public void ReadingJsonText()
        {
            #region ReadingJsonText
            string json = @"{
               'CPU': 'Intel',
               'PSU': '500W',
               'Drives': [
                 'DVD read/writer'
                 /*(broken)*/,
                 '500 gigabyte hard drive',
                 '200 gigabype hard drive'
               ]
            }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
                if (reader.Value != null)
                    Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                else
                    Console.WriteLine("Token: {0}", reader.TokenType);
            }

            // Token: StartObject
            // Token: PropertyName, Value: CPU
            // Token: String, Value: Intel
            // Token: PropertyName, Value: PSU
            // Token: String, Value: 500W
            // Token: PropertyName, Value: Drives
            // Token: StartArray
            // Token: String, Value: DVD read/writer
            // Token: Comment, Value: (broken)
            // Token: String, Value: 500 gigabyte hard drive
            // Token: String, Value: 200 gigabype hard drive
            // Token: EndArray
            // Token: EndObject
            #endregion
        }

        public void ReadingAndWritingJsonLinq()
        {
            #region ReadingAndWritingJsonLinq
            JObject o = new JObject(
                new JProperty("Name", "John Smith"),
                new JProperty("BirthDate", new DateTime(1983, 3, 20))
                );

            JsonSerializer serializer = new JsonSerializer();
            Person p = (Person)serializer.Deserialize(new JTokenReader(o), typeof(Person));

            Console.WriteLine(p.Name);
            // John Smith
            #endregion
        }
    }
}

#endif