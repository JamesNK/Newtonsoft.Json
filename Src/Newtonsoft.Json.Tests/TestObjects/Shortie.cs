using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class Shortie
  {
    public string Original { get; set; }
    public string Shortened { get; set; }
    public string Short { get; set; }
    public ShortieException Error { get; set; }
  }

  public class ShortieException
  {
    public int Code { get; set; }
    public string ErrorMessage { get; set; }
  }
}