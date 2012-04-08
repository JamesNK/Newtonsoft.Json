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

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class Store
  {
    public StoreColor Color = StoreColor.Yellow;
    public DateTime Establised = new DateTime(2010, 1, 22, 1, 1, 1, DateTimeKind.Utc);
    public double Width = 1.1;
    public int Employees = 999;
    public int[] RoomsPerFloor = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public bool Open = false;
    public char Symbol = '@';
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> Mottos = new List<string>();
    public decimal Cost = 100980.1M;
    public string Escape = "\r\n\t\f\b?{\\r\\n\"\'";
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<Product> product = new List<Product>();

    public Store()
    {
      Mottos.Add("Hello World");
      Mottos.Add("öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~");
      Mottos.Add(null);
      Mottos.Add(" ");

      Product rocket = new Product();
      rocket.Name = "Rocket";
      rocket.ExpiryDate = new DateTime(2000, 2, 2, 23, 1, 30, DateTimeKind.Utc);
      Product alien = new Product();
      alien.Name = "Alien";

      product.Add(rocket);
      product.Add(alien);
    }
  }
}
