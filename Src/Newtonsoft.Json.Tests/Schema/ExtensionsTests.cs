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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Schema
{
  public class ExtensionsTests : TestFixtureBase
  {
    [Test]
    public void IsValid()
    {
      JsonSchema schema = JsonSchema.Parse("{'type':'integer'}");
      JToken stringToken = JToken.FromObject("pie");
      JToken integerToken = JToken.FromObject(1);

      Assert.AreEqual(false, stringToken.IsValid(schema));
      Assert.AreEqual(true, integerToken.IsValid(schema));
    }

    [Test]
    public void ValidateWithEventHandler()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie lol");

      List<string> errors = new List<string>();
      stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
      Assert.AreEqual(0, errors.Count);

      stringToken = JToken.FromObject("pie");

      stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
      Assert.AreEqual(1, errors.Count);

      Assert.AreEqual("String 'pie' does not match regex pattern 'lol'.", errors[0]);
    }

    [Test]
    [ExpectedException(typeof(JsonSchemaException), ExpectedMessage = @"String 'pie' does not match regex pattern 'lol'.")]
    public void ValidateWithOutEventHandlerFailure()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie");
      stringToken.Validate(schema);
    }

    [Test]
    public void ValidateWithOutEventHandlerSuccess()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie lol");
      stringToken.Validate(schema);
    }
  }
}