using System.Linq;
using Newtonsoft.Json.Linq;
#if !DNXCORE50
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    public class Issue2887
    {
// GAC doesn't exist in NetCore apps. Issue does not repro.
#if !PORTABLE
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2887
        public void LoadNewtonsoftWithGacConflict()
        {
            // TODO: verify test fails without build changes.
            // TODO: Fix test to work locally
            // TODO: Fix test + setup to ensure 

            // TestSetup: Run Gacutil -i v13.0.1 Newtonsoft.Json
            // Call Newtonsoft.Json.Test.dll RuntimeLoadTest
            // TestCleanup: Run Gacutil -u v13.0.1 Newtonsoft.Json
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2887
        public void NewtonsoftJsonHasPatchVersion()
        {
            var assemblyVersion = typeof(JsonConvert).GetTypeInfo().Assembly.Version;
            Assert.NotEqual(0, assemblyVersion.PatchVersion, "Newtonsoft needs a non-zero patch version in the assemblyversion to avoid issue 2887.");
        }
#endif

        public void RuntimeLoadTest()
        {
            var jObject = new JObject { { "NullProperty", false ? "0" : null } };

            var jToken = JToken.FromObject(jObject);
            
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);

            jObject = new JObject { { "NullProperty", (string)null } };

            jToken = JToken.FromObject(jObject);
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);
        }
    }
}
