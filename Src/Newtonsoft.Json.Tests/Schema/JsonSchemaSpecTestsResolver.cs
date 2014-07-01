using Newtonsoft.Json.Schema;
using System;
using System.IO;

namespace Newtonsoft.Json.Tests.Schema
{
    public class JsonSchemaSpecTestsResolver : JsonSchemaResolver
    {
        public JsonSchemaSpecTestsResolver()
        {
            ResolveExternals = true;
        }

        public override string GetRemoteSchemaContents(Uri location)
        {
            // Redirect remote calls to test path
#if NETFX_CORE
            string baseDirectory = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#else
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
            string baseTestPath = Path.Combine(Path.Combine(Path.Combine(baseDirectory, "Schema"), "Specs"), "remotes");
            Uri remotesTestPath = new Uri(baseTestPath);
            Uri draftTestPath = new Uri(Path.Combine(baseTestPath, "draft4schema.json"));

            if (location.ToString().StartsWith("http://localhost:1234"))
                location = new Uri(location.ToString().Replace("http://localhost:1234", remotesTestPath.ToString()));

            if (location.ToString().StartsWith("http://json-schema.org/draft-04/schema#", StringComparison.Ordinal))
                location = new Uri(location.ToString().Replace("http://json-schema.org/draft-04/schema#", draftTestPath.ToString()));

            return base.GetRemoteSchemaContents(location);
        }
    }
}