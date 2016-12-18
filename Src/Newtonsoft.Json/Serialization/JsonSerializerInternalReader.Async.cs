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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    internal partial class JsonSerializerInternalReader
    {
        internal async Task ReadAndAssertAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                throw JsonSerializationException.Create(reader, "Unexpected end when reading JSON.");
            }
        }

        public async Task PopulateAsync(JsonReader reader, object target, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            Type objectType = target.GetType();

            JsonContract contract = Serializer._contractResolver.ResolveContract(objectType);

            if (!await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false))
            {
                throw JsonSerializationException.Create(reader, "No JSON content found.");
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                if (contract.ContractType == JsonContractType.Array)
                {
                    JsonArrayContract arrayContract = (JsonArrayContract)contract;

                    await PopulateListAsync(arrayContract.ShouldCreateWrapper ? arrayContract.CreateWrapper(target) : (IList)target, reader, arrayContract, null, null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);

                string id = null;
                if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore && reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                {
                    await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);
                    id = reader.Value != null ? reader.Value.ToString() : null;
                    await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);
                }

                if (contract.ContractType == JsonContractType.Dictionary)
                {
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                    await PopulateDictionaryAsync(dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(target) : (IDictionary)target, reader, dictionaryContract, null, id, cancellationToken).ConfigureAwait(false);
                }
                else if (contract.ContractType == JsonContractType.Object)
                {
                    await PopulateObjectAsync(target, reader, (JsonObjectContract)contract, null, id, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private async Task<object> PopulateListAsync(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IWrappedCollection wrappedCollection = list as IWrappedCollection;
            object underlyingList = wrappedCollection != null ? wrappedCollection.UnderlyingCollection : list;

            if (id != null)
            {
                AddReference(reader, id, underlyingList);
            }

            // can't populate an existing array
            if (list.IsFixedSize)
            {
                await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                return underlyingList;
            }

            OnDeserializing(reader, contract, underlyingList);

            int initialDepth = reader.Depth;

            if (contract.ItemContract == null)
            {
                contract.ItemContract = GetContractSafe(contract.CollectionItemType);
            }

            JsonConverter collectionItemConverter = GetConverter(contract.ItemContract, null, contract, containerProperty);

            int? previousErrorIndex = null;

            bool finished = false;
            do
            {
                try
                {
                    if (await ReadForTypeAsync(reader, contract.ItemContract, collectionItemConverter != null, cancellationToken).ConfigureAwait(false))
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndArray:
                                finished = true;
                                break;
                            default:
                                object value;

                                if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                {
                                    value = await DeserializeConvertableAsync(collectionItemConverter, reader, contract.CollectionItemType, null, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    value = await CreateValueInternalAsync(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null, cancellationToken).ConfigureAwait(false);
                                }

                                list.Add(value);
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    JsonPosition errorPosition = reader.GetPosition(initialDepth);

                    if (IsErrorHandled(underlyingList, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);

                        if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                        {
                            // reader index has not moved since previous error handling
                            // break out of reading array to prevent infinite loop
                            throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                        }
                        else
                        {
                            previousErrorIndex = errorPosition.Position;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, underlyingList, "Unexpected end when deserializing array.");
            }

            OnDeserialized(reader, contract, underlyingList);
            return underlyingList;
        }

        private async Task<object> PopulateDictionaryAsync(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty containerProperty, string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IWrappedDictionary wrappedDictionary = dictionary as IWrappedDictionary;
            object underlyingDictionary = wrappedDictionary != null ? wrappedDictionary.UnderlyingDictionary : dictionary;

            if (id != null)
            {
                AddReference(reader, id, underlyingDictionary);
            }

            OnDeserializing(reader, contract, underlyingDictionary);

            int initialDepth = reader.Depth;

            if (contract.KeyContract == null)
            {
                contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);
            }

            if (contract.ItemContract == null)
            {
                contract.ItemContract = GetContractSafe(contract.DictionaryValueType);
            }

            JsonConverter dictionaryValueConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);
            PrimitiveTypeCode keyTypeCode = contract.KeyContract is JsonPrimitiveContract ? ((JsonPrimitiveContract)contract.KeyContract).TypeCode : PrimitiveTypeCode.Empty;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        object keyValue = reader.Value;
                        if (await CheckPropertyNameAsync(reader, keyValue.ToString(), cancellationToken).ConfigureAwait(false))
                        {
                            continue;
                        }

                        try
                        {
                            try
                            {
                                // this is for correctly reading ISO and MS formatted dictionary keys
                                switch (keyTypeCode)
                                {
                                    case PrimitiveTypeCode.DateTime:
                                    case PrimitiveTypeCode.DateTimeNullable:
                                    {
                                        DateTime dt;
                                        if (DateTimeUtils.TryParseDateTime(keyValue.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out dt))
                                        {
                                            keyValue = dt;
                                        }
                                        else
                                        {
                                            keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        }
                                        break;
                                    }
#if !NET20
                                    case PrimitiveTypeCode.DateTimeOffset:
                                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                                    {
                                        DateTimeOffset dt;
                                        if (DateTimeUtils.TryParseDateTimeOffset(keyValue.ToString(), reader.DateFormatString, reader.Culture, out dt))
                                        {
                                            keyValue = dt;
                                        }
                                        else
                                        {
                                            keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        }
                                        break;
                                    }
#endif
                                    default:
                                        keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
                            }

                            if (!await ReadForTypeAsync(reader, contract.ItemContract, dictionaryValueConverter != null, cancellationToken).ConfigureAwait(false))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
                            }

                            object itemValue;
                            if (dictionaryValueConverter != null && dictionaryValueConverter.CanRead)
                            {
                                itemValue = await DeserializeConvertableAsync(dictionaryValueConverter, reader, contract.DictionaryValueType, null, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                itemValue = await CreateValueInternalAsync(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null, cancellationToken).ConfigureAwait(false);
                            }

                            dictionary[keyValue] = itemValue;
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(underlyingDictionary, contract, keyValue, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, underlyingDictionary, "Unexpected end when deserializing object.");
            }

            OnDeserialized(reader, contract, underlyingDictionary);
            return underlyingDictionary;
        }

        private async Task<object> PopulateObjectAsync(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty member, string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OnDeserializing(reader, contract, newObject);

            // only need to keep a track of properies presence if they are required or a value should be defaulted if missing
            Dictionary<JsonProperty, PropertyPresence> propertiesPresence = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate) ? contract.Properties.ToDictionary(m => m, m => PropertyPresence.None) : null;

            if (id != null)
            {
                AddReference(reader, id, newObject);
            }

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                    {
                        string memberName = reader.Value.ToString();

                        if (await CheckPropertyNameAsync(reader, memberName, cancellationToken).ConfigureAwait(false))
                        {
                            continue;
                        }

                        try
                        {
                            // attempt exact case match first
                            // then try match ignoring case
                            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                            if (property == null)
                            {
                                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                                {
                                    TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                                }

                                if (Serializer._missingMemberHandling == MissingMemberHandling.Error)
                                {
                                    throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));
                                }

                                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    break;
                                }

                                await SetExtensionDataAsync(contract, member, reader, memberName, newObject, cancellationToken).ConfigureAwait(false);
                                continue;
                            }

                            if (property.Ignored || !ShouldDeserialize(reader, property, newObject))
                            {
                                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    break;
                                }

                                SetPropertyPresence(reader, property, propertiesPresence);
                                await SetExtensionDataAsync(contract, member, reader, memberName, newObject, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, member);

                                if (!await ReadForTypeAsync(reader, property.PropertyContract, propertyConverter != null, cancellationToken).ConfigureAwait(false))
                                {
                                    throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                                }

                                SetPropertyPresence(reader, property, propertiesPresence);

                                // set extension data if property is ignored or readonly
                                if (!await SetPropertyValueAsync(property, propertyConverter, contract, member, reader, newObject, cancellationToken).ConfigureAwait(false))
                                {
                                    await SetExtensionDataAsync(contract, member, reader, memberName, newObject, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        break;
                    }
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    case JsonToken.Comment:

                        // ignore
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            if (propertiesPresence != null)
            {
                foreach (KeyValuePair<JsonProperty, PropertyPresence> propertyPresence in propertiesPresence)
                {
                    JsonProperty property = propertyPresence.Key;
                    PropertyPresence presence = propertyPresence.Value;

                    await EndProcessPropertyAsync(newObject, reader, contract, initialDepth, property, presence, true, cancellationToken).ConfigureAwait(false);
                }
            }

            OnDeserialized(reader, contract, newObject);
            return newObject;
        }

        private static async Task<bool> ReadForTypeAsync(JsonReader reader, JsonContract contract, bool hasConverter, CancellationToken cancellationToken)
        {
            // don't read properties with converters as a specific value
            // the value might be a string which will then get converted which will error if read as date for example
            if (hasConverter)
            {
                return await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }

            ReadType t = contract != null ? contract.InternalReadType : ReadType.Read;

            switch (t)
            {
                case ReadType.Read:
                    return await reader.ReadAndMoveToContentAsync(cancellationToken).ConfigureAwait(false);
                case ReadType.ReadAsInt32:
                    await reader.ReadAsInt32Async(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsDecimal:
                    await reader.ReadAsDecimalAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsDouble:
                    await reader.ReadAsDoubleAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsBytes:
                    await reader.ReadAsBytesAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsBoolean:
                    await reader.ReadAsBooleanAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsString:
                    await reader.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsDateTime:
                    await reader.ReadAsDateTimeAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case ReadType.ReadAsDateTimeOffset:
                    await reader.ReadAsDateTimeOffsetAsync(cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return reader.TokenType != JsonToken.None;
        }

        private async Task<object> DeserializeConvertableAsync(JsonConverter converter, JsonReader reader, Type objectType, object existingValue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            object value = await converter.ReadJsonAsync(reader, objectType, existingValue, GetInternalSerializer(), cancellationToken).ConfigureAwait(false);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            return value;
        }

        private async Task<object> CreateValueInternalAsync(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, CancellationToken cancellationToken)
        {
            if (contract != null && contract.ContractType == JsonContractType.Linq)
            {
                return await CreateJTokenAsync(reader, contract, cancellationToken).ConfigureAwait(false);
            }

            do
            {
                switch (reader.TokenType)
                {
                    // populate a typed object or generic dictionary/array
                    // depending upon whether an objectType was supplied
                    case JsonToken.StartObject:
                        return await CreateObjectAsync(reader, objectType, contract, member, containerContract, containerMember, existingValue, cancellationToken).ConfigureAwait(false);
                    case JsonToken.StartArray:
                        return await CreateListAsync(reader, objectType, contract, member, existingValue, null, cancellationToken).ConfigureAwait(false);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.String:
                        string s = (string)reader.Value;

                        // convert empty string to null automatically for nullable types
                        if (CoerceEmptyStringToNull(objectType, contract, s))
                        {
                            return null;
                        }

                        // string that needs to be returned as a byte array should be base 64 decoded
                        if (objectType == typeof(byte[]))
                        {
                            return Convert.FromBase64String(s);
                        }

                        return EnsureType(reader, s, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        return EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Null:
                    case JsonToken.Undefined:
#if !(DOTNET || PORTABLE40 || PORTABLE)
                        if (objectType == typeof(DBNull))
                        {
                            return DBNull.Value;
                        }
#endif

                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Raw:
                        return new JRaw((string)reader.Value);
                    case JsonToken.Comment:

                        // ignore
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
                }
            } while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
        }

        private async Task HandleErrorAsync(JsonReader reader, bool readPastError, int initialDepth, CancellationToken cancellationToken)
        {
            ClearErrorContext();

            if (readPastError)
            {
                await reader.SkipAsync(cancellationToken).ConfigureAwait(false);

                while (reader.Depth > initialDepth + 1)
                {
                    if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        break;
                    }
                }
            }
        }

        private Task<bool> CheckPropertyNameAsync(JsonReader reader, string memberName, CancellationToken cancellationToken)
        {
            if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                switch (memberName)
                {
                    case JsonTypeReflector.IdPropertyName:
                    case JsonTypeReflector.RefPropertyName:
                    case JsonTypeReflector.TypePropertyName:
                    case JsonTypeReflector.ArrayValuesPropertyName:
                        return SkipAndTrueAsync(reader, cancellationToken);
                }
            }

            return AsyncUtils.False;
        }

        private async Task<bool> SkipAndTrueAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        private async Task SetExtensionDataAsync(JsonObjectContract contract, JsonProperty member, JsonReader reader, string memberName, object o, CancellationToken cancellationToken)
        {
            if (contract.ExtensionDataSetter != null)
            {
                try
                {
                    object value = await ReadExtensionDataValueAsync(contract, member, reader, cancellationToken).ConfigureAwait(false);

                    contract.ExtensionDataSetter(o, memberName, value);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error setting value in extension data for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType), ex);
                }
            }
            else
            {
                await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private Task<object> ReadExtensionDataValueAsync(JsonObjectContract contract, JsonProperty member, JsonReader reader, CancellationToken cancellationToken)
        {
            return contract.ExtensionDataIsJToken ? JToken.ReadFromAsObjectAsync(reader, cancellationToken) : CreateValueInternalAsync(reader, null, null, null, contract, member, null, cancellationToken);
        }

        private async Task<bool> SetPropertyValueAsync(JsonProperty property, JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target, CancellationToken cancellationToken)
        {
            object currentValue;
            bool useExistingValue;
            JsonContract propertyContract;
            bool gottenCurrentValue;

            if (CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out useExistingValue, out currentValue, out propertyContract, out gottenCurrentValue))
            {
                return false;
            }

            object value;

            if (propertyConverter != null && propertyConverter.CanRead)
            {
                if (!gottenCurrentValue && target != null && property.Readable)
                {
                    currentValue = property.ValueProvider.GetValue(target);
                }

                value = await DeserializeConvertableAsync(propertyConverter, reader, property.PropertyType, currentValue, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                value = await CreateValueInternalAsync(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, useExistingValue ? currentValue : null, cancellationToken).ConfigureAwait(false);
            }

            // always set the value if useExistingValue is false,
            // otherwise also set it if CreateValue returns a new value compared to the currentValue
            // this could happen because of a JsonConverter against the type
            if ((!useExistingValue || value != currentValue) && ShouldSetPropertyValue(property, value))
            {
                property.ValueProvider.SetValue(target, value);

                if (property.SetIsSpecified != null)
                {
                    if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                    {
                        TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
                    }

                    property.SetIsSpecified(target, true);
                }

                return true;
            }

            // the value wasn't set be JSON was populated onto the existing value
            return useExistingValue;
        }

        private async Task EndProcessPropertyAsync(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue, CancellationToken cancellationToken)
        {
            if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
            {
                try
                {
                    Required resolvedRequired = property._required ?? contract.ItemRequired ?? Required.Default;

                    switch (presence)
                    {
                        case PropertyPresence.None:
                            if (resolvedRequired == Required.AllowNull || resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }

                            if (setDefaultValue && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
                                {
                                    property.ValueProvider.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
                                }
                            }
                            break;
                        case PropertyPresence.Null:
                            if (resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }
                            if (resolvedRequired == Required.DisallowNull)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }

                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task<JToken> CreateJTokenAsync(JsonReader reader, JsonContract contract, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (contract != null)
            {
                if (contract.UnderlyingType == typeof(JRaw))
                {
                    return await JRaw.CreateAsync(reader, cancellationToken).ConfigureAwait(false);
                }
                if (reader.TokenType == JsonToken.Null && !(contract.UnderlyingType == typeof(JValue) || contract.UnderlyingType == typeof(JToken)))
                {
                    return null;
                }
            }

            using (JTokenWriter writer = new JTokenWriter())
            {
                await writer.WriteTokenAsync(reader, cancellationToken).ConfigureAwait(false);
                return writer.Token;
            }
        }

        private async Task<object> CreateObjectAsync(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, CancellationToken cancellationToken)
        {
            string id;
            Type resolvedObjectType = objectType;

            if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
            {
                // don't look for metadata properties
                await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);
                id = null;
            }
            else if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                JTokenReader tokenReader = reader as JTokenReader;
                if (tokenReader == null)
                {
                    JToken t = await JToken.ReadFromAsync(reader, cancellationToken).ConfigureAwait(false);
                    tokenReader = (JTokenReader)t.CreateReader();
                    tokenReader.Culture = reader.Culture;
                    tokenReader.DateFormatString = reader.DateFormatString;
                    tokenReader.DateParseHandling = reader.DateParseHandling;
                    tokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                    tokenReader.FloatParseHandling = reader.FloatParseHandling;
                    tokenReader.SupportMultipleContent = reader.SupportMultipleContent;

                    // start
                    tokenReader.ReadAndAssert();

                    reader = tokenReader;
                }

                object newValue;
                if (ReadMetadataPropertiesToken(tokenReader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id))
                {
                    return newValue;
                }
            }
            else
            {
                await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);
                object newValue;
                if (ReadMetadataProperties(reader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id))
                {
                    return newValue;
                }
            }

            if (HasNoDefinedType(contract))
            {
                return await CreateJObjectAsync(reader, cancellationToken).ConfigureAwait(false);
            }

            switch (contract.ContractType)
            {
                case JsonContractType.Object:
                {
                    JsonObjectContract objectContract = (JsonObjectContract)contract;
                    object targetObject;

                    // check that if type name handling is being used that the existing value is compatible with the specified type
                    if (existingValue != null && (resolvedObjectType == objectType || resolvedObjectType.IsAssignableFrom(existingValue.GetType())))
                    {
                        targetObject = existingValue;
                    }
                    else
                    {
                        var created = await CreateNewObjectAsync(reader, objectContract, member, containerMember, id, cancellationToken).ConfigureAwait(false);
                        if (created.Item2)
                        {
                            // don't populate if read from non-default creator because the object has already been read
                            return created.Item1;
                        }

                        targetObject = created.Item1;
                    }

                    return await PopulateObjectAsync(targetObject, reader, objectContract, member, id, cancellationToken).ConfigureAwait(false);
                }
                case JsonContractType.Primitive:
                {
                    JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;

                    // if the content is inside $value then read past it
                    if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore && reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
                    {
                        await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);

                        // the token should not be an object because the $type value could have been included in the object
                        // without needing the $value property
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
                        }

                        object value = await CreateValueInternalAsync(reader, resolvedObjectType, primitiveContract, member, null, null, existingValue, cancellationToken).ConfigureAwait(false);

                        await ReadAndAssertAsync(reader, cancellationToken).ConfigureAwait(false);
                        return value;
                    }

                    break;
                }
                case JsonContractType.Dictionary:
                {
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                    object targetDictionary;

                    if (existingValue == null)
                    {
                        bool createdFromNonDefaultCreator;
                        IDictionary dictionary = CreateNewDictionary(reader, dictionaryContract, out createdFromNonDefaultCreator);

                        if (createdFromNonDefaultCreator)
                        {
                            if (id != null)
                            {
                                throw JsonSerializationException.Create(reader, "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                            }

                            if (contract.OnSerializingCallbacks.Count > 0)
                            {
                                throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                            }

                            if (contract.OnErrorCallbacks.Count > 0)
                            {
                                throw JsonSerializationException.Create(reader, "Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                            }

                            if (!dictionaryContract.HasParameterizedCreatorInternal)
                            {
                                throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size dictionary: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                            }
                        }

                        await PopulateDictionaryAsync(dictionary, reader, dictionaryContract, member, id, cancellationToken).ConfigureAwait(false);

                        if (createdFromNonDefaultCreator)
                        {
                            ObjectConstructor<object> creator = dictionaryContract.OverrideCreator ?? dictionaryContract.ParameterizedCreator;

                            return creator(dictionary);
                        }
                        else if (dictionary is IWrappedDictionary)
                        {
                            return ((IWrappedDictionary)dictionary).UnderlyingDictionary;
                        }

                        targetDictionary = dictionary;
                    }
                    else
                    {
                        targetDictionary = await PopulateDictionaryAsync(dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(existingValue) : (IDictionary)existingValue, reader, dictionaryContract, member, id, cancellationToken).ConfigureAwait(false);
                    }

                    return targetDictionary;
                }
#if !(PORTABLE40)
                case JsonContractType.Dynamic:
                    JsonDynamicContract dynamicContract = (JsonDynamicContract)contract;
                    return await CreateDynamicAsync(reader, dynamicContract, member, id, cancellationToken).ConfigureAwait(false);
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
                case JsonContractType.Serializable:
                    JsonISerializableContract serializableContract = (JsonISerializableContract)contract;
                    return await CreateISerializableAsync(reader, serializableContract, member, id, cancellationToken).ConfigureAwait(false);
#endif
            }

            string message = @"Cannot deserialize the current JSON object (e.g. {{""name"":""value""}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine + @"To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
            message = message.FormatWith(CultureInfo.InvariantCulture, resolvedObjectType, GetExpectedDescription(contract));

            throw JsonSerializationException.Create(reader, message);
        }

        private async Task<object> CreateListAsync(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string id, CancellationToken cancellationToken)
        {
            object value;

            if (HasNoDefinedType(contract))
            {
                return CreateJToken(reader, contract);
            }

            JsonArrayContract arrayContract = EnsureArrayContract(reader, objectType, contract);

            if (existingValue == null)
            {
                bool createdFromNonDefaultCreator;
                IList list = CreateNewList(reader, arrayContract, out createdFromNonDefaultCreator);

                if (createdFromNonDefaultCreator)
                {
                    if (id != null)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnSerializingCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnErrorCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (!arrayContract.HasParameterizedCreatorInternal && !arrayContract.IsArray)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }
                }

                if (!arrayContract.IsMultidimensionalArray)
                {
                    await PopulateListAsync(list, reader, arrayContract, member, id, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await PopulateMultidimensionalArrayAsync(list, reader, arrayContract, member, id, cancellationToken).ConfigureAwait(false);
                }

                if (createdFromNonDefaultCreator)
                {
                    if (arrayContract.IsMultidimensionalArray)
                    {
                        list = CollectionUtils.ToMultidimensionalArray(list, arrayContract.CollectionItemType, contract.CreatedType.GetArrayRank());
                    }
                    else if (arrayContract.IsArray)
                    {
                        Array a = Array.CreateInstance(arrayContract.CollectionItemType, list.Count);
                        list.CopyTo(a, 0);
                        list = a;
                    }
                    else
                    {
                        ObjectConstructor<object> creator = arrayContract.OverrideCreator ?? arrayContract.ParameterizedCreator;

                        return creator(list);
                    }
                }
                else if (list is IWrappedCollection)
                {
                    return ((IWrappedCollection)list).UnderlyingCollection;
                }

                value = list;
            }
            else
            {
                if (!arrayContract.CanDeserialize)
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
                }

                value = PopulateListAsync(arrayContract.ShouldCreateWrapper ? arrayContract.CreateWrapper(existingValue) : (IList)existingValue, reader, arrayContract, member, id, cancellationToken);
            }

            return value;
        }

        public async Task<Tuple<object, bool>> CreateNewObjectAsync(JsonReader reader, JsonObjectContract objectContract, JsonProperty containerMember, JsonProperty containerProperty, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            object newObject = null;
            if (objectContract.OverrideCreator != null)
            {
                if (objectContract.CreatorParameters.Count > 0)
                {
                    return Tuple.Create(await CreateObjectUsingCreatorWithParametersAsync(reader, objectContract, containerMember, objectContract.OverrideCreator, id, cancellationToken).ConfigureAwait(false), true);
                }

                newObject = objectContract.OverrideCreator(new object[0]);
            }
            else if (objectContract.DefaultCreator != null && (!objectContract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
            {
                // use the default constructor if it is...
                // public
                // non-public and the user has change constructor handling settings
                // non-public and there is no other creator
                newObject = objectContract.DefaultCreator();
            }
            else if (objectContract.ParameterizedCreator != null)
            {
                return Tuple.Create(await CreateObjectUsingCreatorWithParametersAsync(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id, cancellationToken).ConfigureAwait(false), true);
            }

            if (newObject == null)
            {
                if (!objectContract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
            }

            return Tuple.Create(newObject, false);
        }

        private async Task<object> CreateObjectUsingCreatorWithParametersAsync(JsonReader reader, JsonObjectContract contract, JsonProperty containerProperty, ObjectConstructor<object> creator, string id, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            // only need to keep a track of properies presence if they are required or a value should be defaulted if missing
            bool trackPresence = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate);

            Type objectType = contract.UnderlyingType;

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                string parameters = string.Join(", ", contract.CreatorParameters.Select(p => p.PropertyName).ToArray());
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using creator with parameters: {1}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, parameters)), null);
            }

            List<CreatorPropertyContext> propertyContexts = await ResolvePropertyAndCreatorValuesAsync(contract, containerProperty, reader, objectType, cancellationToken).ConfigureAwait(false);
            if (trackPresence)
            {
                foreach (JsonProperty property in contract.Properties)
                {
                    if (propertyContexts.All(p => p.Property != property))
                    {
                        propertyContexts.Add(new CreatorPropertyContext {Property = property, Name = property.PropertyName, Presence = PropertyPresence.None});
                    }
                }
            }

            object[] creatorParameterValues = new object[contract.CreatorParameters.Count];

            foreach (CreatorPropertyContext context in propertyContexts)
            {
                // set presence of read values
                if (trackPresence)
                {
                    if (context.Property != null && context.Presence == null)
                    {
                        object v = context.Value;
                        PropertyPresence propertyPresence;
                        if (v == null)
                        {
                            propertyPresence = PropertyPresence.Null;
                        }
                        else if (v is string)
                        {
                            propertyPresence = CoerceEmptyStringToNull(context.Property.PropertyType, context.Property.PropertyContract, (string)v) ? PropertyPresence.Null : PropertyPresence.Value;
                        }
                        else
                        {
                            propertyPresence = PropertyPresence.Value;
                        }

                        context.Presence = propertyPresence;
                    }
                }

                JsonProperty constructorProperty = context.ConstructorProperty;
                if (constructorProperty == null && context.Property != null)
                {
                    constructorProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind(p => p.PropertyName, context.Property.UnderlyingName);
                }

                if (constructorProperty != null && !constructorProperty.Ignored)
                {
                    // handle giving default values to creator parameters
                    // this needs to happen before the call to creator
                    if (trackPresence)
                    {
                        if (context.Presence == PropertyPresence.None || context.Presence == PropertyPresence.Null)
                        {
                            if (constructorProperty.PropertyContract == null)
                            {
                                constructorProperty.PropertyContract = GetContractSafe(constructorProperty.PropertyType);
                            }

                            if (HasFlag(constructorProperty.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate))
                            {
                                context.Value = EnsureType(reader, constructorProperty.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, constructorProperty.PropertyContract, constructorProperty.PropertyType);
                            }
                        }
                    }

                    creatorParameterValues[contract.CreatorParameters.IndexOf(constructorProperty)] = context.Value;

                    context.Used = true;
                }
            }

            object createdObject = creator(creatorParameterValues);

            if (id != null)
            {
                AddReference(reader, id, createdObject);
            }

            OnDeserializing(reader, contract, createdObject);

            // go through unused values and set the newly created object's properties
            foreach (CreatorPropertyContext context in propertyContexts)
            {
                if (context.Used || context.Property == null || context.Property.Ignored || context.Presence == PropertyPresence.None)
                {
                    continue;
                }

                JsonProperty property = context.Property;
                object value = context.Value;

                if (ShouldSetPropertyValue(property, value))
                {
                    property.ValueProvider.SetValue(createdObject, value);
                    context.Used = true;
                }
                else if (!property.Writable && value != null)
                {
                    // handle readonly collection/dictionary properties
                    JsonContract propertyContract = Serializer._contractResolver.ResolveContract(property.PropertyType);

                    if (propertyContract.ContractType == JsonContractType.Array)
                    {
                        JsonArrayContract propertyArrayContract = (JsonArrayContract)propertyContract;

                        object createdObjectCollection = property.ValueProvider.GetValue(createdObject);
                        if (createdObjectCollection != null)
                        {
                            IWrappedCollection createdObjectCollectionWrapper = propertyArrayContract.CreateWrapper(createdObjectCollection);
                            IWrappedCollection newValues = propertyArrayContract.CreateWrapper(value);

                            foreach (object newValue in newValues)
                            {
                                createdObjectCollectionWrapper.Add(newValue);
                            }
                        }
                    }
                    else if (propertyContract.ContractType == JsonContractType.Dictionary)
                    {
                        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)propertyContract;

                        object createdObjectDictionary = property.ValueProvider.GetValue(createdObject);
                        if (createdObjectDictionary != null)
                        {
                            IDictionary targetDictionary = dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(createdObjectDictionary) : (IDictionary)createdObjectDictionary;
                            IDictionary newValues = dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(value) : (IDictionary)value;

                            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                            IDictionaryEnumerator e = newValues.GetEnumerator();
                            try
                            {
                                while (e.MoveNext())
                                {
                                    DictionaryEntry entry = e.Entry;
                                    targetDictionary[entry.Key] = entry.Value;
                                }
                            }
                            finally
                            {
                                (e as IDisposable)?.Dispose();
                            }
                        }
                    }

                    context.Used = true;
                }
            }

            if (contract.ExtensionDataSetter != null)
            {
                foreach (CreatorPropertyContext propertyValue in propertyContexts)
                {
                    if (!propertyValue.Used)
                    {
                        contract.ExtensionDataSetter(createdObject, propertyValue.Name, propertyValue.Value);
                    }
                }
            }

            if (trackPresence)
            {
                foreach (CreatorPropertyContext context in propertyContexts)
                {
                    if (context.Property == null)
                    {
                        continue;
                    }

                    await EndProcessPropertyAsync(createdObject, reader, contract, reader.Depth, context.Property, context.Presence.GetValueOrDefault(), !context.Used, cancellationToken).ConfigureAwait(false);
                }
            }

            OnDeserialized(reader, contract, createdObject);
            return createdObject;
        }

        private async Task<List<CreatorPropertyContext>> ResolvePropertyAndCreatorValuesAsync(JsonObjectContract contract, JsonProperty containerProperty, JsonReader reader, Type objectType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<CreatorPropertyContext> propertyValues = new List<CreatorPropertyContext>();
            bool exit = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        CreatorPropertyContext creatorPropertyContext = new CreatorPropertyContext {Name = reader.Value.ToString(), ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(memberName), Property = contract.Properties.GetClosestMatchProperty(memberName)};
                        propertyValues.Add(creatorPropertyContext);

                        JsonProperty property = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
                        if (property != null && !property.Ignored)
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, containerProperty);

                            if (!await ReadForTypeAsync(reader, property.PropertyContract, propertyConverter != null, cancellationToken).ConfigureAwait(false))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (propertyConverter != null && propertyConverter.CanRead)
                            {
                                creatorPropertyContext.Value = await DeserializeConvertableAsync(propertyConverter, reader, property.PropertyType, null, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                creatorPropertyContext.Value = await CreateValueInternalAsync(reader, property.PropertyType, property.PropertyContract, property, contract, containerProperty, null, cancellationToken).ConfigureAwait(false);
                            }

                            continue;
                        }
                        else
                        {
                            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                            {
                                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                            }

                            if (Serializer._missingMemberHandling == MissingMemberHandling.Error)
                            {
                                throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));
                            }
                        }

                        if (contract.ExtensionDataSetter != null)
                        {
                            creatorPropertyContext.Value = await ReadExtensionDataValueAsync(contract, containerProperty, reader, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        exit = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!exit && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (!exit)
            {
                ThrowUnexpectedEndException(reader, contract, null, "Unexpected end when deserializing object.");
            }

            return propertyValues;
        }

        public async Task<object> DeserializeAsync(JsonReader reader, Type objectType, bool checkAdditionalContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            JsonContract contract = GetContractSafe(objectType);

            try
            {
                JsonConverter converter = GetConverter(contract, null, null, null);

                if (reader.TokenType == JsonToken.None && !await ReadForTypeAsync(reader, contract, converter != null, cancellationToken).ConfigureAwait(false))
                {
                    if (contract != null && !contract.IsNullable)
                    {
                        throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    return null;
                }

                object deserializedValue;

                if (converter != null && converter.CanRead)
                {
                    deserializedValue = await DeserializeConvertableAsync(converter, reader, objectType, null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    deserializedValue = await CreateValueInternalAsync(reader, objectType, contract, null, null, null, null, cancellationToken).ConfigureAwait(false);
                }

                if (checkAdditionalContent)
                {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.Comment)
                    {
                        throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
                    }
                }

                return deserializedValue;
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    await HandleErrorAsync(reader, false, 0, cancellationToken).ConfigureAwait(false);
                    return null;
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
        }

#if !(PORTABLE40)
        private async Task<object> CreateDynamicAsync(JsonReader reader, JsonDynamicContract contract, JsonProperty member, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!contract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            IDynamicMetaObjectProvider newObject;

            if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                newObject = (IDynamicMetaObjectProvider)contract.DefaultCreator();
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            if (id != null)
            {
                AddReference(reader, id, newObject);
            }

            OnDeserializing(reader, contract, newObject);

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        try
                        {
                            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            // first attempt to find a settable property, otherwise fall back to a dynamic set without type
                            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                            if (property != null && property.Writable && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, null, null);

                                if (!await SetPropertyValueAsync(property, propertyConverter, null, member, reader, newObject, cancellationToken).ConfigureAwait(false))
                                {
                                    await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                Type t = JsonTokenUtils.IsPrimitiveToken(reader.TokenType) ? reader.ValueType : typeof(IDynamicMetaObjectProvider);

                                JsonContract dynamicMemberContract = GetContractSafe(t);
                                JsonConverter dynamicMemberConverter = GetConverter(dynamicMemberContract, null, null, member);

                                object value;
                                if (dynamicMemberConverter != null && dynamicMemberConverter.CanRead)
                                {
                                    value = await DeserializeConvertableAsync(dynamicMemberConverter, reader, t, null, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    value = await CreateValueInternalAsync(reader, t, dynamicMemberContract, null, null, member, null, cancellationToken).ConfigureAwait(false);
                                }

                                contract.TrySetMember(newObject, memberName, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            OnDeserialized(reader, contract, newObject);

            return newObject;
        }

        private async Task<JToken> CreateJObjectAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // this is needed because we've already read inside the object, looking for metadata properties
            using (JTokenWriter writer = new JTokenWriter())
            {
                writer.WriteStartObject();

                do
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = (string)reader.Value;
                        if (!await reader.ReadAndMoveToContentAsync(cancellationToken).ConfigureAwait(false))
                        {
                            break;
                        }

                        if (await CheckPropertyNameAsync(reader, propertyName, cancellationToken).ConfigureAwait(false))
                        {
                            continue;
                        }

                        writer.WritePropertyName(propertyName);
                        await writer.WriteTokenAsync(reader, true, true, false, cancellationToken).ConfigureAwait(false);
                    }
                    else if (reader.TokenType == JsonToken.Comment)
                    {
                        // eat
                    }
                    else
                    {
                        await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
                        return writer.Token;
                    }
                } while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
            }
        }

        private async Task<object> PopulateMultidimensionalArrayAsync(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rank = contract.UnderlyingType.GetArrayRank();

            if (id != null)
            {
                AddReference(reader, id, list);
            }

            OnDeserializing(reader, contract, list);

            JsonContract collectionItemContract = GetContractSafe(contract.CollectionItemType);
            JsonConverter collectionItemConverter = GetConverter(collectionItemContract, null, contract, containerProperty);

            int? previousErrorIndex = null;
            Stack<IList> listStack = new Stack<IList>();
            listStack.Push(list);
            IList currentList = list;

            bool finished = false;
            do
            {
                int initialDepth = reader.Depth;

                if (listStack.Count == rank)
                {
                    try
                    {
                        if (await ReadForTypeAsync(reader, collectionItemContract, collectionItemConverter != null, cancellationToken).ConfigureAwait(false))
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.EndArray:
                                    listStack.Pop();
                                    currentList = listStack.Peek();
                                    previousErrorIndex = null;
                                    break;
                                default:
                                    object value;

                                    if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                    {
                                        value = await DeserializeConvertableAsync(collectionItemConverter, reader, contract.CollectionItemType, null, cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        value = await CreateValueInternalAsync(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null, cancellationToken).ConfigureAwait(false);
                                    }

                                    currentList.Add(value);
                                    break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        JsonPosition errorPosition = reader.GetPosition(initialDepth);

                        if (IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            await HandleErrorAsync(reader, true, initialDepth, cancellationToken).ConfigureAwait(false);

                            if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                            {
                                // reader index has not moved since previous error handling
                                // break out of reading array to prevent infinite loop
                                throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                            }
                            else
                            {
                                previousErrorIndex = errorPosition.Position;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartArray:
                                IList newList = new List<object>();
                                currentList.Add(newList);
                                listStack.Push(newList);
                                currentList = newList;
                                break;
                            case JsonToken.EndArray:
                                listStack.Pop();

                                if (listStack.Count > 0)
                                {
                                    currentList = listStack.Peek();
                                }
                                else
                                {
                                    finished = true;
                                }
                                break;
                            case JsonToken.Comment:
                                break;
                            default:
                                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
            }

            OnDeserialized(reader, contract, list);
            return list;
        }
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
        private async Task<object> CreateISerializableAsync(JsonReader reader, JsonISerializableContract contract, JsonProperty member, string id, CancellationToken cancellationToken)
        {
            Type objectType = contract.UnderlyingType;

            if (!JsonTypeReflector.FullyTrusted)
            {
                string message = @"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine + @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, objectType);

                throw JsonSerializationException.Create(reader, message);
            }

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();
                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                        }

                        serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
            }

            if (contract.ISerializableCreator == null)
            {
                throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }

            object createdObject = contract.ISerializableCreator(serializationInfo, Serializer._context);

            if (id != null)
            {
                AddReference(reader, id, createdObject);
            }

            // these are together because OnDeserializing takes an object but for an ISerializable the object is fully created in the constructor
            OnDeserializing(reader, contract, createdObject);
            OnDeserialized(reader, contract, createdObject);

            return createdObject;
        }
#endif
    }
}

#endif