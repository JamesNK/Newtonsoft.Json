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

using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Bson
{
    internal abstract class BsonToken
    {
        public abstract BsonType Type { get; }
        public BsonToken Parent { get; set; }
        public int CalculatedSize { get; set; }
    }

    internal class BsonObject : BsonToken, IEnumerable<BsonProperty>
    {
        private readonly List<BsonProperty> _children = new List<BsonProperty>();

        public void Add(string name, BsonToken token)
        {
            _children.Add(new BsonProperty { Name = new BsonString(name, false), Value = token });
            token.Parent = this;
        }

        public override BsonType Type
        {
            get { return BsonType.Object; }
        }

        public IEnumerator<BsonProperty> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class BsonArray : BsonToken, IEnumerable<BsonToken>
    {
        private readonly List<BsonToken> _children = new List<BsonToken>();

        public void Add(BsonToken token)
        {
            _children.Add(token);
            token.Parent = this;
        }

        public override BsonType Type
        {
            get { return BsonType.Array; }
        }

        public IEnumerator<BsonToken> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class BsonEmpty : BsonToken
    {
        public static readonly BsonToken Null = new BsonEmpty(BsonType.Null);
        public static readonly BsonToken Undefined = new BsonEmpty(BsonType.Undefined);

        private BsonEmpty(BsonType type)
        {
            Type = type;
        }

        public override BsonType Type { get; }
    }

    internal class BsonValue : BsonToken
    {
        private readonly object _value;
        private readonly BsonType _type;

        public BsonValue(object value, BsonType type)
        {
            _value = value;
            _type = type;
        }

        public object Value
        {
            get { return _value; }
        }

        public override BsonType Type
        {
            get { return _type; }
        }
    }

    internal class BsonBoolean : BsonValue
    {
        public static readonly BsonBoolean False = new BsonBoolean(false);
        public static readonly BsonBoolean True = new BsonBoolean(true);

        private BsonBoolean(bool value)
            : base(value, BsonType.Boolean)
        {
        }
    }

    internal class BsonString : BsonValue
    {
        public int ByteCount { get; set; }
        public bool IncludeLength { get; }

        public BsonString(object value, bool includeLength)
            : base(value, BsonType.String)
        {
            IncludeLength = includeLength;
        }
    }

    internal class BsonBinary : BsonValue
    {
        public BsonBinaryType BinaryType { get; set; }

        public BsonBinary(byte[] value, BsonBinaryType binaryType)
            : base(value, BsonType.Binary)
        {
            BinaryType = binaryType;
        }
    }

    internal class BsonRegex : BsonToken
    {
        public BsonString Pattern { get; set; }
        public BsonString Options { get; set; }

        public BsonRegex(string pattern, string options)
        {
            Pattern = new BsonString(pattern, false);
            Options = new BsonString(options, false);
        }

        public override BsonType Type
        {
            get { return BsonType.Regex; }
        }
    }

    internal class BsonProperty
    {
        public BsonString Name { get; set; }
        public BsonToken Value { get; set; }
    }
}