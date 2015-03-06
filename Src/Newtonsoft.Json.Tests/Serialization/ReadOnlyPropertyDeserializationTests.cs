using System.IO;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
	[TestFixture]
	public class ReadOnlyPropertyDeserializationTests
	{
		[Test]
		public void Read_only_property_is_accessed_during_deserialization()
		{
			var serializer = new JsonSerializer {ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor};

			var stringWriter = new StringWriter();
			serializer.Serialize(new JsonTextWriter(stringWriter), new DomainClass {IntVal = 1});

			var json = stringWriter.ToString();

			serializer.Deserialize<DomainClass>(new JsonTextReader(new StringReader(json)));
		}

		private class DomainClass
		{
			// We have a read-only property that relies on a writable property.
			// This property should be serialized; but should not be referenced 
			// during the deserialization process. Because the object is in an 
			// invalid state, 
			public ValueClass ReadOnlyVal
			{
				get { return 100/IntVal > 0 ? new ValueClass() : null; }
			}

			public int IntVal { get; set; }
		}

		private class ValueClass
		{
		}
	}
}