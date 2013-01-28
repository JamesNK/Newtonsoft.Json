using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation
{
  public class JsonNetVsWindowsDataJsonTests
  {
    public void CreatingJson()
    {
#if NETFX_CORE
      #region CreatingJSON
      // Windows.Data.Json
      // -----------------
      JsonObject jsonObject = new JsonObject
        {
          {"CPU", JsonValue.CreateStringValue("Intel")},
          {
            "Drives", new JsonArray
                        {
                          JsonValue.CreateStringValue("DVD read/writer"),
                          JsonValue.CreateStringValue("500 gigabyte hard drive")
                        }
          }
        };
      string json1 = jsonObject.Stringify();

      // LINQ to JSON
      // ------------
      JObject jObject = new JObject
        {
          {"CPU", "Intel"},
          {
            "Drives", new JArray
                        {
                          "DVD read/writer",
                          "500 gigabyte hard drive"
                        }
          }
        };
      string json2 = jObject.ToString();
      #endregion
#endif
    }

    public void QueryingJson()
    {
#if NETFX_CORE
      #region QueryingJSON
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
              'category': [
                'Json.NET',
                'CodePlex'
              ]
            }
          ]
        }
      }";
 
      // Windows.Data.Json
      // -----------------
      JsonObject jsonObject = JsonObject.Parse(json);
      string itemTitle1 = jsonObject["channel"].GetObject()["item"].GetArray()[0].GetObject()["title"].GetString();
 
      // LINQ to JSON
      // ------------
      JObject jObject = JObject.Parse(json);
      string itemTitle2 = (string)jObject["channel"]["item"][0]["title"];
      #endregion
#endif
    }

    public void Converting()
    {
#if NETFX_CORE
      #region Converting
      JsonObject jsonObject = new JsonObject
        {
          {"CPU", JsonValue.CreateStringValue("Intel")},
          {"Drives", new JsonArray {
              JsonValue.CreateStringValue("DVD read/writer"),
              JsonValue.CreateStringValue("500 gigabyte hard drive")
            }
          }
        };
 
      // convert Windows.Data.Json to LINQ to JSON
      JObject o = JObject.FromObject(jsonObject);
 
      // convert LINQ to JSON to Windows.Data.Json
      JArray a = (JArray)o["Drives"];
      JsonArray jsonArray = a.ToObject<JsonArray>();
      #endregion
#endif
    }
  }
}