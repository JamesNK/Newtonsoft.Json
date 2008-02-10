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
    public static void AddRange<T>(IList<T> initial, IEnumerable<T> collection)
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
        throw new Exception(string.Format("Expected single {0} in list but got {1}.", typeof(T), list.Count));
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

    public static object CreateGenericList(Type listType)
    {
      ValidationUtils.ArgumentNotNull(listType, "listType");

      return ReflectionUtils.CreateGeneric(typeof(List<>), listType);
    }

    public static bool IsListType(Type type)
    {
      ValidationUtils.ArgumentNotNull(type, "listType");

      if (type.IsArray)
        return true;
      else if (typeof(IList).IsAssignableFrom(type))
        return true;
      else if (ReflectionUtils.IsSubClass(type, typeof(IList<>)))
        return true;
      else
        return false;
    }

    public static IList CreateAndPopulateList(Type listType, Action<IList> populateList)
    {
      ValidationUtils.ArgumentNotNull(listType, "listType");
      ValidationUtils.ArgumentNotNull(populateList, "populateList");

      IList list;
      Type readOnlyCollectionType;
      bool isReadOnlyOrFixedSize = false;

      if (listType.IsArray)
      {
        // have to use an arraylist when creating array
        // there is no way to know the size until it is finised
        list = new ArrayList();
        isReadOnlyOrFixedSize = true;
      }
      else if (ReflectionUtils.IsSubClass(listType, typeof(ReadOnlyCollection<>), out readOnlyCollectionType))
      {
        Type readOnlyCollectionContentsType = readOnlyCollectionType.GetGenericArguments()[0];
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
          throw new Exception(string.Format("Readonly type {0} does not have a public constructor that takes a type that implements {1}.", listType, genericEnumerable));

        // can't add or modify a readonly list
        // use List<T> and convert once populated
        list = (IList)CreateGenericList(readOnlyCollectionContentsType);
        isReadOnlyOrFixedSize = true;
      }
      else if (typeof(IList).IsAssignableFrom(listType) && ReflectionUtils.IsInstantiatableType(listType))
      {
        list = (IList)Activator.CreateInstance(listType);
      }
      else
      {
        throw new Exception(string.Format("Cannot create and populate list type {0}.", listType));
      }

      populateList(list);

      // create readonly and fixed sized collections using the temporary list
      if (isReadOnlyOrFixedSize)
      {
        if (listType.IsArray)
          list = ((ArrayList)list).ToArray(ReflectionUtils.GetListItemType(listType));
        else if (ReflectionUtils.IsSubClass(listType, typeof(ReadOnlyCollection<>)))
          list = (IList)Activator.CreateInstance(listType, list);
      }

      return list;
    }
  }
}