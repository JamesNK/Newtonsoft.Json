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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Collections;

#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#endif

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class JsonDictionaryContract : JsonContainerContract
    {
        /// <summary>
        /// Gets or sets the dictionary key resolver.
        /// </summary>
        /// <value>The dictionary key resolver.</value>
        public Func<string, string>? DictionaryKeyResolver { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary keys.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary keys.</value>
        public Type? DictionaryKeyType { get; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary values.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary values.</value>
        public Type? DictionaryValueType { get; }

        internal JsonContract? KeyContract { get; set; }

        private readonly Type? _genericCollectionDefinitionType;

        private Type? _genericWrapperType;
        private ObjectConstructor<object>? _genericWrapperCreator;

        private Func<object>? _genericTemporaryDictionaryCreator;

        internal bool ShouldCreateWrapper { get; }

        private readonly ConstructorInfo? _parameterizedConstructor;

        private ObjectConstructor<object>? _overrideCreator;
        private ObjectConstructor<object>? _parameterizedCreator;

        internal ObjectConstructor<object>? ParameterizedCreator
        {
            get
            {
                if (_parameterizedCreator == null && _parameterizedConstructor != null)
                {
                    _parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
                }

                return _parameterizedCreator;
            }
        }

        /// <summary>
        /// Gets or sets the function used to create the object. When set this function will override <see cref="JsonContract.DefaultCreator"/>.
        /// </summary>
        /// <value>The function used to create the object.</value>
        public ObjectConstructor<object>? OverrideCreator
        {
            get => _overrideCreator;
            set => _overrideCreator = value;
        }

        /// <summary>
        /// Gets a value indicating whether the creator has a parameter with the dictionary values.
        /// </summary>
        /// <value><c>true</c> if the creator has a parameter with the dictionary values; otherwise, <c>false</c>.</value>
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal => (HasParameterizedCreator || _parameterizedCreator != null || _parameterizedConstructor != null);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDictionaryContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public JsonDictionaryContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Dictionary;

            Type? keyType;
            Type? valueType;

            if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<,>), out _genericCollectionDefinitionType))
            {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<,>)))
                {
                    CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                }
                else if (NonNullableUnderlyingType.IsGenericType())
                {
                    // ConcurrentDictionary<,> + IDictionary setter + null value = error
                    // wrap to use generic setter
                    // https://github.com/JamesNK/Newtonsoft.Json/issues/1582
                    Type typeDefinition = NonNullableUnderlyingType.GetGenericTypeDefinition();
                    if (typeDefinition.FullName == JsonTypeReflector.ConcurrentDictionaryTypeName)
                    {
                        ShouldCreateWrapper = true;
                    }
                }

#if HAVE_READ_ONLY_COLLECTIONS
                IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(NonNullableUnderlyingType, typeof(ReadOnlyDictionary<,>));
#endif

            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<,>), out _genericCollectionDefinitionType))
            {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<,>)))
                {
                    CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
                }

                IsReadOnlyOrFixedSize = true;
            }
#endif
            else
            {
                ReflectionUtils.GetDictionaryKeyValueTypes(NonNullableUnderlyingType, out keyType, out valueType);

                if (NonNullableUnderlyingType == typeof(IDictionary))
                {
                    CreatedType = typeof(Dictionary<object, object>);
                }
            }

            if (keyType != null && valueType != null)
            {
                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(
                    CreatedType,
                    typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType),
                    typeof(IDictionary<,>).MakeGenericType(keyType, valueType));

#if HAVE_FSHARP_TYPES
                if (!HasParameterizedCreatorInternal && NonNullableUnderlyingType.Name == FSharpUtils.FSharpMapTypeName)
                {
                    FSharpUtils.EnsureInitialized(NonNullableUnderlyingType.Assembly());
                    _parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
                }
#endif
            }

            if (!typeof(IDictionary).IsAssignableFrom(CreatedType))
            {
                ShouldCreateWrapper = true;
            }

            DictionaryKeyType = keyType;
            DictionaryValueType = valueType;

#if (NET20 || NET35)
            if (DictionaryValueType != null && ReflectionUtils.IsNullableType(DictionaryValueType))
            {
                // bug in .NET 2.0 & 3.5 that Dictionary<TKey, Nullable<TValue>> throws an error when adding null via IDictionary[key] = object
                // wrapper will handle calling Add(T) instead
                if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(Dictionary<,>), out _))
                {
                    ShouldCreateWrapper = true;
                }
            }
#endif

            if (DictionaryKeyType != null && 
                DictionaryValueType != null &&
                ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(
                    NonNullableUnderlyingType,
                    DictionaryKeyType,
                    DictionaryValueType,
                    out Type? immutableCreatedType,
                    out ObjectConstructor<object>? immutableParameterizedCreator))
            {
                CreatedType = immutableCreatedType;
                _parameterizedCreator = immutableParameterizedCreator;
                IsReadOnlyOrFixedSize = true;
            }
        }

        internal IWrappedDictionary CreateWrapper(object dictionary)
        {
            if (_genericWrapperCreator == null)
            {
                _genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

                ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { _genericCollectionDefinitionType! });
                _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedDictionary)_genericWrapperCreator(dictionary);
        }

        internal IDictionary CreateTemporaryDictionary()
        {
            if (_genericTemporaryDictionaryCreator == null)
            {
                Type temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));

                _genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
            }

            return (IDictionary)_genericTemporaryDictionaryCreator();
        }
    }
}