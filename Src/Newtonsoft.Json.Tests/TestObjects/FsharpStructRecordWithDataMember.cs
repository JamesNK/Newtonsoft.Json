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
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if !NET20
using System.Runtime.Serialization;
#endif

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !NET20
    [Serializable]   
    [DataContract]
#endif
    // This mimics what F# compiler produces for a simple [<Struct>] record which is also using DataMember attribute
    // Follows https://github.com/JamesNK/Newtonsoft.Json/issues/1295#issuecomment-1534807350 , simplified to have it compile
    public struct FSharpStructRecordWithDataMember
    {
        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string Foo_;
#if !NET20
        [DataMember(Name = "foo_field")]
#endif
        public readonly string Foo
        {
            [CompilerGenerated]
            [DebuggerNonUserCode]
            get
            {
                return Foo_;
            }
        }

        public FSharpStructRecordWithDataMember(string foo)
        {
            Foo_ = foo;
        }

        [CompilerGenerated]
        public override string ToString()
        {
            return "FSharpStructRecordWithDataMember { Foo = " + Foo + " }";
        }
    }
}
