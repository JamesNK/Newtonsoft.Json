using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.LinqToSql
{
  [MetadataType(typeof(PersonMetadata))]
  public partial class Person
  {
    public class PersonMetadata
    {
      [JsonProperty("first_name")]
      public string FirstName { get; set; }
    }
  }
}