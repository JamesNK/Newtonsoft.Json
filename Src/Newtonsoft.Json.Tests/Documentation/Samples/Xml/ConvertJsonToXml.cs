using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
  public class ConvertJsonToXml
  {
    public void Example()
    {
      #region Usage
      string json = @"{
        '@Id': 1,
        'Email': 'james@example.com',
        'Active': true,
        'CreatedDate': '2013-01-20T00:00:00Z',
        'Roles': [
          'User',
          'Admin'
        ],
        'Team': {
          '@Id': 2,
          'Name': 'Software Developers',
          'Description': 'Creators of fine software products and services.'
        }
      }";

      XNode node = JsonConvert.DeserializeXNode(json, "Root");

      Console.WriteLine(node.ToString());
      // <Root Id="1">
      //   <Email>james@example.com</Email>
      //   <Active>true</Active>
      //   <CreatedDate>2013-01-20T00:00:00Z</CreatedDate>
      //   <Roles>User</Roles>
      //   <Roles>Admin</Roles>
      //   <Team Id="2">
      //     <Name>Software Developers</Name>
      //     <Description>Creators of fine software products and services.</Description>
      //   </Team>
      // </Root>
      #endregion
    }
  }
}