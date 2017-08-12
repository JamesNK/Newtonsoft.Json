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
    public class AATestClass
    {
        [JsonProperty]
        protected int AA_field1;

        protected int AA_field2;

        [JsonProperty]
        protected int AA_property1 { get; set; }

        [JsonProperty]
        protected int AA_property2 { get; private set; }

        [JsonProperty]
        protected int AA_property3 { private get; set; }

        [JsonProperty]
        private int AA_property4 { get; set; }

        protected int AA_property5 { get; private set; }
        protected int AA_property6 { private get; set; }

        public AATestClass()
        {
        }

        public AATestClass(int f)
        {
            AA_field1 = f;
            AA_field2 = f;
            AA_property1 = f;
            AA_property2 = f;
            AA_property3 = f;
            AA_property4 = f;
            AA_property5 = f;
            AA_property6 = f;
        }
    }
}
