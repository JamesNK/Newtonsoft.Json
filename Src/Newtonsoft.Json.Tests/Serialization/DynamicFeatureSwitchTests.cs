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

#if DNXCORE50
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using Test = Xunit.FactAttribute;

namespace Newtonsoft.Json.Tests.Serialization
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class DynamicFeatureSwitchCollection
    {
        public const string Name = "Dynamic feature switch";
    }

    [Collection(DynamicFeatureSwitchCollection.Name)]
    [TestFixture]
    public class DynamicFeatureSwitchTests : TestFixtureBase
    {
        [Test]
        public void DynamicFeatureSwitch_DisablesSerializerDynamicPaths()
        {
            const string switchName = "Newtonsoft.Json.Linq.JToken.DynamicIsSupported";

            AppContext.SetSwitch(switchName, true);
            DefaultContractResolver resolver = new DefaultContractResolver();
            resolver.ResolveContract(typeof(TestDynamicObject));

            AppContext.SetSwitch(switchName, false);
            try
            {
                ExceptionAssert.Throws<NotSupportedException>(
                    () => new DefaultContractResolver().ResolveContract(typeof(TestDynamicObject)),
                    JToken.DynamicNotSupportedMessage);

                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    ContractResolver = resolver
                });

                using (StringWriter stringWriter = new StringWriter())
                using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
                {
                    ExceptionAssert.Throws<NotSupportedException>(
                        () => serializer.Serialize(jsonWriter, new TestDynamicObject()),
                        JToken.DynamicNotSupportedMessage);
                }

                using (StringReader stringReader = new StringReader("{}"))
                using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
                {
                    ExceptionAssert.Throws<NotSupportedException>(
                        () => serializer.Deserialize<TestDynamicObject>(jsonReader),
                        JToken.DynamicNotSupportedMessage);
                }
            }
            finally
            {
                AppContext.SetSwitch(switchName, true);
            }
        }
    }
}
#endif