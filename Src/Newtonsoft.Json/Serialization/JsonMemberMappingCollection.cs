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
  public class JsonMemberMappingCollection : KeyedCollection<string, JsonMemberMapping>
  {
    protected override string GetKeyForItem(JsonMemberMapping item)
    {
      return item.MappingName;
    }

    public void AddMapping(JsonMemberMapping memberMapping)
    {
      if (Contains(memberMapping.MappingName))
      {
        // don't overwrite existing mapping with ignored mapping
        if (memberMapping.Ignored)
          return;

        JsonMemberMapping existingMemberMapping = this[memberMapping.MappingName];

        if (!existingMemberMapping.Ignored)
        {
          throw new JsonSerializationException(
            "A member with the name '{0}' already exists on {1}. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, memberMapping.MappingName, memberMapping.Member.DeclaringType));
        }
        else
        {
          // remove ignored mapping so it can be replaced in collection
          Remove(existingMemberMapping);
        }
      }

      Add(memberMapping);
    }

    public bool TryGetClosestMatchMapping(string mappingName, out JsonMemberMapping memberMapping)
    {
      if (TryGetMapping(mappingName, StringComparison.Ordinal, out memberMapping))
        return true;
      if (TryGetMapping(mappingName, StringComparison.OrdinalIgnoreCase, out memberMapping))
        return true;

      return false;
    }

    public bool TryGetMapping(string mappingName, StringComparison comparisonType, out JsonMemberMapping memberMapping)
    {
      foreach (JsonMemberMapping mapping in this)
      {
        if (string.Compare(mappingName, mapping.MappingName, comparisonType) == 0)
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