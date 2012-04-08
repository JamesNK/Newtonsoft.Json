using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.LinqToSql
{
  [MetadataType(typeof(RoleMetadata))]
  public partial class Role
  {
    public class RoleMetadata
    {
      [JsonConverter(typeof(GuidByteArrayConverter))]
      public Guid RoleId { get; set; }
      [JsonIgnore]
      public EntitySet<PersonRole> PersonRoles { get; set; }
    }
  }
}