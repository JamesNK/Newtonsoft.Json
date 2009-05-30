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
using System.Text;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// A collection of <see cref="JsonMemberMapping"/> objects.
  /// </summary>
  public class JsonMemberMappingCollection : KeyedCollection<string, JsonMemberMapping>
  {
    /// <summary>
    /// When implemented in a derived class, extracts the key from the specified element.
    /// </summary>
    /// <param name="item">The element from which to extract the key.</param>
    /// <returns>The key for the specified element.</returns>
    protected override string GetKeyForItem(JsonMemberMapping item)
    {
      return item.PropertyName;
    }

    /// <summary>
    /// Adds a <see cref="JsonMemberMapping"/> object.
    /// </summary>
    /// <param name="memberMapping">The member mapping to add to the collection.</param>
    public void AddMapping(JsonMemberMapping memberMapping)
    {
      if (Contains(memberMapping.PropertyName))
      {
        // don't overwrite existing mapping with ignored mapping
        if (memberMapping.Ignored)
          return;

        JsonMemberMapping existingMemberMapping = this[memberMapping.PropertyName];

        if (!existingMemberMapping.Ignored)
        {
          throw new JsonSerializationException(
            "A member with the name '{0}' already exists on {1}. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, memberMapping.PropertyName, memberMapping.Member.DeclaringType));
        }
        else
        {
          // remove ignored mapping so it can be replaced in collection
          Remove(existingMemberMapping);
        }
      }

      Add(memberMapping);
    }

    /// <summary>
    /// Tries to get the closest matching <see cref="JsonMemberMapping"/> object.
    /// First attempts to get an exact case match of propertyName and then
    /// a case insensitive match.
    /// </summary>
    /// <param name="propertyName">Name of the mapping.</param>
    /// <param name="memberMapping">The member mapping.</param>
    /// <returns></returns>
    public bool TryGetClosestMatchMapping(string propertyName, out JsonMemberMapping memberMapping)
    {
      if (TryGetMapping(propertyName, StringComparison.Ordinal, out memberMapping))
        return true;
      if (TryGetMapping(propertyName, StringComparison.OrdinalIgnoreCase, out memberMapping))
        return true;

      return false;
    }

    /// <summary>
    /// Tries to get a member mapping by property name.
    /// </summary>
    /// <param name="propertyName">The property name of the member mapping to get.</param>
    /// <param name="comparisonType">Type property name compare string comparison.</param>
    /// <param name="memberMapping">The member mapping.</param>
    /// <returns></returns>
    public bool TryGetMapping(string propertyName, StringComparison comparisonType, out JsonMemberMapping memberMapping)
    {
      foreach (JsonMemberMapping mapping in this)
      {
        if (string.Compare(propertyName, mapping.PropertyName, comparisonType) == 0)
        {
          memberMapping = mapping;
          return true;
        }
      }

      memberMapping = default(JsonMemberMapping);
      return false;
    }
  }
}