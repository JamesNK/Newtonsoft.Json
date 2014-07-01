using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Newtonsoft.Json.Tests.Schema
{
    public class JsonSchemaSpecTestsResolver : JsonSchemaResolver
    {
        public override string GetRemoteSchemaContents(Uri location)
        {
            // Redirect remote calls to test path
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string baseTestPath = Path.Combine(baseDirectory, "Schema", "Specs", "remotes");
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