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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class VersionKeyedCollection : KeyedCollection<string, Person>, IEnumerable<Person>
    {
        public List<string> Messages { get; set; }

        public VersionKeyedCollection()
        {
            Messages = new List<string>();
        }

        protected override string GetKeyForItem(Person item)
        {
            return item.Name;
        }

        [OnError]
        internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
        {
            Messages.Add(errorContext.Path + " - Error message for member " + errorContext.Member + " = " + errorContext.Error.Message);
            errorContext.Handled = true;
        }

        IEnumerator<Person> IEnumerable<Person>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                if (i % 2 == 0)
                    throw new Exception("Index even: " + i);

                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Person>)this).GetEnumerator();
        }
    }
}