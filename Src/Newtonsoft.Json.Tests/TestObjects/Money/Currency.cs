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

#if !(NET40 || NET35 || NET20 || DNXCORE50) || NETSTANDARD1_3 || NETSTANDARD2_0
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.FSharp.Core;

namespace Newtonsoft.Json.Tests.TestObjects.Money
{
    [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}"), CompilationMapping(SourceConstructFlags.SumType)]
    public class Currency
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly int _tag;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_AUD = new Currency(1);

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_EUR = new Currency(4);

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_JPY = new Currency(5);

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_LocalCurrency = new Currency(0);

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_NZD = new Currency(2);

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Currency _unique_USD = new Currency(3);

        [CompilerGenerated, DebuggerNonUserCode]
        internal Currency(int _tag)
        {
            this._tag = _tag;
        }

        [CompilerGenerated, DebuggerNonUserCode]
        internal object __DebugDisplay()
        {
            return ExtraTopLevelOperators.PrintFormatToString<FSharpFunc<Currency, string>>(new PrintfFormat<FSharpFunc<Currency, string>, Unit, string, string, string>("%+0.8A")).Invoke(this);
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency AUD
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 1)]
            get { return _unique_AUD; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency EUR
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 4)]
            get { return _unique_EUR; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsAUD
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 1); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEUR
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 4); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsJPY
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 5); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsLocalCurrency
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 0); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsNZD
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 2); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsUSD
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return (this.Tag == 3); }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency JPY
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 5)]
            get { return _unique_JPY; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency LocalCurrency
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 0)]
            get { return _unique_LocalCurrency; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency NZD
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 2)]
            get { return _unique_NZD; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Tag
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get { return this._tag; }
        }

        [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Currency USD
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 3)]
            get { return _unique_USD; }
        }
    }
}
#endif