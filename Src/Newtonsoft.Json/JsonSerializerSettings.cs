using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  public class JsonSerializerSettings
  {
    internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;
    internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Error;
    internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;
    internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;
    internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;

    public ReferenceLoopHandling ReferenceLoopHandling { get; set; }
    public MissingMemberHandling MissingMemberHandling { get; set; }
    public ObjectCreationHandling ObjectCreationHandling { get; set; }
    public NullValueHandling NullValueHandling { get; set; }
    public DefaultValueHandling DefaultValueHandling { get; set; }
    public IList<JsonConverter> Converters { get; set; }

    public JsonSerializerSettings()
    {
      ReferenceLoopHandling = DefaultReferenceLoopHandling;
      MissingMemberHandling = DefaultMissingMemberHandling;
      ObjectCreationHandling = DefaultObjectCreationHandling;
      NullValueHandling = DefaultNullValueHandling;
      DefaultValueHandling = DefaultDefaultValueHandling;
      Converters = null;
    }
  }
}