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
