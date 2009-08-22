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

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq.ComponentModel;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace Newtonsoft.Json.Tests.Linq.ComponentModel
{
  public class JTypeDescriptorTests : TestFixtureBase
  {
    [Test]
    public void GetProperties()
    {
      JObject o = JObject.Parse("{'prop1':12,'prop2':'hi!','prop3':null,'prop4':[1,2,3]}");

      JTypeDescriptor descriptor = new JTypeDescriptor(o);

      PropertyDescriptorCollection properties = descriptor.GetProperties();
      Assert.AreEqual(4, properties.Count);

      PropertyDescriptor prop1 = properties[0];
      Assert.AreEqual("prop1", prop1.Name);
      Assert.AreEqual(typeof(long), prop1.PropertyType);
      Assert.AreEqual(typeof(JObject), prop1.ComponentType);
      Assert.AreEqual(false, prop1.CanResetValue(o));
      Assert.AreEqual(false, prop1.ShouldSerializeValue(o));

      PropertyDescriptor prop2 = properties[1];
      Assert.AreEqual("prop2", prop2.Name);
      Assert.AreEqual(typeof(string), prop2.PropertyType);
      Assert.AreEqual(typeof(JObject), prop2.ComponentType);
      Assert.AreEqual(false, prop2.CanResetValue(o));
      Assert.AreEqual(false, prop2.ShouldSerializeValue(o));

      PropertyDescriptor prop3 = properties[2];
      Assert.AreEqual("prop3", prop3.Name);
      Assert.AreEqual(typeof(object), prop3.PropertyType);
      Assert.AreEqual(typeof(JObject), prop3.ComponentType);
      Assert.AreEqual(false, prop3.CanResetValue(o));
      Assert.AreEqual(false, prop3.ShouldSerializeValue(o));

      PropertyDescriptor prop4 = properties[3];
      Assert.AreEqual("prop4", prop4.Name);
      Assert.AreEqual(typeof(JArray), prop4.PropertyType);
      Assert.AreEqual(typeof(JObject), prop4.ComponentType);
      Assert.AreEqual(false, prop4.CanResetValue(o));
      Assert.AreEqual(false, prop4.ShouldSerializeValue(o));
    }
  }
}
#endif