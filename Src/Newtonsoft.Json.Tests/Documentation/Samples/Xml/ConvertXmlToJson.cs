using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
  public class ConvertXmlToJson
  {
    public void Example()
    {
      #region Usage
      string xml = @"<?xml version='1.0' standalone='no'?>
      <root>
        <person id='1'>
        <name>Alan</name>
        <url>http://www.google.com</url>
        </person>
        <person id='2'>
        <name>Louis</name>
        <url>http://www.yahoo.com</url>
        </person>
      </root>";

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(xml);

      string json = JsonConvert.SerializeXmlNode(doc);

      Console.WriteLine(json);
      // {
      //   "?xml": {
      //     "@version": "1.0",
      //     "@standalone": "no"
      //   },
      //   "root": {
      //     "person": [
      //       {
      //         "@id": "1",
      //         "name": "Alan",
      //         "url": "http://www.google.com"
      //       },
      //       {
      //         "@id": "2",
      //         "name": "Louis",
      //         "url": "http://www.yahoo.com"
      //       }
      //     ]
      //   }
      // }
      #endregion
    }
  }
}