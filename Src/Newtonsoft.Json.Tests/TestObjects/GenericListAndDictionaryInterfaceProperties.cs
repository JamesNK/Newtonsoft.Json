using System.Collections.Generic;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class GenericListAndDictionaryInterfaceProperties
  {
    public IEnumerable<int> IEnumerableProperty { get; set; }
    public IList<int> IListProperty { get; set; }
    public IDictionary<string, int> IDictionaryProperty { get; set; }
  }
}