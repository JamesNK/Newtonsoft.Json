using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
  [XmlRoot("short")]
  [JsonObject(MemberSerialization.OptIn)]
  public class Shortie
  {
    [JsonProperty("original")]
    [XmlElement("original")]
    public String Original { get; set; }

    [JsonProperty("shortened")]
    [XmlElement("shortened")]
    public String Shortened { get; set; }

    [JsonProperty("short")]
    [XmlElement("short")]
    public String Short { get; set; }

    public ShortieException Error { get; set; }
  }

  public class ShortieException
  {
    [JsonProperty("code")]
    [XmlElement("code")]
    public int Code { get; set; }

    [JsonProperty("msg")]
    [XmlElement("msg")]
    public String ErrorMessage { get; set; }
  }
}