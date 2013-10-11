using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json {
  
  public class ConstructorHandlingFallbackEventArgs : EventArgs {
    private JsonObjectContract _objectContract;

    public ConstructorHandlingFallbackEventArgs(JsonObjectContract objectContract) {
      _objectContract = objectContract;
    }

    public JsonObjectContract ObjectContract { get { return _objectContract; } }

    public object Object { get; set; }
    public bool Handled { get; set; }
  }

}
