using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class ErrorHandlingAttribute
  {
    public class Employee
    {
      private List<string> _roles;

      public string Name { get; set; }
      public int Age { get; set; }
      public List<string> Roles
      {
        get
        {
          if (_roles == null)
            throw new Exception("Roles not loaded!");

          return _roles;
        }
        set { _roles = value; }
      }
      public string Title { get; set; }

      [OnError]
      internal void OnError(StreamingContext context, ErrorContext errorContext)
      {
        errorContext.Handled = true;
      }
    }

    public void Example()
    {
      Employee person = new Employee
      {
        Name = "George Michael Bluth",
        Age = 16,
        Roles = null,
        Title = "Mister Manager"
      };

      string json = JsonConvert.SerializeObject(person, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "Name": "George Michael Bluth",
      //   "Age": 16,
      //   "Title": "Mister Manager"
      // }
    }
  }
}
