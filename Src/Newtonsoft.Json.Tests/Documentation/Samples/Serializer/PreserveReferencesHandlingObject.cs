using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class PreserveReferencesHandlingObject
  {
    #region Types
    public class Directory
    {
      public string Name { get; set; }
      public Directory Parent { get; set; }
      public IList<File> Files { get; set; }
    }

    public class File
    {
      public string Name { get; set; }
      public Directory Parent { get; set; } 
    }
    #endregion

    public void Example()
    {
      #region Usage
      Directory root = new Directory { Name = "Root" };
      Directory documents = new Directory { Name = "My Documents", Parent = root };

      File file = new File { Name = "ImportantLegalDocument.docx", Parent = documents };

      documents.Files = new List<File> { file };

      try
      {
        JsonConvert.SerializeObject(documents, Formatting.Indented);
      }
      catch (JsonSerializationException)
      {
        // Self referencing loop detected for property 'Parent' with type
        // 'Newtonsoft.Json.Tests.Documentation.Examples.ReferenceLoopHandlingObject+Directory'. Path 'Files[0]'.
      }

      string preserveReferenacesAll = JsonConvert.SerializeObject(documents, Formatting.Indented, new JsonSerializerSettings
        {
          PreserveReferencesHandling = PreserveReferencesHandling.All
        });

      Console.WriteLine(preserveReferenacesAll);
      // {
      //   "$id": "1",
      //   "Name": "My Documents",
      //   "Parent": {
      //     "$id": "2",
      //     "Name": "Root",
      //     "Parent": null,
      //     "Files": null
      //   },
      //   "Files": {
      //     "$id": "3",
      //     "$values": [
      //       {
      //         "$id": "4",
      //         "Name": "ImportantLegalDocument.docx",
      //         "Parent": {
      //           "$ref": "1"
      //         }
      //       }
      //     ]
      //   }
      // }

      string preserveReferenacesObjects = JsonConvert.SerializeObject(documents, Formatting.Indented, new JsonSerializerSettings
      {
        PreserveReferencesHandling = PreserveReferencesHandling.Objects
      });

      Console.WriteLine(preserveReferenacesObjects);
      // {
      //   "$id": "1",
      //   "Name": "My Documents",
      //   "Parent": {
      //     "$id": "2",
      //     "Name": "Root",
      //     "Parent": null,
      //     "Files": null
      //   },
      //   "Files": [
      //     {
      //       "$id": "3",
      //       "Name": "ImportantLegalDocument.docx",
      //       "Parent": {
      //         "$ref": "1"
      //       }
      //     }
      //   ]
      // }
      #endregion
    }
  }
}