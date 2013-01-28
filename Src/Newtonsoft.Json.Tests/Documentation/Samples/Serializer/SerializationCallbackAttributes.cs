using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class SerializationCallbackAttributes
  {
    #region Types
    public class SerializationEventTestObject
    {
      // 2222
      // This member is serialized and deserialized with no change.
      public int Member1 { get; set; }

      // The value of this field is set and reset during and 
      // after serialization.
      public string Member2 { get; set; }

      // This field is not serialized. The OnDeserializedAttribute 
      // is used to set the member value after serialization.
      [JsonIgnore]
      public string Member3 { get; set; }

      // This field is set to null, but populated after deserialization.
      public string Member4 { get; set; }

      public SerializationEventTestObject()
      {
        Member1 = 11;
        Member2 = "Hello World!";
        Member3 = "This is a nonserialized value";
        Member4 = null;
      }

      [OnSerializing]
      internal void OnSerializingMethod(StreamingContext context)
      {
        Member2 = "This value went into the data file during serialization.";
      }

      [OnSerialized]
      internal void OnSerializedMethod(StreamingContext context)
      {
        Member2 = "This value was reset after serialization.";
      }

      [OnDeserializing]
      internal void OnDeserializingMethod(StreamingContext context)
      {
        Member3 = "This value was set during deserialization";
      }

      [OnDeserialized]
      internal void OnDeserializedMethod(StreamingContext context)
      {
        Member4 = "This value was set after deserialization.";
      }
    }
    #endregion

    public void Example()
    {
      #region Usage
      SerializationEventTestObject obj = new SerializationEventTestObject();

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // Hello World!
      Console.WriteLine(obj.Member3);
      // This is a nonserialized value
      Console.WriteLine(obj.Member4);
      // null

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      // {
      //   "Member1": 11,
      //   "Member2": "This value went into the data file during serialization.",
      //   "Member4": null
      // }

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // This value was reset after serialization.
      Console.WriteLine(obj.Member3);
      // This is a nonserialized value
      Console.WriteLine(obj.Member4);
      // null

      obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // This value went into the data file during serialization.
      Console.WriteLine(obj.Member3);
      // This value was set during deserialization
      Console.WriteLine(obj.Member4);
      // This value was set after deserialization.
      #endregion
    }
  }
}
