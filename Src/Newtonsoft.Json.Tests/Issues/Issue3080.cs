#if NET8_0_OR_GREATER
using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

#nullable enable

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue3080
    {
        [Test]
        public void OptionalProperty_DeserializationWithNullExplicitlySet_AllPropertiesHaveNull()
        {
            string json = @"{
	            ""optional_int"": null,
	            ""optional_nullable_int"": null,
	            ""optional_string"": null,
	            ""optional_nullable_string"": null,
	            ""optional_bool"": null,
	            ""optional_nullable_bool"": null,
	            ""optional_decimal"": null,
	            ""optional_nullable_decimal"": null,
	            ""optional_guid"": null,
	            ""optional_nullable_guid"": null,
	            ""iso3166_country_code"": null
            }";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new OptionalValueContractResolver(),
            };

            var testModel = JsonConvert.DeserializeObject<TestModel>(json, settings);

            Assert.IsNotNull(testModel);

            Assert.IsTrue(testModel!.OptionalNullableInt.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableInt);

            Assert.IsTrue(testModel.OptionalNullableString.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableString);

            Assert.IsTrue(testModel.OptionalNullableBool.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableBool);

            Assert.IsTrue(testModel.OptionalNullableDecimal.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableDecimal);

            Assert.IsTrue(testModel.OptionalNullableGuid.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableGuid);

            // These properties do not allow null, so the attempt to set them null is ignored
            Assert.IsFalse(testModel.OptionalInt.HasValue);
            Assert.IsFalse(testModel.OptionalBool.HasValue);
            Assert.IsFalse(testModel.OptionalDecimal.HasValue);
            Assert.IsFalse(testModel.OptionalGuid.HasValue);
        }

#if DNXCORE50
        [Theory]
