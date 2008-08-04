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

namespace Newtonsoft.Json
{
  internal class MemberMappingCollection : KeyedCollection<string, MemberMapping>
  {
    protected override string GetKeyForItem(MemberMapping item)
    {
      return item.MappingName;
    }

    public void AddMapping(MemberMapping memberMapping)
    {
      if (Contains(memberMapping.MappingName))
      {
        // don't overwrite existing mapping with ignored mapping
        if (memberMapping.Ignored)
          return;

        MemberMapping existingMemberMapping = this[memberMapping.MappingName];

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

    public bool TryGetMapping(string mappingName, out MemberMapping memberMapping)
    {
      if (Contains(mappingName))
      {
        memberMapping = this[mappingName];
        return true;
      }
      else
      {
        memberMapping = default(MemberMapping);
        return false;
      }
    }
  }
}