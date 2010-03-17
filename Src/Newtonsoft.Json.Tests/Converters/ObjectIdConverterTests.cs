using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Converters
{
  public class ObjectIdConverterTests : TestFixtureBase
  {
    public class ObjectIdTestClass
    {
      [JsonProperty("_id")]
      public BsonObjectId Id { get; set; }
      [JsonProperty("test")]
      public string Test { get; set; }
    }

    [Test]
    public void Serialize()
    {
      ObjectIdTestClass c = new ObjectIdTestClass
                              {
                                Id = new BsonObjectId(MiscellaneousUtils.HexToBytes("4ABBED9D1D8B0F0218000001")),
                                Test = "1234£56"
                              };

      MemoryStream ms = new MemoryStream();
      JsonSerializer serializer = new JsonSerializer();

      // serialize product to BSON
      BsonWriter writer = new BsonWriter(ms);
      serializer.Serialize(writer, c);

      byte[] expected = MiscellaneousUtils.HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

      Assert.AreEqual(expected, ms.ToArray());
    }

    [Test]
    public void Deserialize()
    {
      byte[] bson = MiscellaneousUtils.HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

      JsonSerializer serializer = new JsonSerializer();

      BsonReader reader = new BsonReader(new MemoryStream(bson));
      ObjectIdTestClass c = serializer.Deserialize<ObjectIdTestClass>(reader);

      Assert.AreEqual(c.Id.Value, MiscellaneousUtils.HexToBytes("4ABBED9D1D8B0F0218000001"));
      Assert.AreEqual(c.Test, "1234£56");
    }
  }
}
