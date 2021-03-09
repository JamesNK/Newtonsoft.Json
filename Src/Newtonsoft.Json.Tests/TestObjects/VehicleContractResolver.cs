using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class VehicleContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(Type type, MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(type, member, memberSerialization);
            
            // Only ignore the registration for Trains
            if (member.Name == nameof(Vehicle.Registration) && type == typeof(Train))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}