using System;
using System.Collections.Generic;
using System.IO;
#if !NET20
using System.Linq;
#endif
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
#if !(SILVERLIGHT || NETFX_CORE || NET20)
using System.Runtime.Serialization.Json;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
  public class WebApiIntegrationTests : TestFixtureBase
  {
    [Test]
    public void SerializeSerializableType()
    {
      SerializableType serializableType = new SerializableType("protected")
        {
          publicField = "public",
          protectedInternalField = "protected internal",
          internalField = "internal",
          PublicProperty = "private",
          nonSerializedField = "Error"
        };

#if !(SILVERLIGHT || NETFX_CORE || NET20)
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(SerializableType));
      dataContractJsonSerializer.WriteObject(ms, serializableType);

      string dtJson = Encoding.UTF8.GetString(ms.ToArray());
      string dtExpected = @"{""internalField"":""internal"",""privateField"":""private"",""protectedField"":""protected"",""protectedInternalField"":""protected internal"",""publicField"":""public""}";

      Assert.AreEqual(dtExpected, dtJson);
#endif

      string expected = "{\"publicField\":\"public\",\"internalField\":\"internal\",\"protectedInternalField\":\"protected internal\",\"protectedField\":\"protected\",\"privateField\":\"private\"}";
      string json = JsonConvert.SerializeObject(serializableType);

      Assert.AreEqual(expected, json);
    }

#if !(SILVERLIGHT || NETFX_CORE)
    [Test]
    public void SerializeInheritedType()
    {
      InheritedType serializableType = new InheritedType("protected")
      {
        publicField = "public",
        protectedInternalField = "protected internal",
        internalField = "internal",
        PublicProperty = "private",
        nonSerializedField = "Error",
        inheritedTypeField = "inherited"
      };

      string json = JsonConvert.SerializeObject(serializableType);

      Assert.AreEqual(@"{""inheritedTypeField"":""inherited"",""publicField"":""public"",""PublicProperty"":""private""}", json);
    }
#endif
  }

  public class InheritedType : SerializableType
  {
    public string inheritedTypeField;

    public InheritedType(string protectedFieldValue) : base(protectedFieldValue)
    {
    }
  }

#if !(SILVERLIGHT || NETFX_CORE)
  [Serializable]
#else
  [JsonObject(MemberSerialization.Fields)]
#endif
  public class SerializableType : IEquatable<SerializableType>
  {
    public SerializableType(string protectedFieldValue)
    {
      this.protectedField = protectedFieldValue;
    }

    public string publicField;
    internal string internalField;
    protected internal string protectedInternalField;
    protected string protectedField;
    private string privateField;

    public string PublicProperty
    {
      get
      {
        return privateField;
      }
      set
      {
        this.privateField = value;
      }
    }

#if !(SILVERLIGHT || NETFX_CORE)
    [NonSerialized]
#else
    [JsonIgnore]
#endif
    public string nonSerializedField;

    public bool Equals(SerializableType other)
    {
      return this.publicField == other.publicField &&
          this.internalField == other.internalField &&
          this.protectedInternalField == other.protectedInternalField &&
          this.protectedField == other.protectedField &&
          this.privateField == other.privateField;
    }
  }
}