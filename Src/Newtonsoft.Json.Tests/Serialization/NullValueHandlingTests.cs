using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class NullValueHandlingTests : TestFixtureBase
  {
    [Test]
    public void NullValueHandlingSerialization()
    {
      Store s1 = new Store();

      JsonSerializer jsonSerializer = new JsonSerializer();
      jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

      StringWriter sw = new StringWriter();
      jsonSerializer.Serialize(sw, s1);

      //JsonConvert.ConvertDateTimeToJavaScriptTicks(s1.Establised.DateTime)

      Assert.AreEqual(@"{""Color"":4,""Establised"":""\/Date(1264122061000+0000)\/"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":""\/Date(949532490000)\/"",""Price"":0.0},{""Name"":""Alien"",""ExpiryDate"":""\/Date(946684800000)\/"",""Price"":0.0}]}", sw.GetStringBuilder().ToString());

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