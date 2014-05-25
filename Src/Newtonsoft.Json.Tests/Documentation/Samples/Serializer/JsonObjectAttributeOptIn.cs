using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class JsonObjectAttributeOptIn
    {
        #region Types
        [JsonObject(MemberSerialization.OptIn)]
        public class File
        {
            // excluded from serialization
            // does not have JsonPropertyAttribute
            public Guid Id { get; set; }

            [JsonProperty]
            public string Name { get; set; }

            [JsonProperty]
            public int Size { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            File file = new File
            {
                Id = Guid.NewGuid(),
                Name = "ImportantLegalDocuments.docx",
                Size = 50 * 1024
            };

            string json = JsonConvert.SerializeObject(file, Formatting.Indented);

            Console.WriteLine(json);
            // {
            //   "Name": "ImportantLegalDocuments.docx",
            //   "Size": 51200
            // }
            #endregion
        }
    }
}