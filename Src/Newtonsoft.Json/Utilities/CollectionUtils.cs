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
using System.Linq;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal static class CollectionUtils
  {
    public static IEnumerable<T> CastValid<T>(this IEnumerable enumerable)
    {
      ValidationUtils.ArgumentNotNull(enumerable, "enumerable");

      return enumerable.Cast<object>().Where(o => o is T).Cast<T>();
    }

    public static List<T> CreateList<T>(params T[] values)
    {
      return new List<T>(values);
    }

    /// <summary>
    /// Determines whether the collection is null or empty.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>
    /// 	<c>true</c> if the collection is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(ICollection collection)
    {
      if (collection != null)
      {
        return (collection.Count == 0);
      }
      return true;
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
    /// Determines whether the collection is null, empty or its contents are uninitialized values.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>
    /// 	<c>true</c> if the collection is null or empty or its contents are uninitialized values; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmptyOrDefault<T>(IList<T> list)
    {
      if (IsNullOrEmpty<T>(list))
        return true;

      return ReflectionUtils.ItemsUnitializedValue<T>(list);
    }

    /// <summary>
    /// Makes a slice of the specified list in between the start and end indexes.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="start">The start index.</param>
    /// <param name="end">The end index.</param>
    /// <returns>A slice of the list.</returns>
    public static IList<T> Slice<T>(IList<T> list, int? start, int? end)
    {
      return Slice<T>(list, start, end, null);
    }

    /// <summary>
    /// Makes a slice of the specified list in between the start and end indexes,
    /// getting every so many items based upon the step.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="start">The start index.</param>
    /// <param name="end">The end index.</param>
    /// <param name="step">The step.</param>
    /// <returns>A slice of the list.</returns>
    public static IList<T> Slice<T>(IList<T> list, int? start, int? end, int? step)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      if (step == 0)
        throw new ArgumentException("Step cannot be zero.", "step");

      List<T> slicedList = new List<T>();

      // nothing to slice
      if (list.Count == 0)
        return slicedList;

      // set defaults for null arguments
      int s = step ?? 1;
      int startIndex = start ?? 0;
      int endIndex = end ?? list.Count;

      // start from the end of the list if start is negitive
      startIndex = (startIndex < 0) ? list.Count + startIndex : startIndex;

      // end from the start of the list if end is negitive
      endIndex = (endIndex < 0) ? list.Count + endIndex : endIndex;

      // ensure indexes keep within collection bounds
      startIndex = Math.Max(startIndex, 0);
      endIndex = Math.Min(endIndex, list.Count - 1);

      // loop between start and end indexes, incrementing by the step
      for (int i = startIndex; i < endIndex; i += s)
      {
        slicedList.Add(list[i]);
      }

      return slicedList;
    }


    /// <summary>
    /// Group the collection using a function which returns the key.
    /// </summary>
    /// <param name="source">The source collection to group.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <returns>A Dictionary with each key relating to a list of objects in a list grouped under it.</returns>
    public static Dictionary<K, List<V>> GroupBy<K, V>(ICollection<V> source, Func<V, K> keySelector)
    {
      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      Dictionary<K, List<V>> groupedValues = new Dictionary<K, List<V>>();

      foreach (V value in source)
      {
        // using delegate to get the value's key
        K key = keySelector(value);
        List<V> groupedValueList;

        // add a list for grouped values if the key is not already in Dictionary
        if (!groupedValues.TryGetValue(key, out groupedValueList))
        {
          groupedValueList = new List<V>();
          groupedValues.Add(key, groupedValueList);
        }

        groupedValueList.Add(value);
      }

      return groupedValues;
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

    public static List<T> Distinct<T>(List<T> collection)
    {
      List<T> distinctList = new List<T>();

      foreach (T value in collection)
      {
        if (!distinctList.Contains(value))
          distinctList.Add(value);
      }

      return distinctList;
    }

    public static List<List<T>> Flatten<T>(params IList<T>[] lists)
    {
      List<List<T>> flattened = new List<List<T>>();
      Dictionary<int, T> currentList = new Dictionary<int, T>();

      Recurse<T>(new List<IList<T>>(lists), 0, currentList, flattened);

      return flattened;
    }

    private static void Recurse<T>(IList<IList<T>> global, int current, Dictionary<int, T> currentSet, List<List<T>> flattenedResult)
    {
      IList<T> currentArray = global[current];

      for (int i = 0; i < currentArray.Count; i++)
      {
        currentSet[current] = currentArray[i];

        if (current == global.Count - 1)
        {
          List<T> items = new List<T>();

          for (int k = 0; k < currentSet.Count; k++)
          {
            items.Add(currentSet[k]);
          }

          flattenedResult.Add(items);
        }
        else
        {
          Recurse(global, current + 1, currentSet, flattenedResult);
        }
      }
    }

    public static List<T> CreateList<T>(ICollection collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      T[] array = new T[collection.Count];
      collection.CopyTo(array, 0);

      return new List<T>(array);
    }

    public static bool ListEquals<T>(IList<T> a, IList<T> b)
    {
      if (a == null || b == null)
        return (a == null && b == null);

      if (a.Count != b.Count)
        return false;

      EqualityComparer<T> comparer = EqualityComparer<T>.Default;

      for (int i = 0; i < a.Count; i++)
      {
        if (!comparer.Equals(a[i], b[i]))
          return false;
      }

      return true;
    }

    #region GetSingleItem
    public static bool TryGetSingleItem<T>(IList<T> list, out T value)
    {
      return TryGetSingleItem<T>(list, false, out value);
    }

    public static bool TryGetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty, out T value)
    {
      return MiscellaneousUtils.TryAction<T>(delegate { return GetSingleItem(list, returnDefaultIfEmpty); }, out value);
    }

    public static T GetSingleItem<T>(IList<T> list)
    {
      return GetSingleItem<T>(list, false);
    }

    public static T GetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty)
    {
      if (list.Count == 1)
        return list[0];
      else if (returnDefaultIfEmpty && list.Count == 0)
        return default(T);
      else
        throw new Exception("Expected single {0} in list but got {1}.".FormatWith(CultureInfo.InvariantCulture, typeof(T), list.Count));
    }
    #endregion

    public static IList<T> Minus<T>(IList<T> list, IList<T> minus)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      List<T> result = new List<T>(list.Count);
      foreach (T t in list)
      {
        if (minus == null || !minus.Contains(t))
          result.Add(t);
      }

      return result;
    }

    public static T[] CreateArray<T>(IEnumerable<T> enumerable)
    {
      ValidationUtils.ArgumentNotNull(enumerable, "enumerable");

      if (enumerable is T[])
        return (T[])enumerable;

      List<T> tempList = new List<T>(enumerable);
      return tempList.ToArray();
    }

    public static IList CreateGenericList(Type listType)
    {
      ValidationUtils.ArgumentNotNull(listType, "listType");

      return (IList)ReflectionUtils.CreateGeneric(typeof(List<>), listType);
    }

    public static IDictionary CreateGenericDictionary(Type keyType, Type valueType)
    {
      ValidationUtils.ArgumentNotNull(keyType, "keyType");
      ValidationUtils.ArgumentNotNull(valueType, "valueType");

      return (IDictionary)ReflectionUtils.CreateGeneric(typeof(Dictionary<,>), keyType, valueType);
    }

    public static bool IsListType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

      if (type.IsArray)
        return true;
      if (typeof(IList).IsAssignableFrom(type))
        return true;
      if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IList<>)))
        return true;

      return false;
    }

    public static bool IsCollectionType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

      if (type.IsArray)
        return true;
      if (typeof(ICollection).IsAssignableFrom(type))
        return true;
      if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(ICollection<>)))
        return true;

      return false;
    }

    public static bool IsDictionaryType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

      if (typeof(IDictionary).IsAssignableFrom(type))
        return true;
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
        throw new Exception("Can not create ListWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, list.GetType()));
      }
    }
    public static IWrappedList CreateListWrapper(object list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      Type listDefinition;
      if (ReflectionUtils.ImplementsGenericDefinition(list.GetType(), typeof(IList<>), out listDefinition))
      {
        Type collectionItemType = ReflectionUtils.GetCollectionItemType(listDefinition);

        // Activator.CreateInstance throws AmbiguousMatchException. Manually invoke constructor
        Func<Type, IList<object>, object> instanceCreator = (t, a) =>
        {
          ConstructorInfo c = t.GetConstructor(new[] {listDefinition});
          return c.Invoke(new[] { list });
        };

        return (IWrappedList)ReflectionUtils.CreateGeneric(typeof(ListWrapper<>), new[] { collectionItemType }, instanceCreator, list);
      }
      else if (list is IList)
      {
        return new ListWrapper<object>((IList)list);
      }
      else
      {
        throw new Exception("Can not create ListWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, list.GetType()));
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
      else if (dictionary is IDictionary)
      {
        return new DictionaryWrapper<object, object>((IDictionary)dictionary);
      }
      else
      {
        throw new Exception("Can not create DictionaryWrapper for type {0}.".FormatWith(CultureInfo.InvariantCulture, dictionary.GetType()));
      }
    }

    public static IList CreateAndPopulateList(Type listType, Action<IList, bool> populateList)
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
        list = (IList)CreateGenericList(readOnlyCollectionContentsType);
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
      else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IList<>))
      {
        list = CollectionUtils.CreateGenericList(ReflectionUtils.GetCollectionItemType(listType));
      }
      else
      {
        list = null;
      }

      if (list == null)
        throw new Exception("Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, listType));

      populateList(list, isReadOnlyOrFixedSize);

      // create readonly and fixed sized collections using the temporary list
      if (isReadOnlyOrFixedSize)
      {
        if (listType.IsArray)
          list = ToArray(((List<object>)list).ToArray(), ReflectionUtils.GetCollectionItemType(listType));
        else if (ReflectionUtils.InheritsGenericDefinition(listType, typeof(ReadOnlyCollection<>)))
          list = (IList)ReflectionUtils.CreateInstance(listType, list);
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

    public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values)
    {
      return list.AddRangeDistinct(values, EqualityComparer<T>.Default);
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
  }
}