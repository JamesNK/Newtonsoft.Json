using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Newtonsoft.Json
{
  internal struct MemberMapping
  {
    private readonly string _mappingName;
    private readonly MemberInfo _member;
    private readonly bool _ignored;
    private readonly bool _readable;
    private readonly bool _writable;

    public MemberMapping(string mappingName, MemberInfo member, bool ignored, bool readable, bool writable)
    {
      _mappingName = mappingName;
      _member = member;
      _ignored = ignored;
      _readable = readable;
      _writable = writable;
    }

    public string MappingName
    {
      get { return _mappingName; }
    }

    public MemberInfo Member
    {
      get { return _member; }
    }

    public bool Ignored
    {
      get { return _ignored; }
    }

    public bool Readable
    {
      get { return _readable; }
    }

    public bool Writable
    {
      get { return _writable; }
    }
  }
}
