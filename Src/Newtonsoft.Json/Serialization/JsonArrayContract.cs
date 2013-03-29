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
    internal Type TemporaryCollectionType { get; private set; }
    internal bool IsReadOnlyOrFixedSize { get; private set; }

    private ConstructorInfo ResolveReadOnlyCollectionConstructor(Type readOnlyCollectionType, Type collectionTypeType)
    {
      Type genericEnumerable = ReflectionUtils.MakeGenericType(typeof(IEnumerable<>), collectionTypeType);

      foreach (ConstructorInfo constructor in readOnlyCollectionType.GetConstructors())
      {
        IList<ParameterInfo> parameters = constructor.GetParameters();

        if (parameters.Count == 1)
        {
          if (genericEnumerable.IsAssignableFrom(parameters[0].ParameterType))
            return constructor;
        }
      }

      return null;
    }

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

        canDeserialize = true;
        TemporaryCollectionType = typeof (List<object>);
      }
      else if (ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyCollection<>), out tempCollectionType))
      {
        CollectionItemType = tempCollectionType.GetGenericArguments()[0];
        IsReadOnlyOrFixedSize = true;

        canDeserialize = (ResolveReadOnlyCollectionConstructor(underlyingType, CollectionItemType) != null);
        TemporaryCollectionType = ReflectionUtils.MakeGenericType(typeof(List<>), CollectionItemType);
      }
      else if (typeof(IList).IsAssignableFrom(underlyingType))
      {
        if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
          CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        else
          CollectionItemType = ReflectionUtils.GetCollectionItemType(underlyingType);

        canDeserialize = true;
      }
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
      {
        CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        canDeserialize = true;
        ShouldCreateWrapper = true;
      }
#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE)
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof (IReadOnlyCollection<>), out tempCollectionType))
      {
        CollectionItemType = underlyingType.GetGenericArguments()[0];
        IsReadOnlyOrFixedSize = true;
        TemporaryCollectionType = ReflectionUtils.MakeGenericType(typeof(List<>), CollectionItemType);
        ShouldCreateWrapper = !typeof(IList).IsAssignableFrom(underlyingType);

        canDeserialize = (ResolveReadOnlyCollectionConstructor(underlyingType, CollectionItemType) != null
          || underlyingType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
          || underlyingType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
      }
#endif
      else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof (IEnumerable<>), out tempCollectionType))
      {
        CollectionItemType = tempCollectionType.GetGenericArguments()[0];

        if (underlyingType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
        {
          _genericCollectionDefinitionType = tempCollectionType;

          IsReadOnlyOrFixedSize = false;
          ShouldCreateWrapper = false;
          canDeserialize = true;
        }
        else
        {
          IsReadOnlyOrFixedSize = true;
          ShouldCreateWrapper = true;
          canDeserialize = (ResolveReadOnlyCollectionConstructor(underlyingType, CollectionItemType) != null);
          TemporaryCollectionType = ReflectionUtils.MakeGenericType(typeof(List<>), CollectionItemType);
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

      if (IsTypeGenericCollectionInterface(UnderlyingType))
        CreatedType = ReflectionUtils.MakeGenericType(typeof(List<>), CollectionItemType);
#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE)
      else if (IsTypeGenericReadOnlyCollectionInterface(UnderlyingType))
        CreatedType = ReflectionUtils.MakeGenericType(typeof(ReadOnlyCollection<>), CollectionItemType);
#endif
#if !(NET20 || NET35)
      else if (IsTypeGenericSetInterface(UnderlyingType))
        CreatedType = ReflectionUtils.MakeGenericType(typeof(HashSet<>), CollectionItemType);
#endif

#if (NET20 || NET35)
      // bug in .NET 2.0 & 3.5 that List<Nullable<T>> throws an error when adding null via IList.Add(object)
      // wrapper will handle calling Add(T) instead
      if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(List<>), out tempCollectionType))
      {
        Type tempCollectionItemType = tempCollectionType.GetGenericArguments()[0];
        if (ReflectionUtils.IsNullableType(tempCollectionItemType))
          ShouldCreateWrapper = true;
      }
#endif

      IsMultidimensionalArray = (UnderlyingType.IsArray && UnderlyingType.GetArrayRank() > 1);
    }

    internal IWrappedCollection CreateWrapper(object list)
    {
      if ((list is IList && (CollectionItemType == null || !_isCollectionItemTypeNullableType))
        || UnderlyingType.IsArray)
        return new CollectionWrapper<object>((IList)list);

      if (_genericCollectionDefinitionType != null)
      {
        EnsureGenericWrapperCreator();
        return (IWrappedCollection) _genericWrapperCreator(null, list);
      }
      else
      {
        IList values = ((IEnumerable) list).Cast<object>().ToList();

        if (CollectionItemType != null)
        {
          Array array = Array.CreateInstance(CollectionItemType, values.Count);
          for (int i = 0; i < values.Count; i++)
          {
            array.SetValue(values[i], i);
          }

          values = array;
        }

        return new CollectionWrapper<object>(values);
      }
    }

    internal IList CreateTemporaryCollection()
    {
      if (_genericTemporaryCollectionCreator == null)
        _genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(TemporaryCollectionType);

      return (IList)_genericTemporaryCollectionCreator();
    }

    private void EnsureGenericWrapperCreator()
    {
      if (_genericWrapperCreator == null)
      {
        _genericWrapperType = ReflectionUtils.MakeGenericType(typeof (CollectionWrapper<>), CollectionItemType);

        Type constructorArgument;

        if (ReflectionUtils.InheritsGenericDefinition(_genericCollectionDefinitionType, typeof(List<>))
          || _genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
          constructorArgument = ReflectionUtils.MakeGenericType(typeof(ICollection<>), CollectionItemType);
        else
          constructorArgument = _genericCollectionDefinitionType;

        ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { constructorArgument });
        _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(genericWrapperConstructor);
      }
    }

    private bool IsTypeGenericCollectionInterface(Type type)
    {
      if (!type.IsGenericType())
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof(IList<>)
              || genericDefinition == typeof(ICollection<>)
              || genericDefinition == typeof(IEnumerable<>));
    }

#if !(NET40 || NET35 || NET20 || SILVERLIGHT || WINDOWS_PHONE || PORTABLE)
    private bool IsTypeGenericReadOnlyCollectionInterface(Type type)
    {
      if (!type.IsGenericType())
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof (IReadOnlyCollection<>)
              || genericDefinition == typeof (IReadOnlyList<>));
    }
#endif

#if !(NET20 || NET35)
    private bool IsTypeGenericSetInterface(Type type)
    {
      if (!type.IsGenericType())
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof(ISet<>));
    }
#endif
  }
}