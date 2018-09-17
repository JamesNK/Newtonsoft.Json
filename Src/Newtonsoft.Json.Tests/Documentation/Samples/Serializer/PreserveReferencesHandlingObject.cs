#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class PreserveReferencesHandlingObject : TestFixtureBase
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

        [Test]
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

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""Name"": ""My Documents"",
  ""Parent"": {
    ""$id"": ""2"",
    ""Name"": ""Root"",
    ""Parent"": null,
    ""Files"": null
  },
  ""Files"": [
    {
      ""$id"": ""3"",
      ""Name"": ""ImportantLegalDocument.docx"",
      ""Parent"": {
        ""$ref"": ""1""
      }
    }
  ]
}", preserveReferenacesObjects);
        }
    }
}