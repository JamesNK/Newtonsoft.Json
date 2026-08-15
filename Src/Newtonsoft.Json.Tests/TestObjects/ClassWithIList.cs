using System.Collections.Generic;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class ClassWithIList
    {
        private string foo;
        private readonly IList<long> bar;

        public ClassWithIList()
        {
            bar = new List<long>();
        }

        public ClassWithIList(string _foo, IList<long> _bar)
        {
            foo = _foo;
            bar = _bar;
        }

        public string Foo
        {
            get { return foo; }
            set { foo = value; }
        }

        public IList<long> Bar
        {
            get { return bar; }
        }
    }
}
