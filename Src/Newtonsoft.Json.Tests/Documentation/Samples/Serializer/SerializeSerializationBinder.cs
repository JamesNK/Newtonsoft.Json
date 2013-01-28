using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class SerializeSerializationBinder
  {
    #region Types
    public class KnownTypesBinder : SerializationBinder
    {
      public IList<Type> KnownTypes { get; set; }
 
      public override Type BindToType(string assemblyName, string typeName)
      {
        return KnownTypes.SingleOrDefault(t => t.Name == typeName);
      }

      public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
      {
        assemblyName = null;
        typeName = serializedType.Name;
      }
    }

    public class Car
    {
      public string Maker { get; set; }
      public string Model { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      KnownTypesBinder knownTypesBinder = new KnownTypesBinder
        {
          KnownTypes = new List<Type> {typeof (Car)}
        };

      Car car = new Car
        {
          Maker = "Ford",
          Model = "Explorer"
        };

      string json = JsonConvert.SerializeObject(car, Formatting.Indented, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Objects,
          Binder = knownTypesBinder
        });

      Console.WriteLine(json);
      // {
      //   "$type": "Car",
      //   "Maker": "Ford",
      //   "Model": "Explorer"
      // }

      object newValue = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Objects,
          Binder = knownTypesBinder
        });

      Console.WriteLine(newValue.GetType().Name);
      // Car
      #endregion
    }
  }
}