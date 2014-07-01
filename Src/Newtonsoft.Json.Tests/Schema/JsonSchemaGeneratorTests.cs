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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Schema;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;
using Extensions = Newtonsoft.Json.Schema.Extensions;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Schema
{
    [TestFixture]
    public class JsonSchemaGeneratorTests : TestFixtureBase
    {
        [Test]
        public void Generate_GenericDictionary()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Dictionary<string, List<string>>));

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": [
      ""array"",
      ""null""
    ],
    ""items"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    }
  }
}", json);

            Dictionary<string, List<string>> value = new Dictionary<string, List<string>>
            {
                { "HasValue", new List<string>() { "first", "second", null } },
                { "NoValue", null }
            };

            string valueJson = JsonConvert.SerializeObject(value, Formatting.Indented);
            JObject o = JObject.Parse(valueJson);

            Assert.IsTrue(o.IsValid(schema));
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
        [Test]
        public void Generate_DefaultValueAttributeTestClass()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(DefaultValueAttributeTestClass));

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""description"": ""DefaultValueAttributeTestClass description!"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""TestField1"": {
      ""type"": ""integer"",
      ""default"": 21
    },
    ""TestProperty1"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""TestProperty1Value""
    }
  },
  ""required"": [
    ""TestField1"",
    ""TestProperty1""
  ]
}", json);
        }
#endif

        [Test]
        public void Generate_Person()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Person));

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""Person"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""title"": ""Title!"",
  ""description"": ""JsonObjectAttribute description!"",
  ""type"": ""object"",
  ""properties"": {
    ""Name"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""BirthDate"": {
      ""type"": ""string""
    },
    ""LastModified"": {
      ""type"": ""string""
    }
  },
  ""required"": [
    ""Name"",
    ""BirthDate"",
    ""LastModified""
  ]
}", json);
        }

        [Test]
        public void Generate_UserNullable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(UserNullable));

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""Id"": {
      ""type"": ""string""
    },
    ""FName"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""LName"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""RoleId"": {
      ""type"": ""integer""
    },
    ""NullableRoleId"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""NullRoleId"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""Active"": {
      ""type"": [
        ""boolean"",
        ""null""
      ]
    }
  },
  ""required"": [
    ""Id"",
    ""FName"",
    ""LName"",
    ""RoleId"",
    ""NullableRoleId"",
    ""NullRoleId"",
    ""Active""
  ]
}", json);
        }

        [Test]
        public void Generate_RequiredMembersClass()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(RequiredMembersClass));

            Assert.AreEqual(JsonSchemaType.String, schema.Properties["FirstName"].Type);
            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["MiddleName"].Type);
            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["LastName"].Type);
            Assert.AreEqual(JsonSchemaType.String, schema.Properties["BirthDate"].Type);
        }

        [Test]
        public void Generate_Store()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Store));

            Assert.AreEqual(11, schema.Properties.Count);

            JsonSchema productArraySchema = schema.Properties["product"];
            JsonSchema productSchema = productArraySchema.Items[0];

            Assert.AreEqual(4, productSchema.Properties.Count);
        }

        [Test]
        public void MissingSchemaIdHandlingTest()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(Store));
            Assert.AreEqual(null, schema.Id);

            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            schema = generator.Generate(typeof(Store));
            Assert.AreEqual(typeof(Store).FullName, schema.Id);

            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
            schema = generator.Generate(typeof(Store));
            Assert.AreEqual(typeof(Store).AssemblyQualifiedName, schema.Id);
        }

        [Test]
        public void CircularReferenceError()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                JsonSchemaGenerator generator = new JsonSchemaGenerator();
                generator.Generate(typeof(CircularReferenceClass));
            }, @"Unresolved circular reference for type 'Newtonsoft.Json.Tests.TestObjects.CircularReferenceClass'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.");
        }

        [Test]
        public void CircularReferenceWithTypeNameId()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(CircularReferenceClass), true);

            Assert.AreEqual(JsonSchemaType.String, schema.Properties["Name"].Type);
            Assert.AreEqual(typeof(CircularReferenceClass).FullName, schema.Id);
            Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
            Assert.AreEqual(schema, schema.Properties["Child"]);
        }

        [Test]
        public void CircularReferenceWithExplicitId()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(CircularReferenceWithIdClass));

            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["Name"].Type);
            Assert.AreEqual("MyExplicitId", schema.Id);
            Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
            Assert.AreEqual(schema, schema.Properties["Child"]);
        }

        [Test]
        public void GenerateSchemaForType()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Type));

            Assert.AreEqual(JsonSchemaType.String, schema.Type);

            string json = JsonConvert.SerializeObject(typeof(Version), Formatting.Indented);

            JValue v = new JValue(json);
            Assert.IsTrue(v.IsValid(schema));
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
        [Test]
        public void GenerateSchemaForISerializable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Exception));

            Assert.AreEqual(JsonSchemaType.Object, schema.Type);
            Assert.AreEqual(true, schema.AllowAdditionalProperties);
            Assert.AreEqual(null, schema.Properties);
        }
