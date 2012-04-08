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

  internal class BsonString : BsonValue
  {
    public int ByteCount { get; set; }
    public bool IncludeLength { get; set; }

    public BsonString(object value, bool includeLength)
      : base(value, BsonType.String)
    {
      IncludeLength = includeLength;
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