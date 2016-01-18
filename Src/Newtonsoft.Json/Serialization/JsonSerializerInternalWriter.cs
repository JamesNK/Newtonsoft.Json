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
using System.ComponentModel;
#if !(NET35 || NET20 || PORTABLE40)
using System.Dynamic;
#endif
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Serialization
{
    internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
    {
        private Type _rootType;
        private int _rootLevel;
        private readonly List<object> _serializeStack = new List<object>();

        public JsonSerializerInternalWriter(JsonSerializer serializer)
            : base(serializer)
        {
        }

        public void Serialize(JsonWriter jsonWriter, object value, Type objectType)
        {
            if (jsonWriter == null)
            {
                throw new ArgumentNullException(nameof(jsonWriter));
            }

            _rootType = objectType;
            _rootLevel = _serializeStack.Count + 1;

            JsonContract contract = GetContractSafe(value);

            try
            {
                if (ShouldWriteReference(value, null, contract, null, null))
                {
                    WriteReference(jsonWriter, value);
                }
                else
                {
                    SerializeValue(jsonWriter, value, contract, null, null, null);
                }
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(null, contract, null, null, jsonWriter.Path, ex))
                {
                    HandleError(jsonWriter, 0);
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
                // accidently be used for non root values
                _rootType = null;
            }
        }

        private JsonSerializerProxy GetInternalSerializer()
        {
            if (InternalSerializer == null)
            {
                InternalSerializer = new JsonSerializerProxy(this);
            }

            return InternalSerializer;
        }

        private JsonContract GetContractSafe(object value)
        {
            if (value == null)
            {
                return null;
            }

            return Serializer._contractResolver.ResolveContract(value.GetType());
        }

        private void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            if (contract.TypeCode == PrimitiveTypeCode.Bytes)
            {
                // if type name handling is enabled then wrap the base64 byte string in an object with the type name
                bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty);
                if (includeTypeDetails)
                {
                    writer.WriteStartObject();
                    WriteTypeProperty(writer, contract.CreatedType);
                    writer.WritePropertyName(JsonTypeReflector.ValuePropertyName, false);

                    JsonWriter.WriteValue(writer, contract.TypeCode, value);

                    writer.WriteEndObject();
                    return;
                }
            }

            JsonWriter.WriteValue(writer, contract.TypeCode, value);
        }

        private void SerializeValue(JsonWriter writer, object value, JsonContract valueContract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            JsonConverter converter =
                ((member != null) ? member.Converter : null) ??
                ((containerProperty != null) ? containerProperty.ItemConverter : null) ??
                ((containerContract != null) ? containerContract.ItemConverter : null) ??
                valueContract.Converter ??
                Serializer.GetMatchingConverter(valueContract.UnderlyingType) ??
                valueContract.InternalConverter;

            if (converter != null && converter.CanWrite)
            {
                SerializeConvertable(writer, converter, value, valueContract, containerContract, containerProperty);
                return;
            }

            switch (valueContract.ContractType)
            {
                case JsonContractType.Object:
                    SerializeObject(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty);
                    break;
                case JsonContractType.Array:
                    JsonArrayContract arrayContract = (JsonArrayContract)valueContract;
                    if (!arrayContract.IsMultidimensionalArray)
                    {
                        SerializeList(writer, (IEnumerable)value, arrayContract, member, containerContract, containerProperty);
                    }
                    else
                    {
                        SerializeMultidimensionalArray(writer, (Array)value, arrayContract, member, containerContract, containerProperty);
                    }
                    break;
                case JsonContractType.Primitive:
                    SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty);
                    break;
                case JsonContractType.String:
                    SerializeString(writer, value, (JsonStringContract)valueContract);
                    break;
                case JsonContractType.Dictionary:
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)valueContract;
                    SerializeDictionary(writer, (value is IDictionary) ? (IDictionary)value : dictionaryContract.CreateWrapper(value), dictionaryContract, member, containerContract, containerProperty);
                    break;
#if !(NET35 || NET20 || PORTABLE40)
                case JsonContractType.Dynamic:
                    SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty);
                    break;
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
                case JsonContractType.Serializable:
                    SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty);
                    break;
