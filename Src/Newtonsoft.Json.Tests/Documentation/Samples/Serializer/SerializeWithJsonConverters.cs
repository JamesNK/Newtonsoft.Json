using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class SerializeWithJsonConverters
  {
    public void Example()
    {
      #region Usage
      List<StringComparison> stringComparisons = new List<StringComparison>
        {
          StringComparison.CurrentCulture,
          StringComparison.InvariantCulture
        };

      string jsonWithoutConverter = JsonConvert.SerializeObject(stringComparisons);

      Console.WriteLine(jsonWithoutConverter);
      // [0,2]

      string jsonWithConverter = JsonConvert.SerializeObject(stringComparisons, new StringEnumConverter());

      Console.WriteLine(jsonWithConverter);
      // ["CurrentCulture","InvariantCulture"]

      List<StringComparison> newStringComparsions = JsonConvert.DeserializeObject<List<StringComparison>>(
        jsonWithConverter,
        new StringEnumConverter());

      Console.WriteLine(string.Join(", ", newStringComparsions.Select(c => c.ToString())));
      // CurrentCulture, InvariantCulture
      #endregion
    }
  }
}