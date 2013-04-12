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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Collections;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonArrayContract : JsonContainerContract
  {
    /// <summary>
    /// Gets the <see cref="Type"/> of the collection items.
    /// </summary>
    /// <value>The <see cref="Type"/> of the collection items.</value>
    public Type CollectionItemType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the collection type is a multidimensional array.
    /// </summary>
    /// <value><c>true</c> if the collection type is a multidimensional array; otherwise, <c>false</c>.</value>
    public bool IsMultidimensionalArray { get; private set; }

    private readonly bool _isCollectionItemTypeNullableType;
    private readonly Type _genericCollectionDefinitionType;

    private Type _genericWrapperType;
    private MethodCall<object, object> _genericWrapperCreator;
    private Func<object> _genericTemporaryCollectionCreator;

    internal bool ShouldCreateWrapper { get; private set; }
    internal bool CanDeserialize { get; private set; }
    internal ConstructorInfo ParametrizedConstructor { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonArrayContract(Type underlyingType)
      : base(underlyingType)
    {
      ContractType = JsonContractType.Array;

      bool canDeserialize;

      Type tempCollectionType;
      if (CreatedType.IsArray)
      {
        CollectionItemType = ReflectionUtils.GetCollectionItemType(UnderlyingType);
        IsReadOnlyOrFixedSize = true;
        _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

        canDeserialize = true;
        IsMultidimensionalArray = (UnderlyingType.IsArray && UnderlyingType.GetArrayRank() > 1);
      }
      else if (typeof(IList).IsAssignableFrom(underlyingType))
      {
        if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
          CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        else
          CollectionItemType = ReflectionUtils.GetCollectionItemType(underlyingType);

        if (underlyingType == typeof (IList))
          CreatedType = typeof (List<object>);

        if (CollectionItemType != null)
          ParametrizedConstructor = CollectionUtils.ResolveEnumableCollectionConstructor(underlyingType, CollectionItemType);

        IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyCollection<>));
        canDeserialize = true;
      }
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
      {
        CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];

        if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(ICollection<>))
          || ReflectionUtils.IsGenericDefinition(underlyingType, typeof(IList<>)))
          CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);

#if !(NET20 || NET35 || PORTABLE40)
        if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(ISet<>)))
          CreatedType = typeof(HashSet<>).MakeGenericType(CollectionItemType);
#endif

        ParametrizedConstructor = CollectionUtils.ResolveEnumableCollectionConstructor(underlyingType, CollectionItemType);
        canDeserialize = true;
        ShouldCreateWrapper = true;
      }
#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE40)
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof (IReadOnlyCollection<>), out tempCollectionType))
      {
        CollectionItemType = underlyingType.GetGenericArguments()[0];

        if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof (IReadOnlyCollection<>))
          || ReflectionUtils.IsGenericDefinition(underlyingType, typeof (IReadOnlyList<>)))
          CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(CollectionItemType);

        _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
        ParametrizedConstructor = CollectionUtils.ResolveEnumableCollectionConstructor(CreatedType, CollectionItemType);
        IsReadOnlyOrFixedSize = true;
        canDeserialize = (ParametrizedConstructor != null);
      }
#endif
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof (IEnumerable<>), out tempCollectionType))
      {
        CollectionItemType = tempCollectionType.GetGenericArguments()[0];

        if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IEnumerable<>)))
          CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);

        ParametrizedConstructor = CollectionUtils.ResolveEnumableCollectionConstructor(underlyingType, CollectionItemType);

        if (underlyingType.IsGenericType() && underlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
          _genericCollectionDefinitionType = tempCollectionType;

          IsReadOnlyOrFixedSize = false;
          ShouldCreateWrapper = false;
          canDeserialize = true;
        }
        else
        {
          _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

          IsReadOnlyOrFixedSize = true;
          ShouldCreateWrapper = true;
          canDeserialize = (ParametrizedConstructor != null);
        }
      }
      else
      {
        // types that implement IEnumerable and nothing else
        canDeserialize = false;
        ShouldCreateWrapper = true;
      }

      CanDeserialize = canDeserialize;

      if (CollectionItemType != null)
        _isCollectionItemTypeNullableType = ReflectionUtils.IsNullableType(CollectionItemType);

#if (NET20 || NET35)
      // bug in .NET 2.0 & 3.5 that List<Nullable<T>> throws an error when adding null via IList.Add(object)
      // wrapper will handle calling Add(T) instead
      if (_isCollectionItemTypeNullableType
        && (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(List<>), out tempCollectionType)
        || (CreatedType.IsArray && !IsMultidimensionalArray)))
      {
        ShouldCreateWrapper = true;
      }
#endif
    }

    internal IWrappedCollection CreateWrapper(object list)
    {
      if (_genericWrapperCreator == null)
      {
        _genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(CollectionItemType);

        Type constructorArgument;

        if (ReflectionUtils.InheritsGenericDefinition(_genericCollectionDefinitionType, typeof(List<>))
          || _genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
          constructorArgument = typeof(ICollection<>).MakeGenericType(CollectionItemType);
        else
          constructorArgument = _genericCollectionDefinitionType;

        ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { constructorArgument });
        _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(genericWrapperConstructor);
      }
      
      return (IWrappedCollection) _genericWrapperCreator(null, list);
    }

    internal IList CreateTemporaryCollection()
    {
      if (_genericTemporaryCollectionCreator == null)
      {
        // multidimensional array will also have array instances in it
        Type collectionItemType = (IsMultidimensionalArray) ? typeof (object) : CollectionItemType;
        Type temporaryListType = typeof(List<>).MakeGenericType(collectionItemType);
        _genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryListType);
      }

      return (IList)_genericTemporaryCollectionCreator();
    }
  }
}