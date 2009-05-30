using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class PrivateMembersClass
  {
    public PrivateMembersClass(string privateString, string internalString)
    {
      _privateString = privateString;
      _internalString = internalString;
    }

    public PrivateMembersClass()
    {
      i = default(int);
    }

    private string _privateString;
    private int i;
    internal string _internalString;
  }
}