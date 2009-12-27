#if !SILVERLIGHT && !PocketPC && !NET20
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

using Newtonsoft.Json;
using System.IO;

using System.Web.Script.Serialization;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Tests
{
  public class PerformanceTests : TestFixtureBase
  {
    private const int Iterations = 100;
    //private const int Iterations = 5000;

    private const string BsonHex =
      @"B4-01-00-00-04-73-74-72-69-6E-67-73-00-44-00-00-00-02-30-00-05-00-00-00-52-69-63-6B-00-02-31-00-17-00-00-00-4D-61-72-6B-75-73-20-65-67-67-65-72-20-5D-5B-2C-20-28-32-6E-64-29-00-02-32-00-0E-00-00-00-4B-65-76-69-6E-20-4D-63-4E-65-69-73-68-00-00-03-64-69-63-74-69-6F-6E-61-72-79-00-27-00-00-00-10-56-61-6C-20-61-73-64-31-00-01-00-00-00-10-56-61-6C-32-00-03-00-00-00-10-56-61-6C-33-00-04-00-00-00-00-02-4E-61-6D-65-00-05-00-00-00-52-69-63-6B-00-09-4E-6F-77-00-A0-80-DB-70-25-01-00-00-01-42-69-67-4E-75-6D-62-65-72-00-E7-7B-CC-26-96-C7-1F-42-03-41-64-64-72-65-73-73-31-00-48-00-00-00-02-41-64-64-72-65-73-73-00-0B-00-00-00-66-66-66-20-53-74-72-65-65-74-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-20-C2-A3-D7-25-01-00-00-00-04-41-64-64-72-65-73-73-65-73-00-A3-00-00-00-03-30-00-4B-00-00-00-02-41-64-64-72-65-73-73-00-0E-00-00-00-61-72-72-61-79-20-61-64-64-72-65-73-73-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-20-36-7E-6B-25-01-00-00-00-03-31-00-4D-00-00-00-02-41-64-64-72-65-73-73-00-10-00-00-00-61-72-72-61-79-20-32-20-61-64-64-72-65-73-73-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-20-DA-57-66-25-01-00-00-00-00-00";

    private const string JsonText =
      @"{
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

    public enum SerializeMethod
    {
      JsonNet,
      JsonNetBinary,
      JavaScriptSerializer,
      DataContractJsonSerializer
    }

    private DateTime BaseDate = DateTime.Parse("01/01/2000");

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
      BenchmarkSerializeMethod(SerializeMethod.JsonNetBinary, test);
      BenchmarkSerializeMethod(SerializeMethod.JavaScriptSerializer, test);
      BenchmarkSerializeMethod(SerializeMethod.DataContractJsonSerializer, test);
    }

    [Test]
    public void Deserialize()
    {
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JsonNet, JsonText);
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JsonNetBinary, MiscellaneousUtils.HexToBytes(BsonHex));
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JavaScriptSerializer, JsonText);
      BenchmarkDeserializeMethod<TestClass>(SerializeMethod.DataContractJsonSerializer, JsonText);
    }

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

    public object DeserializeJsonNetBinary<T>(byte[] bson)
    {
      Type type = typeof(T);

      JsonSerializer serializer = new JsonSerializer();

      serializer.NullValueHandling = NullValueHandling.Ignore;

      serializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
      serializer.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
      serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

      return serializer.Deserialize(new BsonReader(new MemoryStream(bson)), type);
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
            json = JsonConvert.SerializeObject(value);
            break;
          case SerializeMethod.JsonNetBinary:
            JsonSerializer serializer = new JsonSerializer();
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            serializer.Serialize(writer, value);
            json = "{Bytes " + ms.Length + "}";
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

    public void BenchmarkDeserializeMethod<T>(SerializeMethod method, object json)
    {
      Stopwatch timed = new Stopwatch();
      timed.Start();

      object value = null;
      for (int x = 0; x < Iterations; x++)
      {
        switch (method)
        {
          case SerializeMethod.JsonNet:
            value = DeserializeJsonNet<T>((string)json);
            break;
          case SerializeMethod.JsonNetBinary:
            value = DeserializeJsonNetBinary<T>((byte[])json);
            break;
          case SerializeMethod.JavaScriptSerializer:
            value = DeserializeWebExtensions<T>((string)json);
            break;
          case SerializeMethod.DataContractJsonSerializer:
            value = DeserializeDataContract<T>((string)json);
            break;
        }
      }

      timed.Stop();

      Console.WriteLine("Deserialize method: {0}", method);
      Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);
      Console.WriteLine(value);
      Console.WriteLine();
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