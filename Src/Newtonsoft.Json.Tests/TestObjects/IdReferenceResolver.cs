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
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class IdReferenceResolver : IReferenceResolver
    {
        private readonly IDictionary<Guid, PersonReference> _people = new Dictionary<Guid, PersonReference>();

        public object ResolveReference(object context, string reference)
        {
            Guid id = new Guid(reference);

            PersonReference p;
            _people.TryGetValue(id, out p);

            return p;
        }

        public string GetReference(object context, object value)
        {
            PersonReference p = (PersonReference)value;
            _people[p.Id] = p;

            return p.Id.ToString();
        }

        public bool IsReferenced(object context, object value)
        {
            PersonReference p = (PersonReference)value;

            return _people.ContainsKey(p.Id);
        }

        public void AddReference(object context, string reference, object value)
        {
            Guid id = new Guid(reference);

            _people[id] = (PersonReference)value;
        }
    }
}