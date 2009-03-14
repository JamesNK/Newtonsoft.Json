using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class GenericListAndDictionaryInterfaceProperties
  {
    public IEnumerable<int> IEnumerableProperty { get; set; }
    public IList<int> IListProperty { get; set; }
    public IDictionary<string, int> IDictionaryProperty { get; set; }
  }
}