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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonArrayContract : JsonContract
  {
    internal Type CollectionTypeToCreate { get; private set; }
    internal Type CollectionItemType { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonArrayContract(Type underlyingType)
      : base(underlyingType)
    {
      CollectionItemType = ReflectionUtils.GetCollectionItemType(UnderlyingType);

      if (IsTypeGenericCollectionInterface(UnderlyingType))
      {
        Type itemType = ReflectionUtils.GetCollectionItemType(UnderlyingType);
        CollectionTypeToCreate = ReflectionUtils.MakeGenericType(typeof(List<>), itemType);
      }
      else
      {
        CollectionTypeToCreate = UnderlyingType;
      }
    }

    private bool IsTypeGenericCollectionInterface(Type type)
    {
      if (!type.IsGenericType)
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof(IList<>)
              || genericDefinition == typeof(ICollection<>)
              || genericDefinition == typeof(IEnumerable<>));
    }
  }
}