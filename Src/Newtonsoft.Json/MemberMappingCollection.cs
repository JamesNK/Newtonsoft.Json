using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

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
            string.Format(
              "A member with the name '{0}' already exists on {1}. Use the JsonPropertyAttribute to specify another name.",
              memberMapping.MappingName, memberMapping.Member.DeclaringType));
        }
        else
        {
          // remove ignored mapping so it can be replaced in collection
          Remove(existingMemberMapping);
        }
      }

      Add(memberMapping);
    }
  }
}