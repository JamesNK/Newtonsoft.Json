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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security;
#if !(NETFX_CORE || PORTABLE || PORTABLE40)
using System.Security.Permissions;
#endif
using Newtonsoft.Json.Utilities;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
    internal static class JsonTypeReflector
    {
        private static bool? _dynamicCodeGeneration;
        private static bool? _fullyTrusted;

        public const string IdPropertyName = "$id";
        public const string RefPropertyName = "$ref";
        public const string TypePropertyName = "$type";
        public const string ValuePropertyName = "$value";
        public const string ArrayValuesPropertyName = "$values";

        public const string ShouldSerializePrefix = "ShouldSerialize";
        public const string SpecifiedPostfix = "Specified";

        private static readonly ThreadSafeStore<Type, Func<JsonConverter>> JsonConverterCreatorCache = new ThreadSafeStore<Type, Func<JsonConverter>>(GetJsonConverterCreator);

#if !(NET20 || NETFX_CORE)
        private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(GetAssociateMetadataTypeFromAttribute);

        private const string MetadataTypeAttributeTypeName =
            "System.ComponentModel.DataAnnotations.MetadataTypeAttribute, System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        private static Type _cachedMetadataTypeAttributeType;
#endif

        public static T GetCachedAttribute<T>(object attributeProvider) where T : Attribute
        {
            return CachedAttributeGetter<T>.GetAttribute(attributeProvider);
        }

#if !NET20
        public static DataContractAttribute GetDataContractAttribute(Type type)
        {
            // DataContractAttribute does not have inheritance
            Type currentType = type;

            while (currentType != null)
            {
                DataContractAttribute result = CachedAttributeGetter<DataContractAttribute>.GetAttribute(currentType);
                if (result != null)
                    return result;

                currentType = currentType.BaseType();
            }

            return null;
        }

        public static DataMemberAttribute GetDataMemberAttribute(MemberInfo memberInfo)
        {
            // DataMemberAttribute does not have inheritance

            // can't override a field
            if (memberInfo.MemberType() == MemberTypes.Field)
                return CachedAttributeGetter<DataMemberAttribute>.GetAttribute(memberInfo);

            // search property and then search base properties if nothing is returned and the property is virtual
            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            DataMemberAttribute result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(propertyInfo);
            if (result == null)
            {
                if (propertyInfo.IsVirtual())
                {
                    Type currentType = propertyInfo.DeclaringType;

                    while (result == null && currentType != null)
                    {
                        PropertyInfo baseProperty = (PropertyInfo)ReflectionUtils.GetMemberInfoFromType(currentType, propertyInfo);
                        if (baseProperty != null && baseProperty.IsVirtual())
                            result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(baseProperty);

                        currentType = currentType.BaseType();
                    }
                }
            }

            return result;
        }
#endif

        public static MemberSerialization GetObjectMemberSerialization(Type objectType, bool ignoreSerializableAttribute)
        {
            JsonObjectAttribute objectAttribute = GetCachedAttribute<JsonObjectAttribute>(objectType);
            if (objectAttribute != null)
                return objectAttribute.MemberSerialization;

#if !NET20
            DataContractAttribute dataContractAttribute = GetDataContractAttribute(objectType);
            if (dataContractAttribute != null)
                return MemberSerialization.OptIn;
#endif

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
            if (!ignoreSerializableAttribute)
            {
                SerializableAttribute serializableAttribute = GetCachedAttribute<SerializableAttribute>(objectType);
                if (serializableAttribute != null)
                    return MemberSerialization.Fields;
            }
#endif

            // the default
            return MemberSerialization.OptOut;
        }

        public static JsonConverter GetJsonConverter(object attributeProvider)
        {
            JsonConverterAttribute converterAttribute = GetCachedAttribute<JsonConverterAttribute>(attributeProvider);

            if (converterAttribute != null)
            {
                Func<JsonConverter> creator = JsonConverterCreatorCache.Get(converterAttribute.ConverterType);
                if (creator != null)
                    return creator();
            }

            return null;
        }

        public static JsonConverter CreateJsonConverterInstance(Type converterType)
        {
            Func<JsonConverter> converterCreator = JsonConverterCreatorCache.Get(converterType);
            return converterCreator();
        }

        private static Func<JsonConverter> GetJsonConverterCreator(Type converterType)
        {
            Func<object> defaultConstructor = (ReflectionUtils.HasDefaultConstructor(converterType, false))
                ? ReflectionDelegateFactory.CreateDefaultConstructor<object>(converterType)
                : null;

            return () =>
            {
                try
                {
                    if (defaultConstructor == null)
                        throw new JsonException("No parameterless constructor defined for '{0}'.".FormatWith(CultureInfo.InvariantCulture, converterType));

                    return (JsonConverter)defaultConstructor();
                }
                catch (Exception ex)
                {
                    throw new JsonException("Error creating '{0}'.".FormatWith(CultureInfo.InvariantCulture, converterType), ex);
                }
            };
        }

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
        public static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
#endif

#if !(NET20 || NETFX_CORE)
        private static Type GetAssociatedMetadataType(Type type)
        {
            return AssociatedMetadataTypesCache.Get(type);
        }

        private static ReflectionObject _metadataTypeAttributeReflectionObject;

        private static Type GetAssociateMetadataTypeFromAttribute(Type type)
        {
            Type metadataTypeAttributeType = GetMetadataTypeAttributeType();
            if (metadataTypeAttributeType == null)
                return null;

            Attribute attribute = ReflectionUtils.GetAttributes(type, metadataTypeAttributeType, true).SingleOrDefault();
            if (attribute == null)
                return null;

            const string metadataClassTypeName = "MetadataClassType";

            if (_metadataTypeAttributeReflectionObject == null)
                _metadataTypeAttributeReflectionObject = ReflectionObject.Create(metadataTypeAttributeType, metadataClassTypeName);

            return (Type)_metadataTypeAttributeReflectionObject.GetValue(attribute, metadataClassTypeName);
        }

        private static Type GetMetadataTypeAttributeType()
        {
            // always attempt to get the metadata type attribute type
            // the assembly may have been loaded since last time
            if (_cachedMetadataTypeAttributeType == null)
            {
                Type metadataTypeAttributeType = Type.GetType(MetadataTypeAttributeTypeName);

                if (metadataTypeAttributeType != null)
                    _cachedMetadataTypeAttributeType = metadataTypeAttributeType;
                else
                    return null;
            }

            return _cachedMetadataTypeAttributeType;
        }
#endif

        private static T GetAttribute<T>(Type type) where T : Attribute
        {
            T attribute;

#if !(NET20 || NETFX_CORE)
            Type metadataType = GetAssociatedMetadataType(type);
            if (metadataType != null)
            {
                attribute = ReflectionUtils.GetAttribute<T>(metadataType, true);
                if (attribute != null)
                    return attribute;
            }
#endif

            attribute = ReflectionUtils.GetAttribute<T>(type, true);
            if (attribute != null)
                return attribute;

            foreach (Type typeInterface in type.GetInterfaces())
            {
                attribute = ReflectionUtils.GetAttribute<T>(typeInterface, true);
                if (attribute != null)
                    return attribute;
            }

            return null;
        }

        private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            T attribute;

#if !(NET20 || NETFX_CORE)
            Type metadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
            if (metadataType != null)
            {
                MemberInfo metadataTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(metadataType, memberInfo);

                if (metadataTypeMemberInfo != null)
                {
                    attribute = ReflectionUtils.GetAttribute<T>(metadataTypeMemberInfo, true);
                    if (attribute != null)
                        return attribute;
                }
            }
#endif

            attribute = ReflectionUtils.GetAttribute<T>(memberInfo, true);
            if (attribute != null)
                return attribute;

            if (memberInfo.DeclaringType != null)
            {
                foreach (Type typeInterface in memberInfo.DeclaringType.GetInterfaces())
                {
                    MemberInfo interfaceTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(typeInterface, memberInfo);

                    if (interfaceTypeMemberInfo != null)
                    {
                        attribute = ReflectionUtils.GetAttribute<T>(interfaceTypeMemberInfo, true);
                        if (attribute != null)
                            return attribute;
                    }
                }
            }

            return null;
        }

        public static T GetAttribute<T>(object provider) where T : Attribute
        {
            Type type = provider as Type;
            if (type != null)
                return GetAttribute<T>(type);

            MemberInfo memberInfo = provider as MemberInfo;
            if (memberInfo != null)
                return GetAttribute<T>(memberInfo);

            return ReflectionUtils.GetAttribute<T>(provider, true);
        }