#endif

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
        [Test]
        public void GenerateSchemaForDBNull()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(DBNull));

            Assert.AreEqual(JsonSchemaType.Null, schema.Type);
        }

        public class CustomDirectoryInfoMapper : DefaultContractResolver
        {
            public CustomDirectoryInfoMapper()
                : base(true)
            {
            }

            protected override JsonContract CreateContract(Type objectType)
            {
                if (objectType == typeof(DirectoryInfo))
                    return base.CreateObjectContract(objectType);

                return base.CreateContract(objectType);
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                JsonPropertyCollection c = new JsonPropertyCollection(type);
                c.AddRange(properties.Where(m => m.PropertyName != "Root"));

                return c;
            }
        }

        [Test]
        public void GenerateSchemaForDirectoryInfo()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            generator.ContractResolver = new CustomDirectoryInfoMapper
            {
#if !(NETFX_CORE || PORTABLE)
                IgnoreSerializableAttribute = true
#endif
            };

            JsonSchema schema = generator.Generate(typeof(DirectoryInfo), true);

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""System.IO.DirectoryInfo"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""Name"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""Parent"": {
      ""$ref"": ""System.IO.DirectoryInfo""
    },
    ""Exists"": {
      ""type"": ""boolean""
    },
    ""FullName"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""Extension"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""CreationTime"": {
      ""type"": ""string""
    },
    ""CreationTimeUtc"": {
      ""type"": ""string""
    },
    ""LastAccessTime"": {
      ""type"": ""string""
    },
    ""LastAccessTimeUtc"": {
      ""type"": ""string""
    },
    ""LastWriteTime"": {
      ""type"": ""string""
    },
    ""LastWriteTimeUtc"": {
      ""type"": ""string""
    },
    ""Attributes"": {
      ""type"": ""integer""
    }
  },
  ""required"": [
    ""Name"",
    ""Parent"",
    ""Exists"",
    ""FullName"",
    ""Extension"",
    ""CreationTime"",
    ""CreationTimeUtc"",
    ""LastAccessTime"",
    ""LastAccessTimeUtc"",
    ""LastWriteTime"",
    ""LastWriteTimeUtc"",
    ""Attributes""
  ]
}", json);

            DirectoryInfo temp = new DirectoryInfo(@"c:\temp");

            JTokenWriter jsonWriter = new JTokenWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IsoDateTimeConverter());
            serializer.ContractResolver = new CustomDirectoryInfoMapper
            {
#if !(NETFX_CORE || PORTABLE)
                IgnoreSerializableInterface = true
#endif
            };
            serializer.Serialize(jsonWriter, temp);

            List<string> errors = new List<string>();
            jsonWriter.Token.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.AreEqual(0, errors.Count);
        }
#endif

        [Test]
        public void GenerateSchemaCamelCase()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            generator.ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
#if !(NETFX_CORE || PORTABLE || PORTABLE40)
                IgnoreSerializableAttribute = true
#endif
            };

            JsonSchema schema = generator.Generate(typeof(Version), true);

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""System.Version"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""major"": {
      ""type"": ""integer""
    },
    ""minor"": {
      ""type"": ""integer""
    },
    ""build"": {
      ""type"": ""integer""
    },
    ""revision"": {
      ""type"": ""integer""
    },
    ""majorRevision"": {
      ""type"": ""integer""
    },
    ""minorRevision"": {
      ""type"": ""integer""
    }
  },
  ""required"": [
    ""major"",
    ""minor"",
    ""build"",
    ""revision"",
    ""majorRevision"",
    ""minorRevision""
  ]
}", json);
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
        [Test]
        public void GenerateSchemaSerializable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Version), true);

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""System.Version"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""_Major"": {
      ""type"": ""integer""
    },
    ""_Minor"": {
      ""type"": ""integer""
    },
    ""_Build"": {
      ""type"": ""integer""
    },
    ""_Revision"": {
      ""type"": ""integer""
    }
  },
  ""required"": [
    ""_Major"",
    ""_Minor"",
    ""_Build"",
    ""_Revision""
  ]
}", json);

            JTokenWriter jsonWriter = new JTokenWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            serializer.Serialize(jsonWriter, new Version(1, 2, 3, 4));

            List<string> errors = new List<string>();
            jsonWriter.Token.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.AreEqual(0, errors.Count);

            StringAssert.AreEqual(@"{
  ""_Major"": 1,
  ""_Minor"": 2,
  ""_Build"": 3,
  ""_Revision"": 4
}", jsonWriter.Token.ToString());

            Version version = jsonWriter.Token.ToObject<Version>(serializer);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Build);
            Assert.AreEqual(4, version.Revision);
        }
