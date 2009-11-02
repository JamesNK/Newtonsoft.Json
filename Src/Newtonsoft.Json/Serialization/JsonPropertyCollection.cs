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
  /// A collection of <see cref="JsonProperty"/> objects.
  /// </summary>
  public class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
  {
    private readonly JsonObjectContract _contract;

    public JsonPropertyCollection(JsonObjectContract contract)
    {
      ValidationUtils.ArgumentNotNull(contract, "contract");
      _contract = contract;
    }

    /// <summary>
    /// When implemented in a derived class, extracts the key from the specified element.
    /// </summary>
    /// <param name="item">The element from which to extract the key.</param>
    /// <returns>The key for the specified element.</returns>
    protected override string GetKeyForItem(JsonProperty item)
    {
      return item.PropertyName;
    }

    /// <summary>
    /// Adds a <see cref="JsonProperty"/> object.
    /// </summary>
    /// <param name="property">The property to add to the collection.</param>
    public void AddProperty(JsonProperty property)
    {
      if (Contains(property.PropertyName))
      {
        // don't overwrite existing property with ignored property
        if (property.Ignored)
          return;

        JsonProperty existingProperty = this[property.PropertyName];

        if (!existingProperty.Ignored)
          throw new JsonSerializationException(
            "A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _contract.UnderlyingType));

        // remove ignored property so it can be replaced in collection
        Remove(existingProperty);
      }

      Add(property);
    }

    /// <summary>
    /// Tries to get the closest matching <see cref="JsonProperty"/> object.
    /// First attempts to get an exact case match of propertyName and then
    /// a case insensitive match.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="property">A matching property if found.</param>
    /// <returns>A flag indicating whether a match was found.</returns>
    public bool TryGetClosestMatchProperty(string propertyName, out JsonProperty property)
    {
      if (TryGetProperty(propertyName, StringComparison.Ordinal, out property))
        return true;
      if (TryGetProperty(propertyName, StringComparison.OrdinalIgnoreCase, out property))
        return true;

      return false;
    }

    /// <summary>
    /// Tries to get a property by property name.
    /// </summary>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <param name="comparisonType">Type property name string comparison.</param>
    /// <param name="matchingProperty">A matching property if found.</param>
    /// <returns>A flag indicating whether a match was found.</returns>
    public bool TryGetProperty(string propertyName, StringComparison comparisonType, out JsonProperty matchingProperty)
    {
      foreach (JsonProperty property in this)
      {
        if (string.Compare(propertyName, property.PropertyName, comparisonType) == 0)
        {
          matchingProperty = property;
          return true;
        }
      }

      matchingProperty = default(JsonProperty);
      return false;
    }
  }
}