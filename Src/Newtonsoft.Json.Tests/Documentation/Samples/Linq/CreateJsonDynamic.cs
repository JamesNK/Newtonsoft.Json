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

#if !(NET20 || NET35)

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    [TestFixture]
    public class CreateJsonDynamic : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            dynamic product = new JObject();
            product.ProductName = "Elbow Grease";
            product.Enabled = true;
            product.Price = 4.90m;
            product.StockCount = 9000;
            product.StockValue = 44100;
            product.Tags = new JArray("Real", "OnSale");

            Console.WriteLine(product.ToString());
            // {
            //   "ProductName": "Elbow Grease",
            //   "Enabled": true,
            //   "Price": 4.90,
            //   "StockCount": 9000,
            //   "StockValue": 44100,
            //   "Tags": [
            //     "Real",
            //     "OnSale"
            //   ]
            // }
            #endregion

            Assert.AreEqual(@"{
  ""ProductName"": ""Elbow Grease"",
  ""Enabled"": true,
  ""Price"": 4.90,
  ""StockCount"": 9000,
  ""StockValue"": 44100,
  ""Tags"": [
    ""Real"",
    ""OnSale""
  ]
}", product.ToString());
        }
    }
}

#endif