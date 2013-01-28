using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class MaxDepth
  {
    public void Example()
    {
      #region Usage
      string json = @"[
        [
          [
            '1',
            'Two',
            'III'
          ]
        ]
      ]";

      try
      {
        JsonConvert.DeserializeObject<List<IList<IList<string>>>>(json, new JsonSerializerSettings
          {
            MaxDepth = 2
          });
      }
      catch (JsonReaderException ex)
      {
        Console.WriteLine(ex.Message);
        // The reader's MaxDepth of 2 has been exceeded. Path '[0][0]', line 3, position 12.
      }
      #endregion
    }
  }
}