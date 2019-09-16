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
    [Serializable]
    public sealed class VersionOld : IComparable, IComparable<VersionOld>, IEquatable<VersionOld>
    {
        // AssemblyName depends on the order staying the same
        private readonly int _Major; // Do not rename (binary serialization)
        private readonly int _Minor; // Do not rename (binary serialization)
        private readonly int _Build = -1; // Do not rename (binary serialization)
        private readonly int _Revision = -1; // Do not rename (binary serialization)

        [JsonConstructor]
        public VersionOld(int major, int minor, int build, int revision)
        {
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = revision;
        }

        public VersionOld(int major, int minor, int build)
        {
            _Major = major;
            _Minor = minor;
            _Build = build;
        }

        public VersionOld(int major, int minor)
        {
            _Major = major;
            _Minor = minor;
        }

        public VersionOld()
        {
            _Major = 0;
            _Minor = 0;
        }

        // Properties for setting and getting version numbers
        public int Major => _Major;

        public int Minor => _Minor;

        public int Build => _Build;

        public int Revision => _Revision;

        public short MajorRevision => (short)(_Revision >> 16);

        public short MinorRevision => (short)(_Revision & 0xFFFF);

        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }

            if (version is VersionOld v)
            {
                return CompareTo(v);
            }

            throw new ArgumentException();
        }

        public int CompareTo(VersionOld value)
        {
            return
                object.ReferenceEquals(value, this) ? 0 :
                value is null ? 1 :
                _Major != value._Major ? (_Major > value._Major ? 1 : -1) :
                _Minor != value._Minor ? (_Minor > value._Minor ? 1 : -1) :
                _Build != value._Build ? (_Build > value._Build ? 1 : -1) :
                _Revision != value._Revision ? (_Revision > value._Revision ? 1 : -1) :
                0;
        }

        public bool Equals(VersionOld obj)
        {
            return object.ReferenceEquals(obj, this) ||
                (!(obj is null) &&
                _Major == obj._Major &&
                _Minor == obj._Minor &&
                _Build == obj._Build &&
                _Revision == obj._Revision);
        }
    }
}