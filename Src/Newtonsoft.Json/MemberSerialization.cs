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
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies the member serialization options for the <see cref="JsonSerializer"/>.
    /// </summary>
    public enum MemberSerialization
    {
#pragma warning disable 1584,1711,1572,1581,1580,1574
        /// <summary>
        /// All public members are serialized by default. Members can be excluded using <see cref="JsonIgnoreAttribute"/> or <see cref="NonSerializedAttribute"/>.
        /// This is the default member serialization mode.
        /// </summary>
        OptOut = 0,

        /// <summary>
        /// Only members marked with <see cref="JsonPropertyAttribute"/> or <see cref="DataMemberAttribute"/> are serialized.
        /// This member serialization mode can also be set by marking the class with <see cref="DataContractAttribute"/>.
        /// </summary>
        OptIn = 1,

        /// <summary>
        /// All public and private fields are serialized. Members can be excluded using <see cref="JsonIgnoreAttribute"/> or <see cref="NonSerializedAttribute"/>.
        /// This member serialization mode can also be set by marking the class with <see cref="SerializableAttribute"/>
        /// and setting IgnoreSerializableAttribute on <see cref="DefaultContractResolver"/> to false.
        /// </summary>
        Fields = 2
#pragma warning restore 1584,1711,1572,1581,1580,1574
    }
}
