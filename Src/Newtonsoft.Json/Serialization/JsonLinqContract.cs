using System;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonLinqContract : JsonContract
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLinqContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonLinqContract(Type underlyingType)
      : base(underlyingType)
    {
      ContractType = JsonContractType.Linq;
    }
  }
}