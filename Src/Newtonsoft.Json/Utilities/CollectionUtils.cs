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
using System.Text;
using System.Collections;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
  internal static class CollectionUtils
  {
    public static IEnumerable<T> CastValid<T>(this IEnumerable enumerable)
    {
      ValidationUtils.ArgumentNotNull(enumerable, "enumerable");

      return enumerable.Cast<object>().Where(o => o is T).Cast<T>();
    }

    /// <summary>
    /// Determines whether the collection is null or empty.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>
    /// 	<c>true</c> if the collection is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty<T>(ICollection<T> collection)
    {
      if (collection != null)
      {
        return (collection.Count == 0);
      }
      return true;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the specified generic IList.
    /// </summary>
    /// <param name="initial">The list to add to.</param>
    /// <param name="collection">The collection of elements to add.</param>
    public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
    {
      if (initial == null)
        throw new ArgumentNullException("initial");

      if (collection == null)
        return;

      foreach (T value in collection)
      {
        initial.Add(value);
      }
    }

    public static void AddRange(this IList initial, IEnumerable collection)
    {
      ValidationUtils.ArgumentNotNull(initial, "initial");

      ListWrapper<object> wrapper = new ListWrapper<object>(initial);
      wrapper.AddRange(collection.Cast<object>());
    }

    public static IList CreateGenericList(Type listType)
    {
      ValidationUtils.ArgumentNotNull(listType, "listType");

      return (IList)ReflectionUtils.CreateGeneric(typeof(List<>), listType);
    }

    public static bool IsDictionaryType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

#if !NETFX_CORE
      if (typeof(IDictionary).IsAssignableFrom(type))
        return true;
#endif
      if (ReflectionUtils.ImplementsGenericDefinition(type, typeof (IDictionary<,>)))
        return true;

      return false;
    }

    public static IWrappedCollection CreateCollectionWrapper(object list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      Type collectionDefinition;
      if (ReflectionUtils.ImplementsGenericDefinition(list.GetType(), typeof(ICollection<>), out collectionDefinition))
      {
        Type collectionItemType = ReflectionUtils.GetCollectionItemType(collectionDefinition);

        // Activator.CreateInstance throws AmbiguousMatchException. Manually invoke constructor
        Func<Type, IList<object>, object> instanceCreator = (t, a) =>
        {
          ConstructorInfo c = t.GetConstructor(new[] { collectionDefinition });
          return c.Invoke(new[] { list });
        };

        return (IWrappedCollection)ReflectionUtils.CreateGeneric(typeof(CollectionWrapper<>), new[] { collectionItemType }, instanceCreator, list);
      }
      else if (list is IList)
      {
        return new CollectionWrapper<object>((IList)list);
      }
      else
      {
        throw new ArgumentException("Can not create ListWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, list.GetType()), "list");
      }
    }

    public static IWrappedDictionary CreateDictionaryWrapper(object dictionary)
    {
      ValidationUtils.ArgumentNotNull(dictionary, "dictionary");

      Type dictionaryDefinition;
      if (ReflectionUtils.ImplementsGenericDefinition(dictionary.GetType(), typeof(IDictionary<,>), out dictionaryDefinition))
      {
        Type dictionaryKeyType = ReflectionUtils.GetDictionaryKeyType(dictionaryDefinition);
        Type dictionaryValueType = ReflectionUtils.GetDictionaryValueType(dictionaryDefinition);

        // Activator.CreateInstance throws AmbiguousMatchException. Manually invoke constructor
        Func<Type, IList<object>, object> instanceCreator = (t, a) =>
        {
          ConstructorInfo c = t.GetConstructor(new[] { dictionaryDefinition });
          return c.Invoke(new[] { dictionary });
        };

        return (IWrappedDictionary)ReflectionUtils.CreateGeneric(typeof(DictionaryWrapper<,>), new[] { dictionaryKeyType, dictionaryValueType }, instanceCreator, dictionary);
      }
#if !NETFX_CORE
      else if (dictionary is IDictionary)
      {
        return new DictionaryWrapper<object, object>((IDictionary)dictionary);
      }
#endif
      else
      {
        throw new ArgumentException("Can not create DictionaryWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, dictionary.GetType()), "dictionary");
      }
    }

    public static object CreateAndPopulateList(Type listType, Action<IList, bool> populateList)
    {
      ValidationUtils.ArgumentNotNull(listType, "listType");
      ValidationUtils.ArgumentNotNull(populateList, "populateList");

      IList list;
      Type collectionType;
      bool isReadOnlyOrFixedSize = false;

      if (listType.IsArray)
      {
        // have to use an arraylist when creating array
        // there is no way to know the size until it is finised
        list = new List<object>();
        isReadOnlyOrFixedSize = true;
      }
      else if (ReflectionUtils.InheritsGenericDefinition(listType, typeof(ReadOnlyCollection<>), out collectionType))
      {
        Type readOnlyCollectionContentsType = collectionType.GetGenericArguments()[0];
        Type genericEnumerable = ReflectionUtils.MakeGenericType(typeof(IEnumerable<>), readOnlyCollectionContentsType);
        bool suitableConstructor = false;

        foreach (ConstructorInfo constructor in listType.GetConstructors())
        {
          IList<ParameterInfo> parameters = constructor.GetParameters();

          if (parameters.Count == 1)
          {
            if (genericEnumerable.IsAssignableFrom(parameters[0].ParameterType))
            {
              suitableConstructor = true;
              break;
            }
          }
        }

        if (!suitableConstructor)
          throw new Exception("Read-only type {0} does not have a public constructor that takes a type that implements {1}.".FormatWith(CultureInfo.InvariantCulture, listType, genericEnumerable));

        // can't add or modify a readonly list
        // use List<T> and convert once populated
        list = CreateGenericList(readOnlyCollectionContentsType);
        isReadOnlyOrFixedSize = true;
      }
      else if (typeof(IList).IsAssignableFrom(listType))
      {
        if (ReflectionUtils.IsInstantiatableType(listType))
          list = (IList)Activator.CreateInstance(listType);
        else if (listType == typeof(IList))
          list = new List<object>();
        else
          list = null;
      }
      else if (ReflectionUtils.ImplementsGenericDefinition(listType, typeof(ICollection<>)))
      {
        if (ReflectionUtils.IsInstantiatableType(listType))
          list = CreateCollectionWrapper(Activator.CreateInstance(listType));
        else
          list = null;
      }
      else
      {
        list = null;
      }

      if (list == null)
        throw new InvalidOperationException("Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, listType));

      populateList(list, isReadOnlyOrFixedSize);

      // create readonly and fixed sized collections using the temporary list
      if (isReadOnlyOrFixedSize)
      {
        if (listType.IsArray)
          list = ToArray(((List<object>)list).ToArray(), ReflectionUtils.GetCollectionItemType(listType));
        else if (ReflectionUtils.InheritsGenericDefinition(listType, typeof(ReadOnlyCollection<>)))
          list = (IList)ReflectionUtils.CreateInstance(listType, list);
      }
      else if (list is IWrappedCollection)
      {
        return ((IWrappedCollection) list).UnderlyingCollection;
      }

      return list;
    }

    public static Array ToArray(Array initial, Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      Array destinationArray = Array.CreateInstance(type, initial.Length);
      Array.Copy(initial, 0, destinationArray, 0, initial.Length);
      return destinationArray;
    }

    public static bool AddDistinct<T>(this IList<T> list, T value)
    {
      return list.AddDistinct(value, EqualityComparer<T>.Default);
    }

    public static bool AddDistinct<T>(this IList<T> list, T value, IEqualityComparer<T> comparer)
    {
      if (list.ContainsValue(value, comparer))
        return false;

      list.Add(value);
      return true;
    }

    // this is here because LINQ Bridge doesn't support Contains with IEqualityComparer<T>
    public static bool ContainsValue<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
    {
      if (comparer == null)
        comparer = EqualityComparer<TSource>.Default;

      if (source == null)
        throw new ArgumentNullException("source");

      foreach (TSource local in source)
      {
        if (comparer.Equals(local, value))
          return true;
      }

      return false;
    }

    public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer)
    {
      bool allAdded = true;
      foreach (T value in values)
      {
        if (!list.AddDistinct(value, comparer))
          allAdded = false;
      }

      return allAdded;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
      int index = 0;
      foreach (T value in collection)
      {
        if (predicate(value))
          return index;

        index++;
      }

      return -1;
    }

    /// <summary>
    /// Returns the index of the first occurrence in a sequence by using a specified IEqualityComparer.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="list">A sequence in which to locate a value.</param>
    /// <param name="value">The object to locate in the sequence</param>
    /// <param name="comparer">An equality comparer to compare values.</param>
    /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
    public static int IndexOf<TSource>(this IEnumerable<TSource> list, TSource value, IEqualityComparer<TSource> comparer)
    {
      int index = 0;
      foreach (TSource item in list)
      {
        if (comparer.Equals(item, value))
        {
          return index;
        }
        index++;
      }
      return -1;
    }
  }
}