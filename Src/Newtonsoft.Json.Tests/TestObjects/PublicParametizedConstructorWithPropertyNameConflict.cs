using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class PublicParametizedConstructorWithPropertyNameConflict
  {
    private readonly int _value;

    public PublicParametizedConstructorWithPropertyNameConflict(string name)
    {
      _value = Convert.ToInt32(name);
    }

    public int Name
    {
      get { return _value; }
    }
  }

  public class PublicParametizedConstructorWithPropertyNameConflictWithAttribute
  {
    private readonly int _value;

    public PublicParametizedConstructorWithPropertyNameConflictWithAttribute([JsonProperty("name")]string nameParameter)
    {
      _value = Convert.ToInt32(nameParameter);
    }

    public int Name
    {
      get { return _value; }
    }
  }
}
