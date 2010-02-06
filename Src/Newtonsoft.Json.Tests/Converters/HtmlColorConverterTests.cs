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

#if !SILVERLIGHT && !PocketPC
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Converters
{
  public class HtmlColorConverterTests : TestFixtureBase
  {
    [Test]
    public void WriteJsonTest()
    {
      string json;
      
      json = JsonConvert.SerializeObject(Color.DarkGray, new HtmlColorConverter());
      Assert.AreEqual(@"""DarkGray""", json);
      
      json = JsonConvert.SerializeObject(Color.FromArgb(255, 1, 2, 3), new HtmlColorConverter());
      Assert.AreEqual(@"""#010203""", json);
    }

    [Test]
    public void ReadJsonTest()
    {
      string json = @"""#010203""";

      Color c = JsonConvert.DeserializeObject<Color>(json, new HtmlColorConverter());
      Assert.AreEqual(c.A, 255);
      Assert.AreEqual(c.R, 1);
      Assert.AreEqual(c.G, 2);
      Assert.AreEqual(c.B, 3);
    }
  }
}
#endif