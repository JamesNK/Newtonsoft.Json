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

using System.Collections.Generic;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class DictionaryInterfaceClass
  {
    public string Name { get; set; }
    public IDictionary<string, int> Dictionary { get; set; }
    public ICollection<int> Collection { get; set; }
    public EmployeeReference Employee { get; set; }
    public object Random { get; set; }

    public DictionaryInterfaceClass()
    {
      Dictionary = new Dictionary<string, int>
        {
          { "existing", 1 }
        };
      Collection = new List<int>
        {
          1,
          2,
          3
        };
      Employee = new EmployeeReference
        {
          Name = "EmployeeName!"
        };
    }
  }
}