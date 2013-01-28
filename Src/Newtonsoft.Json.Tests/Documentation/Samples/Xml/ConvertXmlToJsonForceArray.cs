using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
  public class ConvertXmlToJsonForceArray
  {
    public void Example()
    {
      #region Usage
      string xml = @"<person id='1'>
        <name>Alan</name>
        <url>http://www.google.com</url>
        <role>Admin1</role>
      </person>";

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(xml);

      string json = JsonConvert.SerializeXmlNode(doc);

      Console.WriteLine(json);
      // {
      //   "person": {
      //     "@id": "1",
      //     "name": "Alan",
      //     "url": "http://www.google.com",
      //     "role": "Admin1"
      //   }
      // }

      xml = @"<person xmlns:json='http://james.newtonking.com/projects/json' id='1'>
        <name>Alan</name>
        <url>http://www.google.com</url>
        <role json:Array='true'>Admin</role>
      </person>";

      doc = new XmlDocument();
      doc.LoadXml(xml);

      json = JsonConvert.SerializeXmlNode(doc);

      Console.WriteLine(json);
      // {
      //   "person": {
      //     "@id": "1",
      //     "name": "Alan",
      //     "url": "http://www.google.com",
      //     "role": [
      //       "Admin"
      //     ]
      //   }
      // }
      #endregion
    }
  }
}