#endif
                case JsonContractType.Linq:
                    ((JToken)value).WriteTo(writer, Serializer.Converters.ToArray());
                    break;
            }
        }

        private bool? ResolveIsReference(JsonContract contract, JsonProperty property, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            bool? isReference = null;

            // value could be coming from a dictionary or array and not have a property
            if (property != null)
            {
                isReference = property.IsReference;
            }

            if (isReference == null && containerProperty != null)
            {
                isReference = containerProperty.ItemIsReference;
            }

            if (isReference == null && collectionContract != null)
            {
                isReference = collectionContract.ItemIsReference;
            }

            if (isReference == null)
            {
                isReference = contract.IsReference;
            }

            return isReference;
        }

        private bool ShouldWriteReference(object value, JsonProperty property, JsonContract valueContract, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            if (value == null)
            {
                return false;
            }
            if (valueContract.ContractType == JsonContractType.Primitive || valueContract.ContractType == JsonContractType.String)
            {
                return false;
            }

            bool? isReference = ResolveIsReference(valueContract, property, collectionContract, containerProperty);

            if (isReference == null)
            {
                if (valueContract.ContractType == JsonContractType.Array)
                {
                    isReference = HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays);
                }
                else
                {
                    isReference = HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects);
                }
            }

            if (!isReference.GetValueOrDefault())
            {
                return false;
            }

            return Serializer.GetReferenceResolver().IsReferenced(this, value);
        }

        private bool ShouldWriteProperty(object memberValue, JsonProperty property)
        {
            if (property.NullValueHandling.GetValueOrDefault(Serializer._nullValueHandling) == NullValueHandling.Ignore &&
                memberValue == null)
            {
                return false;
            }

            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore)
                && MiscellaneousUtils.ValueEquals(memberValue, property.GetResolvedDefaultValue()))
            {
                return false;
            }

            return true;
        }

        private bool CheckForCircularReference(JsonWriter writer, object value, JsonProperty property, JsonContract contract, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            if (value == null || contract.ContractType == JsonContractType.Primitive || contract.ContractType == JsonContractType.String)
            {
                return true;
            }

            ReferenceLoopHandling? referenceLoopHandling = null;

            if (property != null)
            {
                referenceLoopHandling = property.ReferenceLoopHandling;
            }

            if (referenceLoopHandling == null && containerProperty != null)
            {
                referenceLoopHandling = containerProperty.ItemReferenceLoopHandling;
            }

            if (referenceLoopHandling == null && containerContract != null)
            {
                referenceLoopHandling = containerContract.ItemReferenceLoopHandling;
            }

            bool exists = (Serializer._equalityComparer != null)
                ? _serializeStack.Contains(value, Serializer._equalityComparer)
                : _serializeStack.Contains(value);

            if (exists)
            {
                string message = "Self referencing loop detected";
                if (property != null)
                {
                    message += " for property '{0}'".FormatWith(CultureInfo.InvariantCulture, property.PropertyName);
                }
                message += " with type '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType());

                switch (referenceLoopHandling.GetValueOrDefault(Serializer._referenceLoopHandling))
                {
                    case ReferenceLoopHandling.Error:
                        throw JsonSerializationException.Create(null, writer.ContainerPath, message, null);
                    case ReferenceLoopHandling.Ignore:
                        if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                        {
                            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, message + ". Skipping serializing self referenced value."), null);
                        }

                        return false;
                    case ReferenceLoopHandling.Serialize:
                        if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                        {
                            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, message + ". Serializing self referenced value."), null);
                        }

                        return true;
                }
            }

            return true;
        }

        private void WriteReference(JsonWriter writer, object value)
        {
            string reference = GetReference(writer, value);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference to Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, value.GetType())), null);
            }

            writer.WriteStartObject();
            writer.WritePropertyName(JsonTypeReflector.RefPropertyName, false);
            writer.WriteValue(reference);
            writer.WriteEndObject();
        }

        private string GetReference(JsonWriter writer, object value)
        {
            try
            {
                string reference = Serializer.GetReferenceResolver().GetReference(this, value);

                return reference;
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(null, writer.ContainerPath, "Error writing object reference for '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), ex);
            }
        }

        internal static bool TryConvertToString(object value, Type type, out string s)
        {
#if !(DOTNET || PORTABLE40 || PORTABLE)
            TypeConverter converter = ConvertUtils.GetConverter(type);

            // use the objectType's TypeConverter if it has one and can convert to a string
            if (converter != null
                && !(converter is ComponentConverter)
                && converter.GetType() != typeof(TypeConverter))
            {
                if (converter.CanConvertTo(typeof(string)))
                {
                    s = converter.ConvertToInvariantString(value);
                    return true;
                }
            }
#endif

#if (DOTNET || PORTABLE)
            if (value is Guid || value is Uri || value is TimeSpan)
            {
                s = value.ToString();
                return true;
            }
#endif

            if (value is Type)
            {
                s = ((Type)value).AssemblyQualifiedName;
                return true;
            }

            s = null;
            return false;
        }

        private void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
        {
            OnSerializing(writer, contract, value);

            string s;
            TryConvertToString(value, contract.UnderlyingType, out s);
            writer.WriteValue(s);

            OnSerialized(writer, contract, value);
        }

        private void OnSerializing(JsonWriter writer, JsonContract contract, object value)
        {
            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnSerializing(value, Serializer._context);
        }

        private void OnSerialized(JsonWriter writer, JsonContract contract, object value)
        {
            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnSerialized(value, Serializer._context);
        }

        private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            OnSerializing(writer, contract, value);

            _serializeStack.Add(value);

            WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

            int initialDepth = writer.Top;

            for (int index = 0; index < contract.Properties.Count; index++)
            {
                JsonProperty property = contract.Properties[index];
                try
                {
                    object memberValue;
                    JsonContract memberContract;

                    if (!CalculatePropertyValues(writer, value, contract, member, property, out memberContract, out memberValue))
                    {
                        continue;
                    }

                    property.WritePropertyName(writer);
                    SerializeValue(writer, memberValue, memberContract, property, contract, member);
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (contract.ExtensionDataGetter != null)
            {
                IEnumerable<KeyValuePair<object, object>> extensionData = contract.ExtensionDataGetter(value);
                if (extensionData != null)
                {
                    foreach (KeyValuePair<object, object> e in extensionData)
                    {
                        JsonContract keyContract = GetContractSafe(e.Key);
                        JsonContract valueContract = GetContractSafe(e.Value);

                        bool escape;
                        string propertyName = GetPropertyName(writer, e.Key, keyContract, out escape);

                        if (ShouldWriteReference(e.Value, null, valueContract, contract, member))
                        {
                            writer.WritePropertyName(propertyName);
                            WriteReference(writer, e.Value);
                        }
                        else
                        {
                            if (!CheckForCircularReference(writer, e.Value, null, valueContract, contract, member))
                            {
                                continue;
                            }

                            writer.WritePropertyName(propertyName);

                            SerializeValue(writer, e.Value, valueContract, null, contract, member);
                        }
                    }
                }
            }

            writer.WriteEndObject();

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, value);
        }

        private bool CalculatePropertyValues(JsonWriter writer, object value, JsonContainerContract contract, JsonProperty member, JsonProperty property, out JsonContract memberContract, out object memberValue)
        {
            if (!property.Ignored && property.Readable && ShouldSerialize(writer, property, value) && IsSpecified(writer, property, value))
            {
                if (property.PropertyContract == null)
                {
                    property.PropertyContract = Serializer._contractResolver.ResolveContract(property.PropertyType);
                }

                memberValue = property.ValueProvider.GetValue(value);
                memberContract = (property.PropertyContract.IsSealed) ? property.PropertyContract : GetContractSafe(memberValue);

                if (ShouldWriteProperty(memberValue, property))
                {
                    if (ShouldWriteReference(memberValue, property, memberContract, contract, member))
                    {
                        property.WritePropertyName(writer);
                        WriteReference(writer, memberValue);
                        return false;
                    }

                    if (!CheckForCircularReference(writer, memberValue, property, memberContract, contract, member))
                    {
                        return false;
                    }

                    if (memberValue == null)
                    {
                        JsonObjectContract objectContract = contract as JsonObjectContract;
                        Required resolvedRequired = property._required ?? ((objectContract != null) ? objectContract.ItemRequired : null) ?? Required.Default;
                        if (resolvedRequired == Required.Always)
                        {
                            throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
                        }
                        if (resolvedRequired == Required.DisallowNull)
                        {
                            throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
                        }
                    }

                    return true;
                }
            }

            memberContract = null;
            memberValue = null;
            return false;
        }

        private void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            writer.WriteStartObject();

            bool isReference = ResolveIsReference(contract, member, collectionContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects);
            // don't make readonly fields the referenced value because they can't be deserialized to
            if (isReference && (member == null || member.Writable))
            {
                WriteReferenceIdProperty(writer, contract.UnderlyingType, value);
            }
            if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
            {
                WriteTypeProperty(writer, contract.UnderlyingType);
            }
        }

        private void WriteReferenceIdProperty(JsonWriter writer, Type type, object value)
        {
            string reference = GetReference(writer, value);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, type)), null);
            }

            writer.WritePropertyName(JsonTypeReflector.IdPropertyName, false);
            writer.WriteValue(reference);
        }

        private void WriteTypeProperty(JsonWriter writer, Type type)
        {
            string typeName = ReflectionUtils.GetTypeName(type, Serializer._typeNameAssemblyFormat, Serializer._binder);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing type name '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, typeName, type)), null);
            }

            writer.WritePropertyName(JsonTypeReflector.TypePropertyName, false);
            writer.WriteValue(typeName);
        }

        private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
        {
            return ((value & flag) == flag);
        }

        private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
        {
            return ((value & flag) == flag);
        }

        private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
        {
            return ((value & flag) == flag);
        }

        private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            if (ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
            {
                WriteReference(writer, value);
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

                converter.WriteJson(writer, value, GetInternalSerializer());

                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
                {
                    TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
                }

                _serializeStack.RemoveAt(_serializeStack.Count - 1);
            }
        }

        private void SerializeList(JsonWriter writer, IEnumerable values, JsonArrayContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            IWrappedCollection wrappedCollection = values as IWrappedCollection;
            object underlyingList = wrappedCollection != null ? wrappedCollection.UnderlyingCollection : values;

            OnSerializing(writer, contract, underlyingList);

            _serializeStack.Add(underlyingList);

            bool hasWrittenMetadataObject = WriteStartArray(writer, underlyingList, contract, member, collectionContract, containerProperty);

            writer.WriteStartArray();

            int initialDepth = writer.Top;

            int index = 0;
            // note that an error in the IEnumerable won't be caught
            foreach (object value in values)
            {
                try
                {
                    JsonContract valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                    if (ShouldWriteReference(value, null, valueContract, contract, member))
                    {
                        WriteReference(writer, value);
                    }
                    else
                    {
                        if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                        {
                            SerializeValue(writer, value, valueContract, null, contract, member);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(underlyingList, contract, index, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
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

            writer.WriteEndArray();

            if (hasWrittenMetadataObject)
            {
                writer.WriteEndObject();
            }

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, underlyingList);
        }

        private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            OnSerializing(writer, contract, values);

            _serializeStack.Add(values);

            bool hasWrittenMetadataObject = WriteStartArray(writer, values, contract, member, collectionContract, containerProperty);

            SerializeMultidimensionalArray(writer, values, contract, member, writer.Top, new int[0]);

            if (hasWrittenMetadataObject)
            {
                writer.WriteEndObject();
            }

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, values);
        }

        private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty member, int initialDepth, int[] indices)
        {
            int dimension = indices.Length;
            int[] newIndices = new int[dimension + 1];
            for (int i = 0; i < dimension; i++)
            {
                newIndices[i] = indices[i];
            }

            writer.WriteStartArray();

            for (int i = values.GetLowerBound(dimension); i <= values.GetUpperBound(dimension); i++)
            {
                newIndices[dimension] = i;
                bool isTopLevel = (newIndices.Length == values.Rank);

                if (isTopLevel)
                {
                    object value = values.GetValue(newIndices);

                    try
                    {
                        JsonContract valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                        if (ShouldWriteReference(value, null, valueContract, contract, member))
                        {
                            WriteReference(writer, value);
                        }
                        else
                        {
                            if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                            {
                                SerializeValue(writer, value, valueContract, null, contract, member);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(values, contract, i, null, writer.ContainerPath, ex))
                        {
                            HandleError(writer, initialDepth + 1);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    SerializeMultidimensionalArray(writer, values, contract, member, initialDepth + 1, newIndices);
                }
            }

            writer.WriteEndArray();
        }

        private bool WriteStartArray(JsonWriter writer, object values, JsonArrayContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            bool isReference = ResolveIsReference(contract, member, containerContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays);
            // don't make readonly fields the referenced value because they can't be deserialized to
            isReference = (isReference && (member == null || member.Writable));

            bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
            bool writeMetadataObject = isReference || includeTypeDetails;

            if (writeMetadataObject)
            {
                writer.WriteStartObject();

                if (isReference)
                {
                    WriteReferenceIdProperty(writer, contract.UnderlyingType, values);
                }
                if (includeTypeDetails)
                {
                    WriteTypeProperty(writer, values.GetType());
                }
                writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName, false);
            }

            if (contract.ItemContract == null)
            {
                contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));
            }

            return writeMetadataObject;
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
#if !(NET20 || NET35)
        [SecuritySafeCritical]
#endif
        private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
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

            WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

            SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
            value.GetObjectData(serializationInfo, Serializer._context);

            foreach (SerializationEntry serializationEntry in serializationInfo)
            {
                JsonContract valueContract = GetContractSafe(serializationEntry.Value);

                if (ShouldWriteReference(serializationEntry.Value, null, valueContract, contract, member))
                {
                    writer.WritePropertyName(serializationEntry.Name);
                    WriteReference(writer, serializationEntry.Value);
                }
                else if (CheckForCircularReference(writer, serializationEntry.Value, null, valueContract, contract, member))
                {
                    writer.WritePropertyName(serializationEntry.Name);
                    SerializeValue(writer, serializationEntry.Value, valueContract, null, contract, member);
                }
            }

            writer.WriteEndObject();

            _serializeStack.RemoveAt(_serializeStack.Count - 1);
            OnSerialized(writer, contract, value);
        }
#endif

#if !(NET35 || NET20 || PORTABLE40)
        private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            OnSerializing(writer, contract, value);
            _serializeStack.Add(value);

            WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

            int initialDepth = writer.Top;

            for (int index = 0; index < contract.Properties.Count; index++)
            {
                JsonProperty property = contract.Properties[index];

                // only write non-dynamic properties that have an explicit attribute
                if (property.HasMemberAttribute)
                {
                    try
                    {
                        object memberValue;
                        JsonContract memberContract;

                        if (!CalculatePropertyValues(writer, value, contract, member, property, out memberContract, out memberValue))
                        {
                            continue;
                        }

                        property.WritePropertyName(writer);
                        SerializeValue(writer, memberValue, memberContract, property, contract, member);
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                        {
                            HandleError(writer, initialDepth);
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
                object memberValue;
                if (contract.TryGetMember(value, memberName, out memberValue))
                {
                    try
                    {
                        JsonContract valueContract = GetContractSafe(memberValue);

                        if (!ShouldWriteDynamicProperty(memberValue))
                        {
                            continue;
                        }

                        if (CheckForCircularReference(writer, memberValue, null, valueContract, contract, member))
                        {
                            string resolvedPropertyName = (contract.PropertyNameResolver != null)
                                ? contract.PropertyNameResolver(memberName)
                                : memberName;

                            writer.WritePropertyName(resolvedPropertyName);
                            SerializeValue(writer, memberValue, valueContract, null, contract, member);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(value, contract, memberName, null, writer.ContainerPath, ex))
                        {
                            HandleError(writer, initialDepth);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            writer.WriteEndObject();

            _serializeStack.RemoveAt(_serializeStack.Count - 1);
            OnSerialized(writer, contract, value);
        }
#endif

        private bool ShouldWriteDynamicProperty(object memberValue)
        {
            if (Serializer._nullValueHandling == NullValueHandling.Ignore && memberValue == null)
            {
                return false;
            }

            if (HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Ignore) &&
                (memberValue == null || MiscellaneousUtils.ValueEquals(memberValue, ReflectionUtils.GetDefaultValue(memberValue.GetType()))))
            {
                return false;
            }

            return true;
        }

        private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            TypeNameHandling resolvedTypeNameHandling =
                ((member != null) ? member.TypeNameHandling : null)
                ?? ((containerProperty != null) ? containerProperty.ItemTypeNameHandling : null)
                ?? ((containerContract != null) ? containerContract.ItemTypeNameHandling : null)
                ?? Serializer._typeNameHandling;

            if (HasFlag(resolvedTypeNameHandling, typeNameHandlingFlag))
            {
                return true;
            }

            // instance type and the property's type's contract default type are different (no need to put the type in JSON because the type will be created by default)
            if (HasFlag(resolvedTypeNameHandling, TypeNameHandling.Auto))
            {
                if (member != null)
                {
                    if (contract.UnderlyingType != member.PropertyContract.CreatedType)
                    {
                        return true;
                    }
                }
                else if (containerContract != null)
                {
                    if (containerContract.ItemContract == null || contract.UnderlyingType != containerContract.ItemContract.CreatedType)
                    {
                        return true;
                    }
                }
                else if (_rootType != null && _serializeStack.Count == _rootLevel)
                {
                    JsonContract rootContract = Serializer._contractResolver.ResolveContract(_rootType);

                    if (contract.UnderlyingType != rootContract.CreatedType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
        {
            IWrappedDictionary wrappedDictionary = values as IWrappedDictionary;
            object underlyingDictionary = wrappedDictionary != null ? wrappedDictionary.UnderlyingDictionary : values;

            OnSerializing(writer, contract, underlyingDictionary);
            _serializeStack.Add(underlyingDictionary);

            WriteObjectStart(writer, underlyingDictionary, contract, member, collectionContract, containerProperty);

            if (contract.ItemContract == null)
            {
                contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));
            }

            if (contract.KeyContract == null)
            {
                contract.KeyContract = Serializer._contractResolver.ResolveContract(contract.DictionaryKeyType ?? typeof(object));
            }

            int initialDepth = writer.Top;

            foreach (DictionaryEntry entry in values)
            {
                bool escape;
                string propertyName = GetPropertyName(writer, entry.Key, contract.KeyContract, out escape);

                propertyName = (contract.DictionaryKeyResolver != null)
                    ? contract.DictionaryKeyResolver(propertyName)
                    : propertyName;

                try
                {
                    object value = entry.Value;
                    JsonContract valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                    if (ShouldWriteReference(value, null, valueContract, contract, member))
                    {
                        writer.WritePropertyName(propertyName, escape);
                        WriteReference(writer, value);
                    }
                    else
                    {
                        if (!CheckForCircularReference(writer, value, null, valueContract, contract, member))
                        {
                            continue;
                        }

                        writer.WritePropertyName(propertyName, escape);

                        SerializeValue(writer, value, valueContract, null, contract, member);
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(underlyingDictionary, contract, propertyName, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            writer.WriteEndObject();

            _serializeStack.RemoveAt(_serializeStack.Count - 1);

            OnSerialized(writer, contract, underlyingDictionary);
        }

        private string GetPropertyName(JsonWriter writer, object name, JsonContract contract, out bool escape)
        {
            string propertyName;

            if (contract.ContractType == JsonContractType.Primitive)
            {
                JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;
                if (primitiveContract.TypeCode == PrimitiveTypeCode.DateTime || primitiveContract.TypeCode == PrimitiveTypeCode.DateTimeNullable)
                {
                    DateTime dt = DateTimeUtils.EnsureDateTime((DateTime)name, writer.DateTimeZoneHandling);

                    escape = false;
                    StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                    DateTimeUtils.WriteDateTimeString(sw, dt, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
                    return sw.ToString();
                }
#if !NET20
                else if (primitiveContract.TypeCode == PrimitiveTypeCode.DateTimeOffset || primitiveContract.TypeCode == PrimitiveTypeCode.DateTimeOffsetNullable)
                {
                    escape = false;
                    StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                    DateTimeUtils.WriteDateTimeOffsetString(sw, (DateTimeOffset)name, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
                    return sw.ToString();
                }
#endif
                else
                {
                    escape = true;
                    return Convert.ToString(name, CultureInfo.InvariantCulture);
                }
            }
            else if (TryConvertToString(name, name.GetType(), out propertyName))
            {
                escape = true;
                return propertyName;
            }
            else
            {
                escape = true;
                return name.ToString();
            }
        }

        private void HandleError(JsonWriter writer, int initialDepth)
        {
            ClearErrorContext();

            if (writer.WriteState == WriteState.Property)
            {
                writer.WriteNull();
            }

            while (writer.Top > initialDepth)
            {
                writer.WriteEnd();
            }
        }

        private bool ShouldSerialize(JsonWriter writer, JsonProperty property, object target)
        {
            if (property.ShouldSerialize == null)
            {
                return true;
            }

            bool shouldSerialize = property.ShouldSerialize(target);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "ShouldSerialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, shouldSerialize)), null);
            }

            return shouldSerialize;
        }

        private bool IsSpecified(JsonWriter writer, JsonProperty property, object target)
        {
            if (property.GetIsSpecified == null)
            {
                return true;
            }

            bool isSpecified = property.GetIsSpecified(target);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "IsSpecified result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, isSpecified)), null);
            }

            return isSpecified;
        }
    }
}