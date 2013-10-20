using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class TraceWriter
    {
        #region Types
        public class Account
        {
            public string FullName { get; set; }
            public bool Deleted { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            string json = @"{
              'FullName': 'Dan Deleted',
              'Deleted': true,
              'DeletedDate': '2013-01-20T00:00:00'
            }";

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            Account account = JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Console.WriteLine(traceWriter.ToString());
            // 2013-01-21T01:36:24.422 Info Started deserializing Newtonsoft.Json.Tests.Documentation.Examples.TraceWriter+Account. Path 'FullName', line 2, position 20.
            // 2013-01-21T01:36:24.442 Verbose Could not find member 'DeletedDate' on Newtonsoft.Json.Tests.Documentation.Examples.TraceWriter+Account. Path 'DeletedDate', line 4, position 23.
            // 2013-01-21T01:36:24.447 Info Finished deserializing Newtonsoft.Json.Tests.Documentation.Examples.TraceWriter+Account. Path '', line 5, position 8.
            #endregion
        }
    }
}