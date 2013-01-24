using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class JsonObjectAttributeOverrideIEnumerable
  {
    [JsonObject]
    public class Directory : IEnumerable<string>
    {
      public string Name { get; set; }
      public IList<string> Files { get; set; }

      public Directory()
      {
        Files = new List<string>();
      }

      public IEnumerator<string> GetEnumerator()
      {
        return Files.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }

    public void Example()
    {
      Directory directory = new Directory
        {
          Name = "My Documents",
          Files =
            {
              "ImportantLegalDocuments.docx",
              "WiseFinancalAdvice.xlsx"
            }
        };

      string json = JsonConvert.SerializeObject(directory, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "Name": "My Documents",
      //   "Files": [
      //     "ImportantLegalDocuments.docx",
      //     "WiseFinancalAdvice.xlsx"
      //   ]
      // }
    }
  }
}