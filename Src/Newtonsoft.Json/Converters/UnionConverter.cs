using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts types marked with System.Runtime.CompilerServices.UnionAttribute to and from their contained JSON values.
    /// </summary>
    [RequiresUnreferencedCode(MiscellaneousUtils.TrimWarning)]
    [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
    public class UnionConverter : JsonConverter
    {
        private sealed class UnionCase
        {
            public readonly Type Type;
            public readonly ObjectConstructor<object> Constructor;
            public readonly bool AcceptsNull;
            public MethodInfo? TryGetValue;

            [RequiresUnreferencedCode(MiscellaneousUtils.TrimWarning)]
            [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
            public UnionCase(Type type, ConstructorInfo constructor)
            {
                Type = type;
                Constructor = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
                AcceptsNull = IsNullableParameter(constructor.GetParameters()[0]);
            }
        }

        private sealed class Union
        {
            public readonly Func<object, object?> GetValue;
            public readonly List<UnionCase> Cases;
            public readonly UnionCase? NullableCase;

            public Union(Func<object, object?> getValue, List<UnionCase> cases, UnionCase? nullableCase)
            {
                GetValue = getValue;
                Cases = cases;
                NullableCase = nullableCase;
            }
        }

        private enum ValueShape
        {
            None,
            Object,
            Array,
            String,
            Number,
            Boolean,
            Any
        }

        private static readonly ThreadSafeStore<Type, Union> UnionCache = new ThreadSafeStore<Type, Union>(CreateUnion);

        [ThreadStatic]
        private static List<JsonWriter>? _writeStack;

        private static Union CreateUnion(Type type)
        {
            PropertyInfo? property = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(object) || property.GetGetMethod() == null || property.GetIndexParameters().Length != 0)
            {
                throw new JsonSerializationException("Union type '{0}' must have a public object Value getter.".FormatWith(CultureInfo.InvariantCulture, type));
            }

            List<UnionCase> cases = new List<UnionCase>();
            UnionCase? nullableCase = null;
            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length == 1 && !parameters[0].ParameterType.IsByRef &&
                    !parameters[0].ParameterType.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                    !cases.Exists(unionCase => unionCase.Type == parameters[0].ParameterType))
                {
                    UnionCase unionCase = new UnionCase(parameters[0].ParameterType, constructor);
                    cases.Add(unionCase);
                    if (nullableCase == null && unionCase.AcceptsNull)
                    {
                        nullableCase = unionCase;
                    }
                }
            }

            if (cases.Count == 0)
            {
                throw new JsonSerializationException("Union type '{0}' must have public single-parameter constructors.".FormatWith(CultureInfo.InvariantCulture, type));
            }

            List<UnionCase> orderedCases = new List<UnionCase>();
            while (cases.Count > 0)
            {
                foreach (UnionCase candidate in cases)
                {
                    bool hasDerivedCase = false;
                    foreach (UnionCase other in cases)
                    {
                        if (candidate != other && candidate.Type != other.Type && IsAssignableCase(candidate.Type, other.Type))
                        {
                            hasDerivedCase = true;
                            break;
                        }
                    }

                    if (!hasDerivedCase)
                    {
                        orderedCases.Add(candidate);
                        cases.Remove(candidate);
                        break;
                    }
                }
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != "TryGetValue" || method.ReturnType != typeof(bool) || method.IsGenericMethod)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].IsOut && parameters[0].ParameterType.IsByRef)
                {
                    foreach (UnionCase unionCase in orderedCases)
                    {
                        if (unionCase.Type == parameters[0].ParameterType.GetElementType())
                        {
                            unionCase.TryGetValue = method;
                            break;
                        }
                    }
                }
            }

            return new Union(JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(property), orderedCases, nullableCase);
        }

        private static bool IsNullableParameter(ParameterInfo parameter)
        {
            if (parameter.ParameterType.IsValueType())
            {
                return ReflectionUtils.IsNullableType(parameter.ParameterType);
            }

#if NET6_0_OR_GREATER
            return new NullabilityInfoContext().Create(parameter).WriteState != NullabilityState.NotNull;
#else
            byte? flag = GetNullableFlag(CustomAttributeData.GetCustomAttributes(parameter), "NullableAttribute");
            if (flag == null)
            {
                flag = GetNullableFlag(CustomAttributeData.GetCustomAttributes(parameter.Member), "NullableContextAttribute");
                for (Type? declaringType = parameter.Member.DeclaringType; flag == null && declaringType != null; declaringType = declaringType.DeclaringType)
                {
                    flag = GetNullableFlag(CustomAttributeData.GetCustomAttributes(declaringType), "NullableContextAttribute");
                }
            }

            return flag != 1;
#endif
        }

