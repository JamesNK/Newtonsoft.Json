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
using System.IO;
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;

#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class NullValueHandlingTests : TestFixtureBase
    {
#if !NET20
        [Test]
        public void DeserializeNullIntoDateTime()
        {
            DateTimeTestClass c = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{DateTimeField:null}", new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.AreEqual(c.DateTimeField, default(DateTime));
        }

        [Test]
        public void DeserializeEmptyStringIntoDateTimeWithEmptyStringDefaultValue()
        {
            DateTimeTestClass c = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{DateTimeField:""""}", new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.AreEqual(c.DateTimeField, default(DateTime));
        }
#endif

        [Test]
        public void NullValueHandlingSerialization()
        {
            Store s1 = new Store();

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

            StringWriter sw = new StringWriter();
            jsonSerializer.Serialize(sw, s1);

            //JsonConvert.ConvertDateTimeToJavaScriptTicks(s1.Establised.DateTime)

            Assert.AreEqual(@"{""Color"":4,""Establised"":""2010-01-22T01:01:01Z"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":""2000-02-02T23:01:30Z"",""Price"":0.0},{""Name"":""Alien"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0}]}", sw.GetStringBuilder().ToString());

            Store s2 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader("{}")), typeof(Store));
            Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s2.Escape);

            Store s3 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Escape"":null}")), typeof(Store));
            Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s3.Escape);

            Store s4 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Color"":2,""Establised"":""\/Date(1264071600000+1300)\/"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":""\/Date(949485690000+1300)\/"",""Price"":0},{""Name"":""Alien"",""ExpiryDate"":""\/Date(946638000000)\/"",""Price"":0.0}]}")), typeof(Store));
            Assert.AreEqual(s1.Establised, s3.Establised);
        }

        [Test]
        public void NullValueHandlingBlogPost()
        {
            Movie movie = new Movie();
            movie.Name = "Bad Boys III";
            movie.Description = "It's no Bad Boys";

            string included = JsonConvert.SerializeObject(movie,
                Formatting.Indented,
                new JsonSerializerSettings { });

            // {
            //   "Name": "Bad Boys III",
            //   "Description": "It's no Bad Boys",
            //   "Classification": null,
            //   "Studio": null,
            //   "ReleaseDate": null,
            //   "ReleaseCountries": null
            // }

            string ignored = JsonConvert.SerializeObject(movie,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // {
            //   "Name": "Bad Boys III",
            //   "Description": "It's no Bad Boys"
            // }

            Assert.AreEqual(@"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys"",
  ""Classification"": null,
  ""Studio"": null,
  ""ReleaseDate"": null,
  ""ReleaseCountries"": null
}", included);

            Assert.AreEqual(@"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys""
}", ignored);
        }
    }
}