using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class Movie
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Classification { get; set; }
    public string Studio { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public List<string> ReleaseCountries { get; set; }
  }
}
