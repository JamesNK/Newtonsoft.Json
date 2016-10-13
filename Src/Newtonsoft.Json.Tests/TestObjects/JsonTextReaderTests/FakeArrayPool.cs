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

using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests
{
    public class FakeArrayPool : IArrayPool<char>
    {
        public readonly List<char[]> FreeArrays = new List<char[]>();
        public readonly List<char[]> UsedArrays = new List<char[]>();

        public char[] Rent(int minimumLength)
        {
            char[] a = FreeArrays.FirstOrDefault(b => b.Length >= minimumLength);
            if (a != null)
            {
                FreeArrays.Remove(a);
                UsedArrays.Add(a);

                return a;
            }

            a = new char[minimumLength];
            UsedArrays.Add(a);

            return a;
        }

        public void Return(char[] array)
        {
            if (UsedArrays.Remove(array))
            {
                FreeArrays.Add(array);

                // smallest first so the first array large enough is rented
                FreeArrays.Sort((b1, b2) => Comparer<int>.Default.Compare(b1.Length, b2.Length));
            }
        }
    }
}
