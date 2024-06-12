﻿#region License
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

#if HAVE_ASYNC

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if HAVE_DYNAMIC
using System.Dynamic;
#endif
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
#endif

namespace Newtonsoft.Json.Serialization
{
    internal partial class JsonSerializerInternalWriter : JsonSerializerInternalBase

    {
        public async Task SerializeAsync(JsonWriter jsonWriter, object? value, Type? objectType, CancellationToken cancellationToken)
        {
            if (jsonWriter == null)
            {
                throw new ArgumentNullException(nameof(jsonWriter));
            }

            _rootType = objectType;
            _rootLevel = _serializeStack.Count + 1;

            JsonContract? contract = GetContractSafe(value);

            try
            {
                if (ShouldWriteReference(value, null, contract, null, null))
                {
                    await WriteReferenceAsync(jsonWriter, value!, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await SerializeValueAsync(jsonWriter, value, contract, null, null, null, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(null, contract, null, null, jsonWriter.Path, ex))
                {
                    await HandleErrorAsync(jsonWriter, 0, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // clear context in case serializer is being used inside a converter
                    // if the converter wraps the error then not clearing the context will cause this error:
                    // "Current error context error is different to requested error."
                    ClearErrorContext();
                    throw;
                }
            }
            finally
            {
                // clear root contract to ensure that if level was > 1 then it won't
                // accidentally be used for non root values
                _rootType = null;
            }
        }

        private async Task SerializePrimitiveAsync(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            if (contract.TypeCode == PrimitiveTypeCode.Bytes)
            {
                // if type name handling is enabled then wrap the base64 byte string in an object with the type name
                bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty);
                if (includeTypeDetails)
                {
                    await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
                    await WriteTypePropertyAsync(writer, contract.CreatedType, cancellationToken).ConfigureAwait(false);
                    await writer.WritePropertyNameAsync(JsonTypeReflector.ValuePropertyName, false, cancellationToken).ConfigureAwait(false);

                    await JsonWriter.WriteValueAsync(writer, contract.TypeCode, value, cancellationToken).ConfigureAwait(false);

                    await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            await JsonWriter.WriteValueAsync(writer, contract.TypeCode, value, cancellationToken).ConfigureAwait(false);
        }

        private async Task SerializeValueAsync(JsonWriter writer, object? value, JsonContract? valueContract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            MiscellaneousUtils.Assert(valueContract != null);

            JsonConverter? converter =
                member?.Converter ??
                containerProperty?.ItemConverter ??
                containerContract?.ItemConverter ??
                valueContract.Converter ??
                Serializer.GetMatchingConverter(valueContract.UnderlyingType) ??
                valueContract.InternalConverter;

            if (converter != null && converter.CanWrite)
            {
                await SerializeConvertableAsync(writer, converter, value, valueContract, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                return;
            }

            switch (valueContract.ContractType)
            {
                case JsonContractType.Object:
                    await SerializeObjectAsync(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    break;
                case JsonContractType.Array:
                    JsonArrayContract arrayContract = (JsonArrayContract)valueContract;
                    if (!arrayContract.IsMultidimensionalArray)
                    {
                        await SerializeListAsync(writer, (IEnumerable)value, arrayContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await SerializeMultidimensionalArrayAsync(writer, (Array)value, arrayContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                case JsonContractType.Primitive:
                    await SerializePrimitiveAsync(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    break;
                case JsonContractType.String:
                    await SerializeStringAsync(writer, value, (JsonStringContract)valueContract, cancellationToken).ConfigureAwait(false);
                    break;
                case JsonContractType.Dictionary:
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)valueContract;
                    await SerializeDictionaryAsync(writer, (value is IDictionary dictionary) ? dictionary : dictionaryContract.CreateWrapper(value), dictionaryContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    break;
#if HAVE_DYNAMIC
                case JsonContractType.Dynamic:
                    await SerializeDynamicAsync(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    break;
#endif
#if HAVE_BINARY_SERIALIZATION
                case JsonContractType.Serializable:
                    await SerializeISerializableAsync(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty, cancellationToken).ConfigureAwait(false);
                    break;
#endif
                case JsonContractType.Linq:
                    await ((JToken)value).WriteToAsync(writer, cancellationToken, Serializer.Converters.ToArray()).ConfigureAwait(false);
                    break;
            }
        }

        
        private async Task WriteReferenceAsync(JsonWriter writer, object value, CancellationToken cancellationToken)
        {
            string reference = GetReference(writer, value);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference to Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, value.GetType())), null);
            }

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(JsonTypeReflector.RefPropertyName, false, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(reference, cancellationToken).ConfigureAwait(false);
            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SerializeStringAsync(JsonWriter writer, object value, JsonStringContract contract, CancellationToken cancellationToken)
        {
            OnSerializing(writer, contract, value);

            TryConvertToString(value, contract.UnderlyingType, out string? s);
            await writer.WriteValueAsync(s, cancellationToken).ConfigureAwait(false);

            OnSerialized(writer, contract, value);
        }

       
        private async Task SerializeObjectAsync(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            OnSerializing(writer, contract, value);

            _serializeStack.Add(value);

            await WriteObjectStartAsync(writer, value, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            int initialDepth = writer.Top;

            for (int index = 0; index < contract.Properties.Count; index++)
            {
                JsonProperty property = contract.Properties[index];
                try
                {
                    var propertyValues = await CalculatePropertyValuesAsync(writer, value, contract, member, property, cancellationToken).ConfigureAwait(false);
                    if (!propertyValues.Result)
                    {
                        continue;
                    }

                    await property.WritePropertyNameAsync(writer, cancellationToken).ConfigureAwait(false);
                    await SerializeValueAsync(writer, propertyValues.MemberValue, propertyValues.MemberContract, property, contract, member, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                    {
                        await HandleErrorAsync(writer, initialDepth, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            IEnumerable<KeyValuePair<object, object>>? extensionData = contract.ExtensionDataGetter?.Invoke(value);
            if (extensionData != null)
            {
                foreach (KeyValuePair<object, object> e in extensionData)
                {
                    JsonContract keyContract = GetContract(e.Key);
                    JsonContract? valueContract = GetContractSafe(e.Value);

                    string propertyName = GetPropertyName(writer, e.Key, keyContract, out _);

                    propertyName = (contract.ExtensionDataNameResolver != null)
                        ? contract.ExtensionDataNameResolver(propertyName)
                        : propertyName;

                    if (ShouldWriteReference(e.Value, null, valueContract, contract, member))
                    {
                        await writer.WritePropertyNameAsync(propertyName, cancellationToken).ConfigureAwait(false);
                        WriteReference(writer, e.Value!);
                    }
                    else
                    {
                        if (!CheckForCircularReference(writer, e.Value, null, valueContract, contract, member))
                        {
                            continue;
                        }

                        await writer.WritePropertyNameAsync(propertyName, cancellationToken).ConfigureAwait(false);

                        await SerializeValueAsync(writer, e.Value, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, value);
        }

        
        private async Task<PropertyValuesReslt> CalculatePropertyValuesAsync(JsonWriter writer, object value, JsonContainerContract contract, JsonProperty? member, JsonProperty property, CancellationToken cancellationToken)
        {
            PropertyValuesReslt result = new PropertyValuesReslt();
            if (!property.Ignored && property.Readable && ShouldSerialize(writer, property, value) && IsSpecified(writer, property, value))
            {
                if (property.PropertyContract == null)
                {
                    property.PropertyContract = Serializer._contractResolver.ResolveContract(property.PropertyType!);
                }

                result.MemberValue = property.ValueProvider!.GetValue(value);
                result.MemberContract = (property.PropertyContract.IsSealed) ? property.PropertyContract : GetContractSafe(result.MemberValue);

                if (ShouldWriteProperty(result.MemberValue, contract as JsonObjectContract, property))
                {
                    if (ShouldWriteReference(result.MemberValue, property, result.MemberContract, contract, member))
                    {
                        await property.WritePropertyNameAsync(writer, cancellationToken).ConfigureAwait(false);
                        await WriteReferenceAsync(writer, result.MemberValue!, cancellationToken).ConfigureAwait(false);
                        return result;
                    }

                    if (!CheckForCircularReference(writer, result.MemberValue, property, result.MemberContract, contract, member))
                    {
                        return result;
                    }

                    if (result.MemberValue == null)
                    {
                        JsonObjectContract? objectContract = contract as JsonObjectContract;
                        Required resolvedRequired = property._required ?? objectContract?.ItemRequired ?? Required.Default;
                        if (resolvedRequired == Required.Always)
                        {
                            throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
                        }
                        if (resolvedRequired == Required.DisallowNull)
                        {
                            throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
                        }
                    }

                    result.Result = true;
                    return result;
                }
            }

            result.MemberContract = null;
            result.MemberValue = null;
            return result;
        }

        private async Task WriteObjectStartAsync(JsonWriter writer, object value, JsonContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

            bool isReference = ResolveIsReference(contract, member, collectionContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects);
            // don't make readonly fields that aren't creator parameters the referenced value because they can't be deserialized to
            if (isReference && (member == null || member.Writable || HasCreatorParameter(collectionContract, member)))
            {
                await WriteReferenceIdPropertyAsync(writer, contract.UnderlyingType, value, cancellationToken).ConfigureAwait(false);
            }
            if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
            {
                await WriteTypePropertyAsync(writer, contract.UnderlyingType, cancellationToken).ConfigureAwait(false);
            }
        }

 

        private async Task WriteReferenceIdPropertyAsync(JsonWriter writer, Type type, object value, CancellationToken cancellationToken)
        {
            string reference = GetReference(writer, value);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, type)), null);
            }

            await writer.WritePropertyNameAsync(JsonTypeReflector.IdPropertyName, false, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(reference, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteTypePropertyAsync(JsonWriter writer, Type type, CancellationToken cancellationToken)
        {
            string typeName = ReflectionUtils.GetTypeName(type, Serializer._typeNameAssemblyFormatHandling, Serializer._serializationBinder);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing type name '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, typeName, type)), null);
            }

            await writer.WritePropertyNameAsync(JsonTypeReflector.TypePropertyName, false, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(typeName, cancellationToken).ConfigureAwait(false);
        }

        private async Task SerializeConvertableAsync(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            if (ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
            {
                await WriteReferenceAsync(writer, value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (!CheckForCircularReference(writer, value, null, contract, collectionContract, containerProperty))
                {
                    return;
                }

                _serializeStack.Add(value);

                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
                {
                    TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
                }

                await converter.WriteJsonAsync(writer, value, GetInternalSerializer(), cancellationToken).ConfigureAwait(false);

                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
                {
                    TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
                }

                _serializeStack.RemoveAt(_serializeStack.Count - 1);
            }
        }

        private async Task SerializeListAsync(JsonWriter writer, IEnumerable values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            object underlyingList = values is IWrappedCollection wrappedCollection ? wrappedCollection.UnderlyingCollection : values;

            OnSerializing(writer, contract, underlyingList);

            _serializeStack.Add(underlyingList);

            bool hasWrittenMetadataObject = await WriteStartArrayAsync(writer, underlyingList, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

            int initialDepth = writer.Top;

            int index = 0;
            // note that an error in the IEnumerable won't be caught
            foreach (object value in values)
            {
                try
                {
                    JsonContract? valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                    if (ShouldWriteReference(value, null, valueContract, contract, member))
                    {
                        await WriteReferenceAsync(writer, value, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                        {
                            await SerializeValueAsync(writer, value, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(underlyingList, contract, index, null, writer.ContainerPath, ex))
                    {
                        await HandleErrorAsync(writer, initialDepth, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    index++;
                }
            }

            await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);

            if (hasWrittenMetadataObject)
            {
                await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
            }

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, underlyingList);
        }

        private async Task SerializeMultidimensionalArrayAsync(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            OnSerializing(writer, contract, values);

            _serializeStack.Add(values);

            bool hasWrittenMetadataObject = await WriteStartArrayAsync(writer, values, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            await SerializeMultidimensionalArrayAsync(writer, values, contract, member, writer.Top, CollectionUtils.ArrayEmpty<int>(), cancellationToken).ConfigureAwait(false);

            if (hasWrittenMetadataObject)
            {
                writer.WriteEndObject();
            }

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, values);
        }

        private async Task SerializeMultidimensionalArrayAsync(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, int initialDepth, int[] indices, CancellationToken cancellationToken)
        {
            int dimension = indices.Length;
            int[] newIndices = new int[dimension + 1];
            for (int i = 0; i < dimension; i++)
            {
                newIndices[i] = indices[i];
            }

            await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

            for (int i = values.GetLowerBound(dimension); i <= values.GetUpperBound(dimension); i++)
            {
                newIndices[dimension] = i;
                bool isTopLevel = (newIndices.Length == values.Rank);

                if (isTopLevel)
                {
                    object value = values.GetValue(newIndices)!;

                    try
                    {
                        JsonContract? valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                        if (ShouldWriteReference(value, null, valueContract, contract, member))
                        {
                            await WriteReferenceAsync(writer, value, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                            {
                                await SerializeValueAsync(writer, value, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(values, contract, i, null, writer.ContainerPath, ex))
                        {
                            await HandleErrorAsync(writer, initialDepth + 1, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    await SerializeMultidimensionalArrayAsync(writer, values, contract, member, initialDepth + 1, newIndices, cancellationToken).ConfigureAwait(false);
                }
            }

            await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> WriteStartArrayAsync(JsonWriter writer, object values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            bool isReference = ResolveIsReference(contract, member, containerContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays);
            // don't make readonly fields that aren't creator parameters the referenced value because they can't be deserialized to
            isReference = (isReference && (member == null || member.Writable || HasCreatorParameter(containerContract, member)));

            bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
            bool writeMetadataObject = isReference || includeTypeDetails;

            if (writeMetadataObject)
            {
                await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

                if (isReference)
                {
                    await WriteReferenceIdPropertyAsync(writer, contract.UnderlyingType, values, cancellationToken).ConfigureAwait(false);
                }
                if (includeTypeDetails)
                {
                    await WriteTypePropertyAsync(writer, values.GetType(), cancellationToken).ConfigureAwait(false);
                }
                await writer.WritePropertyNameAsync(JsonTypeReflector.ArrayValuesPropertyName, false, cancellationToken).ConfigureAwait(false);
            }

            if (contract.ItemContract == null)
            {
                contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));
            }

            return writeMetadataObject;
        }

#if HAVE_BINARY_SERIALIZATION
#if HAVE_SECURITY_SAFE_CRITICAL_ATTRIBUTE
        [SecuritySafeCritical]
#endif
        private Task SerializeISerializableAsync(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            return SerializeISerializableInternalAsync(writer, value, contract, member, collectionContract, containerProperty, cancellationToken);
        }
        
        private async Task SerializeISerializableInternalAsync(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            if (!JsonTypeReflector.FullyTrusted)
            {
                string message = @"Type '{0}' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                                 @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, value.GetType());

                throw JsonSerializationException.Create(null, writer.ContainerPath, message, null);
            }

            OnSerializing(writer, contract, value);
            _serializeStack.Add(value);

            await WriteObjectStartAsync(writer, value, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
            value.GetObjectData(serializationInfo, Serializer._context);

            foreach (SerializationEntry serializationEntry in serializationInfo)
            {
                JsonContract? valueContract = GetContractSafe(serializationEntry.Value);

                if (ShouldWriteReference(serializationEntry.Value, null, valueContract, contract, member))
                {
                    await writer.WritePropertyNameAsync(serializationEntry.Name, cancellationToken).ConfigureAwait(false);
                    await WriteReferenceAsync(writer, serializationEntry.Value!, cancellationToken).ConfigureAwait(false);
                }
                else if (CheckForCircularReference(writer, serializationEntry.Value, null, valueContract, contract, member))
                {
                    await writer.WritePropertyNameAsync(serializationEntry.Name, cancellationToken).ConfigureAwait(false);
                    await SerializeValueAsync(writer, serializationEntry.Value, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                }
            }

            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);

            _serializeStack.RemoveAt(_serializeStack.Count - 1);
            OnSerialized(writer, contract, value);
        }
#endif

#if HAVE_DYNAMIC
        private async Task SerializeDynamicAsync(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
            OnSerializing(writer, contract, value);
            _serializeStack.Add(value);

            await WriteObjectStartAsync(writer, value, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            int initialDepth = writer.Top;

            for (int index = 0; index < contract.Properties.Count; index++)
            {
                JsonProperty property = contract.Properties[index];

                // only write non-dynamic properties that have an explicit attribute
                if (property.HasMemberAttribute)
                {
                    try
                    {
                        if (!CalculatePropertyValues(writer, value, contract, member, property, out JsonContract? memberContract, out object? memberValue))
                        {
                            continue;
                        }

                        await property.WritePropertyNameAsync(writer, cancellationToken).ConfigureAwait(false);
                        await SerializeValueAsync(writer, memberValue, memberContract, property, contract, member, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                        {
                            await HandleErrorAsync(writer, initialDepth, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            foreach (string memberName in value.GetDynamicMemberNames())
            {
                if (contract.TryGetMember(value, memberName, out object? memberValue))
                {
                    try
                    {
                        JsonContract? valueContract = GetContractSafe(memberValue);

                        if (!ShouldWriteDynamicProperty(memberValue))
                        {
                            continue;
                        }

                        if (CheckForCircularReference(writer, memberValue, null, valueContract, contract, member))
                        {
                            string resolvedPropertyName = (contract.PropertyNameResolver != null)
                                ? contract.PropertyNameResolver(memberName)
                                : memberName;

                            await writer.WritePropertyNameAsync(resolvedPropertyName, cancellationToken).ConfigureAwait(false);
                            await SerializeValueAsync(writer, memberValue, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(value, contract, memberName, null, writer.ContainerPath, ex))
                        {
                            await HandleErrorAsync(writer, initialDepth, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);

            _serializeStack.RemoveAt(_serializeStack.Count - 1);
            OnSerialized(writer, contract, value);
        }
#endif

        private async Task SerializeDictionaryAsync(JsonWriter writer, IDictionary values, JsonDictionaryContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty, CancellationToken cancellationToken)
        {
#pragma warning disable CS8600, CS8602, CS8604
            object underlyingDictionary = values is IWrappedDictionary wrappedDictionary ? wrappedDictionary.UnderlyingDictionary : values;

            OnSerializing(writer, contract, underlyingDictionary);
            _serializeStack.Add(underlyingDictionary);

            await WriteObjectStartAsync(writer, underlyingDictionary, contract, member, collectionContract, containerProperty, cancellationToken).ConfigureAwait(false);

            if (contract.ItemContract == null)
            {
                contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));
            }

            if (contract.KeyContract == null)
            {
                contract.KeyContract = Serializer._contractResolver.ResolveContract(contract.DictionaryKeyType ?? typeof(object));
            }

            int initialDepth = writer.Top;

            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    DictionaryEntry entry = e.Entry;

                    string propertyName = GetPropertyName(writer, entry.Key, contract.KeyContract, out bool escape);

                    propertyName = (contract.DictionaryKeyResolver != null)
                        ? contract.DictionaryKeyResolver(propertyName)
                        : propertyName;

                    try
                    {
                        object value = entry.Value;
                        JsonContract? valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                        if (ShouldWriteReference(value, null, valueContract, contract, member))
                        {
                            await writer.WritePropertyNameAsync(propertyName, escape, cancellationToken).ConfigureAwait(false);
                            await WriteReferenceAsync(writer, value, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            if (!CheckForCircularReference(writer, value, null, valueContract, contract, member))
                            {
                                continue;
                            }

                            await writer.WritePropertyNameAsync(propertyName, escape, cancellationToken).ConfigureAwait(false);

                            await SerializeValueAsync(writer, value, valueContract, null, contract, member, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(underlyingDictionary, contract, propertyName, null, writer.ContainerPath, ex))
                        {
                            await HandleErrorAsync(writer, initialDepth, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                (e as IDisposable)?.Dispose();
            }

            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, underlyingDictionary);
#pragma warning restore CS8600, CS8602, CS8604
        }

        
        private async Task HandleErrorAsync(JsonWriter writer, int initialDepth, CancellationToken cancellationToken)
        {
            ClearErrorContext();

            if (writer.WriteState == WriteState.Property)
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
            }

            while (writer.Top > initialDepth)
            {
                await writer.WriteEndAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private class PropertyValuesReslt
        {
            public bool Result;
            public JsonContract? MemberContract;
            public object? MemberValue;
        }

    }
}
#endif
