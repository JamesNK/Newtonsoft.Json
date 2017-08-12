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

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class BBTestClass : AATestClass
    {
        [JsonProperty]
        public int BB_field1;

        public int BB_field2;

        [JsonProperty]
        public int BB_property1 { get; set; }

        [JsonProperty]
        public int BB_property2 { get; private set; }

        [JsonProperty]
        public int BB_property3 { private get; set; }

        [JsonProperty]
        private int BB_property4 { get; set; }

        public int BB_property5 { get; private set; }
        public int BB_property6 { private get; set; }

        [JsonProperty]
        public int BB_property7 { protected get; set; }

        public int BB_property8 { protected get; set; }

        public BBTestClass()
        {
        }

        public BBTestClass(int f, int g)
            : base(f)
        {
            BB_field1 = g;
            BB_field2 = g;
            BB_property1 = g;
            BB_property2 = g;
            BB_property3 = g;
            BB_property4 = g;
            BB_property5 = g;
            BB_property6 = g;
            BB_property7 = g;
            BB_property8 = g;
        }
    }
}