using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class PrivateJsonConstructorArrayObject : IEnumerable<int>
    {
        private List<int> _items;

        public PrivateJsonConstructorArrayObject()
        {
            _items = new List<int>();
        }

        [JsonConstructor]
        private PrivateJsonConstructorArrayObject(IEnumerable<int> items)
        {
            _items = new List<int>(items);
        }

        public void Add(int item)
        {
            _items.Add(item);
        }

        public IEnumerator<int> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}