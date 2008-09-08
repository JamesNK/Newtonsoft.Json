using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Diagnostics;

using Newtonsoft.Json;
using System.IO;
using System.Web.Script.Serialization;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  /// <summary>
  /// Summary description for JsonSerializerTest
  /// </summary>
  public class PerformanceTests : TestFixtureBase
  {
    private DateTime BaseDate = DateTime.Parse("01/01/2000");

    //JSONSerializer ser = null;

    public string SerializeJsonNet(object value)
    {
      Type type = value.GetType();

      Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

      json.NullValueHandling = NullValueHandling.Ignore;

      json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
      json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
      json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;


      StringWriter sw = new StringWriter();
      Newtonsoft.Json.JsonTextWriter writer = new JsonTextWriter(sw);

      if (true)
        writer.Formatting = Formatting.Indented;
      else
        writer.Formatting = Formatting.None;

      writer.QuoteChar = '"';
      json.Serialize(writer, value);

      string output = sw.ToString();
      writer.Close();

      return output;
    }

    public string SerializeWebExtensions(object value)
    {
      JavaScriptSerializer ser = new JavaScriptSerializer();

      List<JavaScriptConverter> converters = new List<JavaScriptConverter>();

      return ser.Serialize(value);
    }


    [Test]
    public void Serialize()
    {
      TestClass test = new TestClass();

      test.Address1.Address = "fff Street";
      test.Address1.Entered = DateTime.Now.AddDays(20);

      test.BigNumber = 34123123123.121M;
      test.Now = DateTime.Now.AddHours(1);
      test.strings = new List<string>() { "Rick", "Markus egger ][, (2nd)", "Kevin McNeish" };

      cAddress address = new cAddress();
      address.Entered = DateTime.Now.AddDays(-1);
      address.Address = "array address";

      test.Addresses.Add(address);

      address = new cAddress();
      address.Entered = DateTime.Now.AddDays(-2);
      address.Address = "array 2 address";
      test.Addresses.Add(address);

      Stopwatch timed = new Stopwatch();
      timed.Start();

      string json = null;
      for (int x = 0; x < 10000; x++)
      {
        json = this.SerializeJsonNet(test);
        //json = this.SerializeWebExtensions(test);
      }
      timed.Stop();

      Console.WriteLine("{0}", timed.ElapsedMilliseconds + " ms");
      Console.WriteLine("{0}", json);
    }

    [Test]
    public void Deserialize()
    {
      string json = @"{
  ""strings"": [
    ""Rick"",
    ""Markus egger ][, (2nd)"",
    ""Kevin McNeish""
  ],
  ""dictionary"": {
    ""Val asd1"": 1,
    ""Val2"": 3,
    ""Val3"": 4
  },
  ""Name"": ""Rick"",
  ""Now"": ""\/Date(1220867547892+1200)\/"",
  ""BigNumber"": 34123123123.121,
  ""Address1"": {
    ""Address"": ""fff Street"",
    ""Phone"": ""(503) 814-6335"",
    ""Entered"": ""\/Date(1222588347892+1300)\/""
  },
  ""Addresses"": [
    {
      ""Address"": ""array address"",
      ""Phone"": ""(503) 814-6335"",
      ""Entered"": ""\/Date(1220777547892+1200)\/""
    },
    {
      ""Address"": ""array 2 address"",
      ""Phone"": ""(503) 814-6335"",
      ""Entered"": ""\/Date(1220691147893+1200)\/""
    }
  ]
}";

      for (int x = 0; x < 10000; x++)
      {
        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

        serializer.NullValueHandling = NullValueHandling.Ignore;

        serializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
        serializer.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
        serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        TestClass test = (TestClass)serializer.Deserialize(new StringReader(json), typeof(TestClass));
      }

    }
  }


  public class TestClass
  {

    public string Name
    {
      get { return _Name; }
      set { _Name = value; }
    }
    private string _Name = "Rick";


    public DateTime Now
    {
      get { return _Now; }
      set { _Now = value; }
    }
    private DateTime _Now = DateTime.Now;


    public decimal BigNumber
    {
      get { return _BigNumber; }
      set { _BigNumber = value; }
    }
    private decimal _BigNumber = 1212121.22M;


    public cAddress Address1
    {
      get { return _Address1; }
      set { _Address1 = value; }
    }
    private cAddress _Address1 = new cAddress();




    public List<cAddress> Addresses
    {
      get { return _Addresses; }
      set { _Addresses = value; }
    }
    private List<cAddress> _Addresses = new List<cAddress>();


    public List<string> strings = new List<string>() { "Rick", "Markus", "Kevin" };


    public Dictionary<string, int> dictionary = new Dictionary<string, int> { { "Val asd1", 1 }, { "Val2", 3 }, { "Val3", 4 } };
  }

  public class cAddress
  {

    public string Address
    {
      get { return _Address; }
      set { _Address = value; }
    }
    private string _Address = "32 Kaiea";


    public string Phone
    {
      get { return _Phone; }
      set { _Phone = value; }
    }
    private string _Phone = "(503) 814-6335";


    public DateTime Entered
    {
      get { return _Entered; }
      set { _Entered = value; }
    }
    private DateTime _Entered = DateTime.Parse("01/01/2007", CultureInfo.CurrentCulture.DateTimeFormat);

  }
}