#endif

        public enum SortTypeFlag
        {
            No = 0,
            Asc = 1,
            Desc = -1
        }

        public class X
        {
            public SortTypeFlag x;
        }

        [Test]
        public void GenerateSchemaWithNegativeEnum()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();
            JsonSchema schema = jsonSchemaGenerator.Generate(typeof(X));

            string json = schema.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""x"": {
      ""type"": ""integer"",
      ""enum"": [
        0,
        1,
        -1
      ]
    }
  },
  ""required"": [
    ""x""
  ]
}", json);
        }

        [Test]
        public void CircularCollectionReferences()
        {
            Type type = typeof(Workspace);
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(type);

            // should succeed
            Assert.IsNotNull(jsonSchema);
        }

        [Test]
        public void CircularReferenceWithMixedRequires()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(CircularReferenceClass));
            string json = jsonSchema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""Newtonsoft.Json.Tests.TestObjects.CircularReferenceClass"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""Name"": {
      ""type"": ""string""
    },
    ""Child"": {
      ""$ref"": ""Newtonsoft.Json.Tests.TestObjects.CircularReferenceClass""
    }
  },
  ""required"": [
    ""Name""
  ]
}", json);
        }

        [Test]
        public void JsonPropertyWithHandlingValues()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(JsonPropertyWithHandlingValues));
            string json = jsonSchema.ToString();

            StringAssert.AreEqual(@"{
  ""id"": ""Newtonsoft.Json.Tests.TestObjects.JsonPropertyWithHandlingValues"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""DefaultValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIncludeProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingPopulateProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIgnoreAndPopulateProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""NullValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""NullValueHandlingIncludeProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""ReferenceLoopHandlingErrorProperty"": {
      ""$ref"": ""Newtonsoft.Json.Tests.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingIgnoreProperty"": {
      ""$ref"": ""Newtonsoft.Json.Tests.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingSerializeProperty"": {
      ""$ref"": ""Newtonsoft.Json.Tests.TestObjects.JsonPropertyWithHandlingValues""
    }
  },
  ""required"": [
    ""DefaultValueHandlingIncludeProperty"",
    ""DefaultValueHandlingPopulateProperty"",
    ""NullValueHandlingIncludeProperty"",
    ""ReferenceLoopHandlingErrorProperty"",
    ""ReferenceLoopHandlingIgnoreProperty"",
    ""ReferenceLoopHandlingSerializeProperty""
  ]
}", json);
        }

        [Test]
        public void GenerateForNullableInt32()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(NullableInt32TestClass));
            string json = jsonSchema.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""Value"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    }
  },
  ""required"": [
    ""Value""
  ]
}", json);
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SortTypeFlagAsString
        {
            No = 0,
            Asc = 1,
            Desc = -1
        }

        public class Y
        {
            public SortTypeFlagAsString y;
        }

        [Test]
        [Ignore]
        public void GenerateSchemaWithStringEnum()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();
            JsonSchema schema = jsonSchemaGenerator.Generate(typeof(Y));

            string json = schema.ToString();

            // NOTE: This fails because the enum is serialized as an integer and not a string.
            // NOTE: There should exist a way to serialize the enum as lowercase strings.
            Assert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""y"": {
      ""type"": ""string"",
      ""enum"": [
        ""no"",
        ""asc"",
        ""desc""
      ]
    }
  },
  ""required"": [
    ""y""
  ]
}", json);
        }
    }

    public class NullableInt32TestClass
    {
        public int? Value { get; set; }
    }

    public class DMDSLBase
    {
        public String Comment;
    }

    public class Workspace : DMDSLBase
    {
        public ControlFlowItemCollection Jobs = new ControlFlowItemCollection();
    }

    public class ControlFlowItemBase : DMDSLBase
    {
        public String Name;
    }

    public class ControlFlowItem : ControlFlowItemBase //A Job
    {
        public TaskCollection Tasks = new TaskCollection();
        public ContainerCollection Containers = new ContainerCollection();
    }

    public class ControlFlowItemCollection : List<ControlFlowItem>
    {
    }

    public class Task : ControlFlowItemBase
    {
        public DataFlowTaskCollection DataFlowTasks = new DataFlowTaskCollection();
        public BulkInsertTaskCollection BulkInsertTask = new BulkInsertTaskCollection();
    }

    public class TaskCollection : List<Task>
    {
    }

    public class Container : ControlFlowItemBase
    {
        public ControlFlowItemCollection ContainerJobs = new ControlFlowItemCollection();
    }

    public class ContainerCollection : List<Container>
    {
    }

    public class DataFlowTask_DSL : ControlFlowItemBase
    {
    }

    public class DataFlowTaskCollection : List<DataFlowTask_DSL>
    {
    }

    public class SequenceContainer_DSL : Container
    {
    }

    public class BulkInsertTaskCollection : List<BulkInsertTask_DSL>
    {
    }

    public class BulkInsertTask_DSL
    {
    }
}