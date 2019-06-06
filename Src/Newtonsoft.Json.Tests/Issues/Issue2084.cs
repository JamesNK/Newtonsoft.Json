#if !(NET20 || NET35 || NET40)
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    public class Issue2084
    {
        [Test]
        public void Test()
        {
            var json = "{ \"test\":\"2011-11-15T10:23:06.000Z\" }";

            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.DateParseHandling = DateParseHandling.DateTimeOffset;

                var root = JToken.Load(reader);
                StringAssert.AreEqual("2011-11-15T10:23:06.000Z", root["test"].Value<string>());
            }
        }
    }
}
#endif