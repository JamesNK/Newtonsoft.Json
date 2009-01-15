using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class JaggedArray
  {
    public string Before { get; set; }
    public int[][] Coordinates { get; set; }
    public string After { get; set; }
  }
}