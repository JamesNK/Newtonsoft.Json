using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.LinqToSql
{
  [MetadataType(typeof(DepartmentMetadata))]
  public partial class Department
  {
    [JsonConverter(typeof(DepartmentConverter))]
    public class DepartmentMetadata
    {
    }
  }
}