#if DEBUG
        internal static void SetFullyTrusted(bool fullyTrusted)
        {
            _fullyTrusted = fullyTrusted;
        }

        internal static void SetDynamicCodeGeneration(bool dynamicCodeGeneration)
        {
            _dynamicCodeGeneration = dynamicCodeGeneration;
        }
#endif

        public static bool DynamicCodeGeneration
        {
#if !(NET20 || NET35 || NETFX_CORE || PORTABLE)
            [SecuritySafeCritical]
#endif
                get
            {
                if (_dynamicCodeGeneration == null)
                {
#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
                    try
                    {
                        new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
                        new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Demand();
                        new SecurityPermission(SecurityPermissionFlag.SkipVerification).Demand();
                        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                        new SecurityPermission(PermissionState.Unrestricted).Demand();
                        _dynamicCodeGeneration = true;
                    }
                    catch (Exception)
                    {
                        _dynamicCodeGeneration = false;
                    }
#else
                    _dynamicCodeGeneration = false;
#endif
                }

                return _dynamicCodeGeneration.Value;
            }
        }

        public static bool FullyTrusted
        {
            get
            {
                if (_fullyTrusted == null)
                {
#if (NETFX_CORE || PORTABLE || PORTABLE40)
                    _fullyTrusted = false;
#elif !(NET20 || NET35 || PORTABLE40)
                    AppDomain appDomain = AppDomain.CurrentDomain;

                    _fullyTrusted = appDomain.IsHomogenous && appDomain.IsFullyTrusted;
#else
                    try
                    {
                        new SecurityPermission(PermissionState.Unrestricted).Demand();
                        _fullyTrusted = true;
                    }
                    catch (Exception)
                    {
                        _fullyTrusted = false;
                    }
#endif
                }

                return _fullyTrusted.Value;
            }
        }

        public static ReflectionDelegateFactory ReflectionDelegateFactory
        {
            get
            {
#if !(PORTABLE40 || PORTABLE || NETFX_CORE)
                if (DynamicCodeGeneration)
                    return DynamicReflectionDelegateFactory.Instance;

                return LateBoundReflectionDelegateFactory.Instance;
#elif !(PORTABLE40)
                return ExpressionReflectionDelegateFactory.Instance;
#else
                return LateBoundReflectionDelegateFactory.Instance;
#endif
            }
        }
    }
}