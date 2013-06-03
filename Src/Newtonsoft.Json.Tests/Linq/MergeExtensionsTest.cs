using System.Collections.Generic;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class MergeExtensionsTest
    {
        [Test]
        public void can_merge_a_property()
        {
            var left = JToken.FromObject(new {Property1 = 1});
            var right = JToken.FromObject(new {Property2 = 2});

           var result = left.Merge(right);

           Assert.AreEqual("{\"Property1\":1,\"Property2\":2}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_a_property_over_a_value()
        {
            var left = JToken.FromObject(1);
            var right = JToken.FromObject(new {Property2 = 2});

            var result = left.Merge(right);

            Assert.AreEqual("{\"Property2\":2}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_a_sub_property()
        {
            var left = JToken.FromObject(new { Property1 = new { SubProperty1 = 1 } });
            var right = JToken.FromObject(new { Property1 = new { SubProperty2 = 2 } });

            var result = left.Merge(right);

            Assert.AreEqual("{\"Property1\":{\"SubProperty1\":1,\"SubProperty2\":2}}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_properties_and_a_sub_properties()
        {
            var left = JToken.FromObject(new { Property1 = new { SubProperty1 = 1 } });
            var right = JToken.FromObject(new { Property1 = new { SubProperty2 = 2 }, Property2 = 2 });

            var result = left.Merge(right);

            Assert.AreEqual("{\"Property1\":{\"SubProperty1\":1,\"SubProperty2\":2},\"Property2\":2}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_objects_with_arrays_by_overwriting_as_default()
        {
            var left = JToken.FromObject(new
            {
                Array1 = new object[]
                        {
                            new {Property1 = 1}
                        }
            });
            var right = JToken.FromObject(new
            {
                Array1 = new object[]
                        {
                            new {Property2 = 2},
                            new {Property3 = 3}
                        }
            });

            var result = left.Merge(right);

            Assert.AreEqual("{\"Array1\":[{\"Property2\":2},{\"Property3\":3}]}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_objects_with_arrays_by_concat()
        {
            var left = JToken.FromObject(new
            {
                Array1 = new object[]
                        {
                            new {Property1 = 1}
                        }
            });
            var right = JToken.FromObject(new
            {
                Array1 = new object[]
                        {
                            new {Property2 = 2},
                            new {Property3 = 3}
                        }
            });

            var result = left.Merge(right, new MergeOptions{ArrayHandling = MergeOptionArrayHandling.Concat});

            Assert.AreEqual("{\"Array1\":[{\"Property1\":1},{\"Property2\":2},{\"Property3\":3}]}", result.ToString(Formatting.None));
        }

        [Test]
        public void can_merge_dictionaries()
        {
            var left = JToken.FromObject(new Dictionary<string, object>
                {
                    { "Item1", "Value1" }, 
                    { "Item2", "Value2" }
                });
            var right = JToken.FromObject(new Dictionary<string, object>
                {
                    { "Item2", "Value2new" }, 
                    { "Item3", "Value3" }
                });

            var result = left.Merge(right);

            Assert.AreEqual("{\"Item1\":\"Value1\",\"Item2\":\"Value2new\",\"Item3\":\"Value3\"}", result.ToString(Formatting.None));
        }
    }
}