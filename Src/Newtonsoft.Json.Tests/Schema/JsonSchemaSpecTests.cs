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
    public class JsonSchemaSpecTest
    {
        public string FileName { get; set; }
        public string TestCaseDescription { get; set; }
        public JObject Schema { get; set; }
        public string TestDescription { get; set; }
        public JToken Data { get; set; }
        public bool IsValid { get; set; }
        public int TestNumber { get; set; }

        public override string ToString()
        {
            return FileName + " - " + TestCaseDescription + " - " + TestDescription;
        }
    }

    [TestFixture]
    public class JsonSchemaSpecTests : TestFixtureBase
    {
        [TestCaseSourceAttribute("GetSpecTestDetails")]
        public void SpecTest(JsonSchemaSpecTest jsonSchemaSpecTest)
        {
            //if (jsonSchemaSpecTest.ToString() == "enum.json - simple enum validation - something else is invalid")
            {
                Console.WriteLine("Running JSON Schema test " + jsonSchemaSpecTest.TestNumber + ": " + jsonSchemaSpecTest);

                JsonSchema s = JsonSchema.Read(jsonSchemaSpecTest.Schema.CreateReader());

                IList<string> errorMessages;
                bool v = jsonSchemaSpecTest.Data.IsValid(s, out errorMessages);
                errorMessages = errorMessages ?? new List<string>();

                Assert.AreEqual(jsonSchemaSpecTest.IsValid, v, jsonSchemaSpecTest.TestCaseDescription + " - " + jsonSchemaSpecTest.TestDescription + " - errors: " + string.Join(", ", errorMessages));
            }
        }

        public IList<JsonSchemaSpecTest> GetSpecTestDetails()
        {
            IList<JsonSchemaSpecTest> specTests = new List<JsonSchemaSpecTest>();

            // get test files location relative to the test project dll
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string baseTestPath = Path.Combine(baseDirectory, "Schema", "Specs");

            string[] testFiles = Directory.GetFiles(baseTestPath, "*.json", SearchOption.AllDirectories);

            // read through each of the *.json test files and extract the test details
            foreach (string testFile in testFiles)
            {
                string testJson = System.IO.File.ReadAllText(testFile);

                JArray a = JArray.Parse(testJson);

                foreach (JObject testCase in a)
                {
                    foreach (JObject test in testCase["tests"])
                    {
                        JsonSchemaSpecTest jsonSchemaSpecTest = new JsonSchemaSpecTest();

                        jsonSchemaSpecTest.FileName = Path.GetFileName(testFile);
                        jsonSchemaSpecTest.TestCaseDescription = (string)testCase["description"];
                        jsonSchemaSpecTest.Schema = (JObject)testCase["schema"];

                        jsonSchemaSpecTest.TestDescription = (string)test["description"];
                        jsonSchemaSpecTest.Data = test["data"];
                        jsonSchemaSpecTest.IsValid = (bool)test["valid"];
                        jsonSchemaSpecTest.TestNumber = specTests.Count + 1;

                        specTests.Add(jsonSchemaSpecTest);
                    }
                }
            }

            specTests = specTests.Where(s => s.FileName != "dependencies.json"
                                             && s.TestCaseDescription != "multiple disallow subschema"
                                             && s.TestCaseDescription != "types from separate schemas are merged"
                                             && s.TestCaseDescription != "when types includes a schema it should fully validate the schema"
                                             && s.TestCaseDescription != "types can include schemas").ToList();

            return specTests;
        }
    }
}