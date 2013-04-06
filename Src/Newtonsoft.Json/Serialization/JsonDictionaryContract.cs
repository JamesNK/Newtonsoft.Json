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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonDictionaryContract : JsonContainerContract
  {
    /// <summary>
    /// Gets or sets the property name resolver.
    /// </summary>
    /// <value>The property name resolver.</value>
    public Func<string, string> PropertyNameResolver { get; set; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the dictionary keys.
    /// </summary>
    /// <value>The <see cref="Type"/> of the dictionary keys.</value>
    public Type DictionaryKeyType { get; private set; }
    /// <summary>
    /// Gets the <see cref="Type"/> of the dictionary values.
    /// </summary>
    /// <value>The <see cref="Type"/> of the dictionary values.</value>
    public Type DictionaryValueType { get; private set; }

    internal JsonContract KeyContract { get; set; }

    private readonly bool _isDictionaryValueTypeNullableType;
    private readonly Type _genericCollectionDefinitionType;

    private Type _genericWrapperType;
    private MethodCall<object, object> _genericWrapperCreator;

    private Func<object> _genericTemporaryDictionaryCreator;

    internal bool ShouldCreateWrapper { get; private set; }
    internal ConstructorInfo ParametrizedConstructor { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDictionaryContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonDictionaryContract(Type underlyingType)
      : base(underlyingType)
    {
      ContractType = JsonContractType.Dictionary;

      Type keyType;
      Type valueType;

      if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<,>), out _genericCollectionDefinitionType))
      {
        keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

        if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IDictionary<,>)))
          CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE40)
        IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyDictionary<,>));
#endif
      }
#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE40)
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IReadOnlyDictionary<,>), out _genericCollectionDefinitionType))
      {
        keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

        if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IReadOnlyDictionary<,>)))
          CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);

        IsReadOnlyOrFixedSize = true;
      }
#endif
      else
      {
        ReflectionUtils.GetDictionaryKeyValueTypes(UnderlyingType, out keyType, out valueType);

        if (UnderlyingType == typeof(IDictionary))
          CreatedType = typeof(Dictionary<object, object>);
      }

      if (keyType != null && valueType != null)
        ParametrizedConstructor = CollectionUtils.ResolveEnumableCollectionConstructor(CreatedType, typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType));

      ShouldCreateWrapper = !typeof (IDictionary).IsAssignableFrom(CreatedType);

      DictionaryKeyType = keyType;
      DictionaryValueType = valueType;

      if (DictionaryValueType != null)
        _isDictionaryValueTypeNullableType = ReflectionUtils.IsNullableType(DictionaryValueType);

#if (NET20 || NET35)
      Type tempDictioanryType;

        // bug in .NET 2.0 & 3.5 that Dictionary<TKey, Nullable<TValue>> throws an error when adding null via IDictionary[key] = object
      // wrapper will handle calling Add(T) instead
      if (_isDictionaryValueTypeNullableType
        && (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(Dictionary<,>), out tempDictioanryType)))
      {
        ShouldCreateWrapper = true;
      }
#endif
    }

    internal IWrappedDictionary CreateWrapper(object dictionary)
    {
      if (_genericWrapperCreator == null)
      {
        _genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

        ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { _genericCollectionDefinitionType });
        _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(genericWrapperConstructor);
      }

      return (IWrappedDictionary)_genericWrapperCreator(null, dictionary);
    }

    internal IDictionary CreateTemporaryDictionary()
    {
      if (_genericTemporaryDictionaryCreator == null)
      {
        Type temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

        _genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
      }

      return (IDictionary)_genericTemporaryDictionaryCreator();
    }
  }
}