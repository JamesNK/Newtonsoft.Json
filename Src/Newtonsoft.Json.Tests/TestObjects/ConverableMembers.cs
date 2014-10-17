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

namespace Newtonsoft.Json.Tests.TestObjects
{
    [JsonObject]
    public class ConverableMembers
    {
        public string String = "string";
        public int Int32 = int.MaxValue;
        public uint UInt32 = uint.MaxValue;
        public byte Byte = byte.MaxValue;
        public sbyte SByte = sbyte.MaxValue;
        public short Short = short.MaxValue;
        public ushort UShort = ushort.MaxValue;
        public long Long = long.MaxValue;
        public ulong ULong = long.MaxValue;
        public double Double = double.MaxValue;
        public float Float = float.MaxValue;
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50)
        public DBNull DBNull = DBNull.Value;
#endif
        public bool Bool = true;
        public char Char = '\0';
    }
}