#if !NET6_0_OR_GREATER
        private static byte? GetNullableFlag(IList<CustomAttributeData> attributes, string name)
        {
            foreach (CustomAttributeData attribute in attributes)
            {
                if (attribute.Constructor.DeclaringType!.FullName == "System.Runtime.CompilerServices." + name && attribute.ConstructorArguments.Count == 1)
                {
                    object? argument = attribute.ConstructorArguments[0].Value;
                    if (argument is byte flag)
                    {
                        return flag;
                    }
                    if (argument is IList<CustomAttributeTypedArgument> flags && flags.Count > 0)
                    {
                        return (byte)flags[0].Value!;
                    }
                }
            }

            return null;
        }
#endif

        private static bool IsAssignableCase(Type caseType, Type valueType)
        {
            return caseType.IsAssignableFrom(valueType) || Nullable.GetUnderlyingType(caseType) == valueType;
        }

        private static UnionCase? FindCase(Union union, Type valueType)
        {
            foreach (UnionCase unionCase in union.Cases)
            {
                if (IsAssignableCase(unionCase.Type, valueType))
                {
                    return unionCase;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            Type type = ReflectionUtils.EnsureNotNullableType(objectType);
            foreach (CustomAttributeData attribute in CustomAttributeData.GetCustomAttributes(type))
            {
                if (attribute.Constructor.DeclaringType!.FullName == "System.Runtime.CompilerServices.UnionAttribute")
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Type unionType = value.GetType();
            Union union = UnionCache.Get(unionType);
            object? caseValue = null;
            UnionCase? selectedCase = null;
            foreach (UnionCase unionCase in union.Cases)
            {
                if (unionCase.TryGetValue != null)
                {
                    object?[] arguments = { null };
                    if ((bool)unionCase.TryGetValue.Invoke(value, arguments)!)
                    {
                        caseValue = arguments[0];
                        selectedCase = unionCase;
                        break;
                    }
                }
            }

            if (selectedCase == null)
            {
                caseValue = union.GetValue(value);
                if (caseValue != null)
                {
                    selectedCase = FindCase(union, caseValue.GetType());
                    if (selectedCase == null)
                    {
                        throw JsonSerializationException.Create(null, writer.Path, "Value type '{0}' does not match any case of union type '{1}'.".FormatWith(CultureInfo.InvariantCulture, caseValue.GetType(), unionType), null);
                    }
                }
            }

            if (caseValue == null)
            {
                writer.WriteNull();
            }
            else
            {
                List<JsonWriter> writeStack = _writeStack ?? (_writeStack = new List<JsonWriter>());
                int depth = 0;
                foreach (JsonWriter activeWriter in writeStack)
                {
                    if (ReferenceEquals(activeWriter, writer))
                    {
                        depth++;
                    }
                }

                if (depth >= (serializer.MaxDepth ?? 64))
                {
                    throw JsonSerializationException.Create(null, writer.Path, "Union nesting exceeds the maximum depth.", null);
                }

                writeStack.Add(writer);
                try
                {
                    serializer.Serialize(writer, caseValue, unionType);
                }
                finally
                {
                    writeStack.RemoveAt(writeStack.Count - 1);
                }
            }
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Type unionType = ReflectionUtils.EnsureNotNullableType(objectType);
            Union union = UnionCache.Get(unionType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (ReflectionUtils.IsNullableType(objectType))
                {
                    return null;
                }

                if (union.NullableCase != null)
                {
                    return union.NullableCase.Constructor(new object?[] { null });
                }

                return unionType.IsValueType() ? Array.CreateInstance(unionType, 1).GetValue(0) : null;
            }

            JToken token = JToken.ReadFrom(reader);

            if (token is JObject jsonObject && serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore)
            {
                JToken? typeToken = GetMetadataProperty(jsonObject, JsonTypeReflector.TypePropertyName, serializer.MetadataPropertyHandling);
                if (typeToken != null && serializer.TypeNameHandling != TypeNameHandling.None)
                {
                    if (typeToken.Type != JTokenType.String)
                    {
                        throw JsonSerializationException.Create(reader, "Error reading '$type' metadata property. Property must have a string value.");
                    }

                    string qualifiedName = (string)typeToken!;
                    StructMultiKey<string?, string> typeName = ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedName);
                    Type? caseType;
                    try
                    {
                        caseType = serializer.SerializationBinder.BindToType(typeName.Value1, typeName.Value2);
                    }
                    catch (Exception exception)
                    {
                        throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedName), exception);
                    }

                    UnionCase? typedCase = caseType != null ? FindCase(union, caseType) : null;
                    if (typedCase == null)
                    {
                        throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' does not match any case of union type '{1}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedName, unionType));
                    }

                    jsonObject.Remove(JsonTypeReflector.TypePropertyName);
                    return ConstructCase(reader, unionType, union, typedCase, token.ToObject(caseType!, serializer));
                }

                if (GetMetadataProperty(jsonObject, JsonTypeReflector.RefPropertyName, serializer.MetadataPropertyHandling)?.Type == JTokenType.String)
                {
                    object? referencedValue = token.ToObject<object>(serializer);
                    UnionCase? referenceCase = referencedValue != null ? FindCase(union, referencedValue.GetType()) : null;
                    if (referenceCase == null)
                    {
                        throw JsonSerializationException.Create(reader, "Referenced value does not match any case of union type '{0}'.".FormatWith(CultureInfo.InvariantCulture, unionType));
                    }

                    return referenceCase.Constructor(referencedValue);
                }
            }

            ValueShape shape = GetTokenShape(token.Type);
            UnionCase? match = null;
            foreach (UnionCase unionCase in union.Cases)
            {
                ValueShape caseShape = GetCaseShape(unionCase.Type, serializer);
                if (shape != ValueShape.None && (caseShape == shape || caseShape == ValueShape.Any))
                {
                    if (match != null)
                    {
                        throw JsonSerializationException.Create(reader, "JSON value type '{0}' is ambiguous for union type '{1}'.".FormatWith(CultureInfo.InvariantCulture, shape, unionType));
                    }

                    match = unionCase;
                }
            }

            if (match == null)
            {
                throw JsonSerializationException.Create(reader, "JSON value type '{0}' does not match any case of union type '{1}'.".FormatWith(CultureInfo.InvariantCulture, shape, unionType));
            }

            return ConstructCase(reader, unionType, union, match, token.ToObject(match.Type, serializer));
        }

        private static object ConstructCase(JsonReader reader, Type unionType, Union union, UnionCase unionCase, object? value)
        {
            if (value == null)
            {
                if (union.NullableCase == null)
                {
                    throw JsonSerializationException.Create(reader, "Union type '{0}' does not accept a null case value.".FormatWith(CultureInfo.InvariantCulture, unionType));
                }

                unionCase = union.NullableCase;
            }

            try
            {
                return unionCase.Constructor(value);
            }
            catch (Exception exception)
            {
                throw JsonSerializationException.Create(reader, "Error constructing union type '{0}'.".FormatWith(CultureInfo.InvariantCulture, unionType), exception);
            }
        }

        private static JToken? GetMetadataProperty(JObject value, string name, MetadataPropertyHandling handling)
        {
            if (handling == MetadataPropertyHandling.ReadAhead)
            {
                return value[name];
            }

            foreach (JProperty property in value.Properties())
            {
                if (property.Name == name)
                {
                    return property.Value;
                }
                if (property.Name != JsonTypeReflector.TypePropertyName && property.Name != JsonTypeReflector.IdPropertyName && property.Name != JsonTypeReflector.RefPropertyName)
                {
                    break;
                }
            }

            return null;
        }

        private static ValueShape GetTokenShape(JTokenType tokenType)
        {
            switch (tokenType)
            {
                case JTokenType.Object: return ValueShape.Object;
                case JTokenType.Array: return ValueShape.Array;
                case JTokenType.String:
                case JTokenType.Date:
                case JTokenType.Bytes: return ValueShape.String;
                case JTokenType.Integer:
                case JTokenType.Float: return ValueShape.Number;
                case JTokenType.Boolean: return ValueShape.Boolean;
                default: return ValueShape.None;
            }
        }

        private static ValueShape GetCaseShape(Type type, JsonSerializer serializer)
        {
            JsonContract contract = serializer.ContractResolver.ResolveContract(type);
            JsonConverter? converter = contract.Converter ?? JsonSerializer.GetMatchingConverter(serializer.Converters, type) ?? contract.InternalConverter;
            if (converter is UnionConverter)
            {
                return ValueShape.None;
            }
            if (converter != null || type == typeof(object))
            {
                return ValueShape.Any;
            }

            switch (contract.ContractType)
            {
                case JsonContractType.Array: return ValueShape.Array;
                case JsonContractType.Object:
                case JsonContractType.Dictionary:
                case JsonContractType.Dynamic:
                case JsonContractType.Serializable: return ValueShape.Object;
                case JsonContractType.String: return ValueShape.String;
                case JsonContractType.Linq: return ValueShape.Any;
                case JsonContractType.Primitive:
                    PrimitiveTypeCode code = ConvertUtils.GetTypeCode(contract.NonNullableUnderlyingType);
                    switch (code)
                    {
                        case PrimitiveTypeCode.Boolean: return ValueShape.Boolean;
                        case PrimitiveTypeCode.Char:
                        case PrimitiveTypeCode.DateTime:
                        case PrimitiveTypeCode.DateTimeOffset:
                        case PrimitiveTypeCode.Guid:
                        case PrimitiveTypeCode.TimeSpan:
                        case PrimitiveTypeCode.Uri:
                        case PrimitiveTypeCode.String:
                        case PrimitiveTypeCode.Bytes: return ValueShape.String;
                        case PrimitiveTypeCode.Empty:
                        case PrimitiveTypeCode.DBNull: return ValueShape.None;
                        default: return ValueShape.Number;
                    }
                default: return ValueShape.Any;
            }
        }
    }
}