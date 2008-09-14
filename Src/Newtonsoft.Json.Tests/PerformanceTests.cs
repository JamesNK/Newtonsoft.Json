#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

using Newtonsoft.Json;
using System.IO;

using System.Web.Script.Serialization;

using NUnit.Framework;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Newtonsoft.Json.Tests
{
  /// <summary>
  /// Summary description for JsonSerializerTest
  /// </summary>
  public class PerformanceTests : TestFixtureBase
  {
    private int Iterations = 100;

    public enum SerializeMethod
    {
      JsonNet,
      JavaScriptSerializer,
      DataContractJsonSerializer
    }

    private DateTime BaseDate = DateTime.Parse("01/01/2000");

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

      writer.Formatting = Formatting.None;

      writer.QuoteChar = '"';
      json.Serialize(writer, value);

      string output = sw.ToString();
      writer.Close();

      return output;
    }

    public object DeserializeJsonNet<T>(string json)
    {
      Type type = typeof(T);

      JsonSerializer serializer = new JsonSerializer();

      serializer.NullValueHandling = NullValueHandling.Ignore;

      serializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
      serializer.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
      serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

      return serializer.Deserialize(new StringReader(json), type);
    }

    public string SerializeWebExtensions(object value)
    {
      JavaScriptSerializer ser = new JavaScriptSerializer();

      return ser.Serialize(value);
    }

    public object DeserializeWebExtensions<T>(string json)
    {
      JavaScriptSerializer ser = new JavaScriptSerializer();

      return ser.Deserialize<T>(json);
    }

    public string SerializeDataContract(object value)
    {
      DataContractJsonSerializer dataContractSerializer
        = new DataContractJsonSerializer(value.GetType());
      
      MemoryStream ms = new MemoryStream();
      dataContractSerializer.WriteObject(ms, value);

      ms.Seek(0, SeekOrigin.Begin);

      using (StreamReader sr = new StreamReader(ms))
      {
        return sr.ReadToEnd();
      }
    }

    public object DeserializeDataContract<T>(string json)
    {
      DataContractJsonSerializer dataContractSerializer
        = new DataContractJsonSerializer(typeof(T));

      MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

      return dataContractSerializer.ReadObject(ms);
    }

    public void BenchmarkSerializeMethod(SerializeMethod method, object value)
    {
      Stopwatch timed = new Stopwatch();
      timed.Start();

      string json = null;
      for (int x = 0; x < Iterations; x++)
      {
        switch (method)
        {
          case SerializeMethod.JsonNet:
            json = JavaScriptConvert.SerializeObject(value);
            break;
          case SerializeMethod.JavaScriptSerializer:
            json = SerializeWebExtensions(value);
            break;
          case SerializeMethod.DataContractJsonSerializer:
            json = SerializeDataContract(value);
            break;
        }
      }

      timed.Stop();

      Console.WriteLine("Serialize method: {0}", method);
      Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);
      Console.WriteLine(json);
      Console.WriteLine();
    }

    public void BenchmarkDeserializeMethod<T>(SerializeMethod method, string json)
    {
      Stopwatch timed = new Stopwatch();
      timed.Start();

      object value = null;
      for (int x = 0; x < Iterations; x++)
      {
        switch (method)
        {
          case SerializeMethod.JsonNet:
            value = DeserializeJsonNet<T>(json);
            break;
          case SerializeMethod.JavaScriptSerializer:
            value = DeserializeWebExtensions<T>(json);
            break;
          case SerializeMethod.DataContractJsonSerializer:
            value = DeserializeDataContract<T>(json);
            break;
        }
      }

      timed.Stop();

      Console.WriteLine("Serialize method: {0}", method);
      Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);
      Console.WriteLine(value);
      Console.WriteLine();
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

      BenchmarkSerializeMethod(SerializeMethod.JsonNet, test);
      BenchmarkSerializeMethod(SerializeMethod.JavaScriptSerializer, test);
      BenchmarkSerializeMethod(SerializeMethod.DataContractJsonSerializer, test);
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

      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JsonNet, json);
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JavaScriptSerializer, json);
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.DataContractJsonSerializer, json);
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
#endif