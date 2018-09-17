#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(NET20 || PORTABLE40)

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
    [TestFixture]
    public class ConvertJsonToXml : TestFixtureBase
    {
        [Test]
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

            StringAssert.AreEqual(@"<Root Id=""1"">
  <Email>james@example.com</Email>
  <Active>true</Active>
  <CreatedDate>2013-01-20T00:00:00Z</CreatedDate>
  <Roles>User</Roles>
  <Roles>Admin</Roles>
  <Team Id=""2"">
    <Name>Software Developers</Name>
    <Description>Creators of fine software products and services.</Description>
  </Team>
</Root>", node.ToString());
        }
    }
}

#endif