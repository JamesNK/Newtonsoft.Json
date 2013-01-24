using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class DataContractAndDataMember
  {
    [DataContract]
    public class File
    {
      // excluded from serialization
      // does not have DataMemberAttribute
      public Guid Id { get; set; }

      [DataMember]
      public string Name { get; set; }
      [DataMember]
      public int Size { get; set; }
    }

    public void Example()
    {
      File file = new File
        {
          Id = Guid.NewGuid(),
          Name = "ImportantLegalDocuments.docx",
          Size = 50*1024
        };

      string json = JsonConvert.SerializeObject(file, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "Name": "ImportantLegalDocuments.docx",
      //   "Size": 51200
      // }
    }
  }
}
