using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies the settings on a <see cref="JsonSerializer"/> object.
  /// </summary>
  public class JsonSerializerSettings
  {
    internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;
    internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;
    internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;
    internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;
    internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;
    internal const PreserveReferencesHandling DefaultPreserveReferencesHandling = PreserveReferencesHandling.None;
    internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;

    /// <summary>
    /// Gets or sets how reference loops (e.g. a class referencing itself) is handled.
    /// </summary>
    /// <value>Reference loop handling.</value>
    public ReferenceLoopHandling ReferenceLoopHandling { get; set; }
    /// <summary>
    /// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
    /// </summary>
    /// <value>Missing member handling.</value>
    public MissingMemberHandling MissingMemberHandling { get; set; }
    /// <summary>
    /// Gets or sets how objects are created during deserialization.
    /// </summary>
    /// <value>The object creation handling.</value>
    public ObjectCreationHandling ObjectCreationHandling { get; set; }
    /// <summary>
    /// Gets or sets how null values are handled during serialization and deserialization.
    /// </summary>
    /// <value>Null value handling.</value>
    public NullValueHandling NullValueHandling { get; set; }
    /// <summary>
    /// Gets or sets how null default are handled during serialization and deserialization.
    /// </summary>
    /// <value>The default value handling.</value>
    public DefaultValueHandling DefaultValueHandling { get; set; }
    /// <summary>
    /// Gets or sets a collection <see cref="JsonConverter"/> that will be used during serialization.
    /// </summary>
    /// <value>The converters.</value>
    public IList<JsonConverter> Converters { get; set; }
    /// <summary>
    /// Gets or sets how object references are preserved by the serializer.
    /// </summary>
    /// <value>The preserve references handling.</value>
    public PreserveReferencesHandling PreserveReferencesHandling { get; set; }
    /// <summary>
    /// Gets or sets how type name writing and reading is handled by the serializer.
    /// </summary>
    /// <value>The type name handling.</value>
    public TypeNameHandling TypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets the contract resolver used by the serializer when
    /// serializing .NET objects to JSON and vice versa.
    /// </summary>
    /// <value>The contract resolver.</value>
    public IContractResolver ContractResolver { get; set; }
    /// <summary>
    /// Gets or sets the <see cref="IReferenceResolver"/> used by the serializer when resolving references.
    /// </summary>
    /// <value>The reference resolver.</value>
    public IReferenceResolver ReferenceResolver { get; set; }
    /// <summary>
    /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
    /// </summary>
    /// <value>The binder.</value>
    public SerializationBinder Binder { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializerSettings"/> class.
    /// </summary>
    public JsonSerializerSettings()
    {
      ReferenceLoopHandling = DefaultReferenceLoopHandling;
      MissingMemberHandling = DefaultMissingMemberHandling;
      ObjectCreationHandling = DefaultObjectCreationHandling;
      NullValueHandling = DefaultNullValueHandling;
      DefaultValueHandling = DefaultDefaultValueHandling;
      PreserveReferencesHandling = DefaultPreserveReferencesHandling;
      TypeNameHandling = DefaultTypeNameHandling;
      Converters = new List<JsonConverter>();
    }
  }
}