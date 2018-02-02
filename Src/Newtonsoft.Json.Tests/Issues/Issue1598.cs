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

#if !(NET20 || NET35)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1598 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            Activities activities = new Activities();
            activities.List = new List<Activity>
            {
                new Activity
                {
                    Name = "An activity"
                }
            };

            string json = JsonConvert.SerializeObject(activities, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""List"": [
    {
      ""Name"": ""An activity""
    }
  ]
}", json);
        }

        [Test]
        public void Test_SubClass()
        {
            ActivitiesSubClass activities = new ActivitiesSubClass();
            activities.List = new List<Activity>
            {
                new Activity
                {
                    Name = "An activity"
                }
            };

            string json = JsonConvert.SerializeObject(activities, Formatting.Indented);
            StringAssert.AreEqual(@"[
  {
    ""Name"": ""An activity""
  }
]", json);
        }

        public class Activity
        {
            public string Name { get; set; }
        }

        public class ActivitiesSubClass : Activities
        {
        }

        [DataContract]
        public class Activities : IEnumerable<Activity>
        {
            [DataMember]
            public List<Activity> List { get; set; }

            public IEnumerator<Activity> GetEnumerator()
            {
                return List.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
#endif