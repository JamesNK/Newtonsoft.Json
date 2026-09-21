using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    public class JsonConverter2Tests
    {
        [Test]
        public void SerializeCollectionWithDifferentSubTypesWithTypeNameHandlingAuto()
        {
            var contacts = new List<ContactModelBase>();
            var c1 = new ContactWithUSAddressModel();
            c1.FullName = "John Doe";
            c1.StreetAddress = "123 Main St";
            c1.City = "New York";
            c1.State = "New York";
            c1.ZipCode = "12345";
            c1.Country = "United States";
            
            var c2 = new ContactWithCanadianAddressModel();
            c2.FullName = "Jane Doe";
            c2.StreetAddress = "123 Main St";
            c2.City = "Toronto";
            c2.Province = "Ontario";
            c2.PostalCode = "J4K3N5";
            c2.Country = "Canada";

            var c3 = new ContactModelBase();
            c3.FullName = "FullName";
            c3.StreetAddress = "StreetAddress";
            c3.City = "CityName";
            c3.Country = "CountryName";
            
            contacts.Add(c1);
            contacts.Add(c2);
            contacts.Add(c3);
            
            var jsonActual = JsonConvert.SerializeObject(contacts, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new ContactConverter()
                },
                TypeNameHandling = TypeNameHandling.Auto
            });

            var jsonExpected = JsonConvert.SerializeObject(contacts, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(jsonExpected, jsonActual);
        }

        public class ContactModelBase
        {
            public string FullName { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }
        
        public class ContactWithUSAddressModel : ContactModelBase
        {
            public string ZipCode { get; set; }
            public string State { get; set; }
        }

        public class ContactWithCanadianAddressModel : ContactModelBase
        {
            public string PostalCode { get; set; }
            public string Province { get; set; }
        }

        public class ContactConverter : JsonConverter2
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType.IsSubclassOf(typeof(ContactModelBase));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer, JsonContract contract, JsonContainerContract collectionContract, JsonProperty containerProperty)
            {
                writer.WriteStartObject();
                if (collectionContract.ItemContract.UnderlyingType != value.GetType())
                {
                    writer.WritePropertyName("$type");
                    writer.WriteValue(ReflectionUtils.GetTypeName(value.GetType(), serializer.TypeNameAssemblyFormatHandling, serializer.SerializationBinder));
                }
                
                if (value is ContactWithUSAddressModel usContact)
                {
                    writer.WritePropertyName(nameof(ContactWithUSAddressModel.ZipCode));
                    serializer.Serialize(writer, usContact.ZipCode);
                    writer.WritePropertyName(nameof(ContactWithUSAddressModel.State));
                    serializer.Serialize(writer, usContact.State);
                    writer.WritePropertyName(nameof(ContactModelBase.FullName));
                    serializer.Serialize(writer, usContact.FullName);
                    writer.WritePropertyName(nameof(ContactModelBase.StreetAddress));
                    serializer.Serialize(writer, usContact.StreetAddress);
                    writer.WritePropertyName(nameof(ContactModelBase.City));
                    serializer.Serialize(writer, usContact.City);
                    writer.WritePropertyName(nameof(ContactModelBase.Country));
                    serializer.Serialize(writer, usContact.Country);
                }
                else if (value is ContactWithCanadianAddressModel canContact)
                {
                    writer.WritePropertyName(nameof(ContactWithCanadianAddressModel.PostalCode));
                    serializer.Serialize(writer, canContact.PostalCode);
                    writer.WritePropertyName(nameof(ContactWithCanadianAddressModel.Province));
                    serializer.Serialize(writer, canContact.Province);
                    writer.WritePropertyName(nameof(ContactModelBase.FullName));
                    serializer.Serialize(writer, canContact.FullName);
                    writer.WritePropertyName(nameof(ContactModelBase.StreetAddress));
                    serializer.Serialize(writer, canContact.StreetAddress);
                    writer.WritePropertyName(nameof(ContactModelBase.City));
                    serializer.Serialize(writer, canContact.City);
                    writer.WritePropertyName(nameof(ContactModelBase.Country));
                    serializer.Serialize(writer, canContact.Country);
                }
                
                writer.WriteEndObject();
            }
        }
    }
}