#endif
        [TestCase(typeof(ReflectionValueProvider))]
        [TestCase(typeof(ExpressionValueProvider))]
        [TestCase(typeof(DynamicValueProvider))]
        public void IValueProvider_SetNullToOptionalNullableInt(Type valueProviderType)
        {
            var testModel = new TestModel();
            var propertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.OptionalNullableInt))!;
            var valueProvider = (IValueProvider)Activator.CreateInstance(valueProviderType, propertyInfo)!;

            // Set null value
            valueProvider.SetValue(testModel, null);

            // Property should remain unset (default) since null was passed
            Assert.IsFalse(testModel.OptionalNullableInt.HasValue);

            // Set Optional with explicit null value
            var optionalWithNull = new Optional<int?>(null);
            valueProvider.SetValue(testModel, optionalWithNull);

            // Property should now have value set (even though value is null)
            Assert.IsTrue(testModel.OptionalNullableInt.HasValue);
            Assert.AreEqual(null, testModel.OptionalNullableInt.Value);
        }
    }

    public class TestModel
    {
        public Optional<int> OptionalInt { get; set; }

        public Optional<int?> OptionalNullableInt { get; set; }

        public Optional<string> OptionalString { get; set; }

        public Optional<string?> OptionalNullableString { get; set; }

        public Optional<bool> OptionalBool { get; set; }

        public Optional<bool?> OptionalNullableBool { get; set; }

        public Optional<decimal> OptionalDecimal { get; set; }

        public Optional<decimal?> OptionalNullableDecimal { get; set; }

        public Optional<Guid> OptionalGuid { get; set; }

        public Optional<Guid?> OptionalNullableGuid { get; set; }

        /// <summary>
        /// Used to ensure snake_casing of property with numerics works as expected.
        /// </summary>
        public Optional<string> Iso3166CountryCode { get; set; }
    }

    /// <summary>
    /// Combines a value, <see cref="Value"/>, and a flag, <see cref="HasValue"/>, indicating whether or not that value is meaningful.
    /// This allows us to differentiate between properties which are simply not set at all (HasValue = false) and properties which are explicitly set to null (HasValue = true, Value = null).
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Newtonsoft.Json.JsonConverter(typeof(OptionalValueJsonConverter))]
    public readonly struct Optional<T> : IOptional, IComparable<Optional<T?>>
    {
        /// <summary>
        /// Constructs an <see cref="Optional{T}"/> with a meaningful value.
        /// </summary>
        /// <param name="value"></param>
        public Optional(T? value)
        {
            HasValue = true;
            Value = value;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the <see cref="Value"/> will return a meaningful value.
        /// </summary>
        /// <returns></returns>
        public bool HasValue { get; }

        /// <summary>
        /// Returns true if a Value has been set, and the Value is not null.
        /// </summary>
        [MemberNotNullWhen(returnValue: true, member: nameof(Value))]
        public bool HasNonNullValue => HasValue && Value != null;

        /// <summary>
        /// Returns true if no Value has been set, or the Value is null.
        /// </summary>
        /// <remarks>Same as !HasNonNullValue, but less confusing syntatically.</remarks>
        [MemberNotNullWhen(returnValue: false, member: nameof(Value))]
        public bool IsUnspecifiedOrNull => !HasValue || Value == null;

        /// <summary>
        /// Gets the value of the current object.  Not meaningful unless <see cref="HasValue"/> returns <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>Unlike <see cref="Nullable{T}.Value"/>, this property does not throw an exception when
        /// <see cref="HasValue"/> is <see langword="false"/>.</para>
        /// </remarks>
        /// <returns>
        /// <para>The value if <see cref="HasValue"/> is <see langword="true"/>; otherwise, the default value for type
        /// <typeparamref name="T"/>.</para>
        /// </returns>
        public T? Value { get; }

        /// <summary>
        /// Object version of the encapsulated value.
        /// </summary>
        public object? ValueObject => Value;

        /// <summary>
        /// Underlying Type of the Value.
        /// </summary>
        public Type ValueType => typeof(T);

        /// <summary>
        /// Creates a new object initialized to a meaningful value. 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Returns object value implicitly.
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator T?(Optional<T> value)
        {
            return value.Value;
        }

        #region Object overrides

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Optional<T?> a, Optional<T?> b)
        {
            return ValueEquals(a, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Optional<T?> a, Optional<T?> b)
        {
            return !ValueEquals(a, b);
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        public override string ToString()
        {
            // Note: For nullable types, it's possible to have _hasValue true and _value null.
            return HasValue ? Value?.ToString() ?? "null" : "unspecified";
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is Optional<T?> optionalObj) return ValueEquals(this, optionalObj);
            else if (obj is T matchingType) return ValueEquals(this, new Optional<T?>(matchingType));
            else return false;
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (!HasValue) return -1;
            else return Value!.GetHashCode();
        }

        private static bool ValueEquals(Optional<T?> a, Optional<T?> b)
        {
            if (!a.HasValue && !b.HasValue) return true; // both have no value. That is equivalent
            if (!a.HasValue || !b.HasValue) return false; // if only one has no value, they are not

            if (a.Value == null && b.Value == null) return true; // if both values are null. That is equivalent
            if (a.Value == null || b.Value == null) return false; // if only one value is null, they are not

            return a.Value.Equals(b.Value);
        }

        /// <summary>
        /// Comparison functionality.
        /// Only implemented for integers.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Optional<T?> other)
        {
            if (!(this.HasNonNullValue && other.HasNonNullValue)) return 0;

            // int -> int comparison override
            if (this.Value is int a && other.Value is int b)
            {
                if (a > b) return 1;
                else return -1;
            }

            return 0;
        }
        #endregion
    }

    /// <summary>
    /// Interface for Optional value types.
    /// </summary>
    public interface IOptional
    {
        /// <summary>
        /// Returns <see langword="true"/> if the Value will return a meaningful value.
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Underlying type of the Value.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Object version of the encapsulated value.
        /// </summary>
        object? ValueObject { get; }
    }

    /// <summary>
    /// Allows customisation of how models are de/serialized and exposed via the API.
    /// </summary>
    public class OptionalValueContractResolver : DefaultContractResolver
    {
        private static readonly MethodInfo? ShouldSerializeOptionalBuilderMethodInfo = typeof(OptionalValueContractResolver).GetMethod(nameof(OptionalValueContractResolver.ShouldSerializeOptionalBuilder), BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionalValueContractResolver()
        {
            // Our default naming strategy is to camelCase all properties, unless an override has been specified via an attribute on the property.
            NamingStrategy = new SnakeCaseNamingStrategy { OverrideSpecifiedNames = false };
        }

        /// <summary>
        /// Strongly typed Predicate factory, to be invoked only once for each type.
        /// </summary>
        public static Predicate<object> ShouldSerializeOptionalBuilder<T>(JsonProperty property)
        {
            return o =>
            {
                var v = property.ValueProvider?.GetValue(o);
                return v != null && ((Optional<T>)v).HasNonNullValue; // Some argument for using HasValue here instead, which would then return explicit nulls where we selected the value in SQL and it was DBNull
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var type = property.PropertyType;
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Optional<>))
            {
                var optionalValueType = type.GetGenericArguments()[0];
                property.ShouldSerialize = MakePredicateForOptionalType(optionalValueType, property);
            }

            return property;
        }

        /// <summary>
        /// For each base type, a ShouldSerialize handler is generated. 
        /// Using reflection at each serialization would be slow, so we use MethodInfo.MakeGenericMethod once and then return the cached Predicate.
        /// </summary>
        public static Predicate<object> MakePredicateForOptionalType(Type baseType, JsonProperty property)
        {
            // ShouldSerializeOptionalBuilderMethodInfo should never return null, as it's a reference to a method in this class!
            var typedMethod = ShouldSerializeOptionalBuilderMethodInfo!.MakeGenericMethod(baseType);

            var methodResult = typedMethod.Invoke(null, new object[] { property });

            // MethodResult should never be null, as the method we are invoking is not a ctor. 
            return (Predicate<object>)methodResult!;
        }
    }
    /// <summary>
    /// Optional Value Converter.
    /// Copied from CoreAPI.
    /// </summary>
    public class OptionalValueJsonConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, ITypedConverter> Converters = new ConcurrentDictionary<Type, ITypedConverter>();

        /// <summary>
        /// Simple interface implemented by strongly typed converters.
        /// </summary>
        public interface ITypedConverter
        {
            /// <summary>
            /// Converts a boxed value into a Optional instance and returns it.
            /// </summary>
            object? Deserialize(object? value);

            /// <summary>
            /// Extracts a value.
            /// </summary>
            object ExtractValue(object value);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is IOptional optionalValue)
            {
                // Literally no value set (not even null) so do not serialize.
                if (!optionalValue.HasValue)
                {
                    serializer.Serialize(writer, null);
                }
                else
                {
                    var jsonValue = optionalValue.ValueObject;

                    if (optionalValue.ValueObject == null)
                    {
                        serializer.Serialize(writer, null);
                    }
                    else
                    {
                        Type optionalType = optionalValue.ValueObject.GetType();
                        optionalType = Nullable.GetUnderlyingType(optionalType) ?? optionalType;

                        serializer.Serialize(writer, jsonValue);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var converter = GetConverter(objectType);

            var value = reader.Value;

            // Get the underlying type from the optional. If this is a nullable then drill through that to the real underlying type.
            var baseType = ResolveOptionalTypeParameter(objectType);
            if (baseType != null) baseType = Nullable.GetUnderlyingType(baseType) ?? baseType;

            // Handle nested objects, ignore depth 0 as this is the top level object and will just result in an overflow.
            if ((reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray) && reader.Depth != 0 && baseType != null)
            {
                var token = JToken.Load(reader);
                value = token.ToObject(baseType, serializer); // Replace the value with the deserialized object.
            }

            return converter.Deserialize(value);
        }

        private static ITypedConverter GetConverter(Type objectType)
        {
            if (!Converters.TryGetValue(objectType, out ITypedConverter? converter))
            {
                var valueType = ResolveOptionalTypeParameter(objectType);
                if (valueType != null)
                {
                    converter = Activator.CreateInstance(typeof(TypedConverter<>).MakeGenericType(valueType)) as ITypedConverter;

                    if (converter != null)
                    {
                        // Attempt to add the converter to the dictionary.
                        Converters.TryAdd(objectType, converter);
                    }
                    else
                    {
                        // Throw an exception if we were unable to create a converter.
                        throw new Exception($"Unable to instantiate TypedConverter for {objectType.Name}");
                    }
                }
                else
                {
                    // Throw an exception if we were unable to determine the optional type parameter.
                    throw new Exception($"Unable to resolve optional type parameter for {objectType.Name}");
                }
            }

            return converter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalType"></param>
        /// <returns></returns>
        public static Type? ResolveOptionalTypeParameter(Type optionalType)
        {
            var toCheck = optionalType;
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (typeof(Optional<>) == cur)
                {
                    return toCheck.GetGenericArguments().Single();
                }

                toCheck = toCheck.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType is IOptional optionalValue && optionalValue.HasValue;
        }

        /// <summary>
        /// Class implementing a strongly typed json converter.
        /// </summary>
        /// <typeparam name="T">c.</typeparam>
        public class TypedConverter<T> : ITypedConverter
        {
            /// <summary>
            /// Converts a boxed value into a Optional instance and returns it.
            /// </summary>
            public object? Deserialize(object? value)
            {
                if (value == null)
                {
                    if (Nullable.GetUnderlyingType(typeof(T)) != null) return (Optional<T?>)default(T); // If this type is a Nullable<T>, then return Optional<T> with value explicitly set null
                    else if (default(T) == null) return (Optional<T?>)default(T); // If this type is a reference type, then return Optional<T> with value explicitly set null
                    else return null; // otherwise this is a value type that cannot be set null. I'll return null (which leaves the value undefined)
                }

                try
                {
                    Type optionalType = typeof(T);
                    optionalType = Nullable.GetUnderlyingType(optionalType) ?? optionalType;

                    if (optionalType.IsEnum)
                    {
                        var enumValue = (T)Enum.Parse(optionalType, (string)value, true);
                        return (Optional<T?>)enumValue;
                    }

                    if (optionalType == typeof(Guid) && value is string guidString)
                    {
                        // Parse the string to a Guid.
                        value = Guid.Parse(guidString);
                    }

                    if (optionalType == typeof(byte[]) && value is string base64String)
                    {
                        // Convert from a base64 encoded string back to a byte array.
                        value = Convert.FromBase64String(base64String);
                    }

                    var actualValue = (T)Convert.ChangeType(value, optionalType);
                    return (Optional<T?>)actualValue;
                }
                catch (Exception)
                {
                    return default(Optional<T>);
                }
            }

            /// <summary>
            /// Extracts a value. 
            /// </summary>
            public object ExtractValue(object value)
            {
                return (Optional<T>)value;
            }
        }
    }
}
#endif