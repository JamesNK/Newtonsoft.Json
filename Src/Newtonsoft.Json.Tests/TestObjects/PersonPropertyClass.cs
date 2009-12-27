using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class PersonPropertyClass
  {
    public Person Person { get; set; }

    public PersonPropertyClass()
    {
      Person = new WagePerson();
    }
  }
}