using System;
using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using TestAttribute = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class AnnotationsTests : TestFixtureBase
    {
        [Test]
        public void AddAnnotation()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");

            string s = o.Annotation<string>();
            Assert.AreEqual("A string!", s);

            s = (string)o.Annotation(typeof(string));
            Assert.AreEqual("A string!", s);
        }

        [Test]
        public void AddAnnotation_MultipleOfTheSameType()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation("Another string!");

            string s = o.Annotation<string>();
            Assert.AreEqual("A string!", s);

            s = (string)o.Annotation(typeof(string));
            Assert.AreEqual("A string!", s);
        }

        [Test]
        public void AddAnnotation_MultipleOfDifferentTypes()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            string s = o.Annotation<string>();
            Assert.AreEqual("A string!", s);

            s = (string)o.Annotation(typeof(string));
            Assert.AreEqual("A string!", s);

            Uri i = o.Annotation<Uri>();
            Assert.AreEqual(new Uri("http://www.google.com/"), i);

            i = (Uri)o.Annotation(typeof(Uri));
            Assert.AreEqual(new Uri("http://www.google.com/"), i);
        }

        [Test]
        public void GetAnnotation_NeverSet()
        {
            JObject o = new JObject();

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            s = (string)o.Annotation(typeof(string));
            Assert.AreEqual(null, s);
        }

        [Test]
        public void GetAnnotations()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation("A string 2!");
            o.AddAnnotation("A string 3!");

            IList<string> l = o.Annotations<string>().ToList();

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("A string!", l[0]);
            Assert.AreEqual("A string 2!", l[1]);
            Assert.AreEqual("A string 3!", l[2]);

            l = o.Annotations(typeof(string)).Cast<string>().ToList();

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("A string!", l[0]);
            Assert.AreEqual("A string 2!", l[1]);
            Assert.AreEqual("A string 3!", l[2]);
        }

        [Test]
        public void GetAnnotations_MultipleTypes()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation("A string 2!");
            o.AddAnnotation("A string 3!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            IList<object> l = o.Annotations<object>().ToList();

            Assert.AreEqual(4, l.Count);
            Assert.AreEqual("A string!", l[0]);
            Assert.AreEqual("A string 2!", l[1]);
            Assert.AreEqual("A string 3!", l[2]);
            Assert.AreEqual(new Uri("http://www.google.com/"), l[3]);

            l = o.Annotations(typeof(object)).ToList();

            Assert.AreEqual(4, l.Count);
            Assert.AreEqual("A string!", l[0]);
            Assert.AreEqual("A string 2!", l[1]);
            Assert.AreEqual("A string 3!", l[2]);
            Assert.AreEqual(new Uri("http://www.google.com/"), l[3]);
        }

        [Test]
        public void RemoveAnnotation()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");

            o.RemoveAnnotations<string>();

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);
        }

        [Test]
        public void RemoveAnnotation_NonGeneric()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");

            o.RemoveAnnotations(typeof(string));

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            s = (string)o.Annotation(typeof(string));
            Assert.AreEqual(null, s);
        }

        [Test]
        public void RemoveAnnotation_Multiple()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation("A string 2!");
            o.AddAnnotation("A string 3!");

            o.RemoveAnnotations<string>();

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            o.AddAnnotation("A string 4!");

            s = o.Annotation<string>();
            Assert.AreEqual("A string 4!", s);

            Uri i = (Uri)o.Annotation(typeof(Uri));
            Assert.AreEqual(null, i);
        }

        [Test]
        public void RemoveAnnotation_MultipleCalls()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            o.RemoveAnnotations<string>();
            o.RemoveAnnotations<Uri>();

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            Uri i = o.Annotation<Uri>();
            Assert.AreEqual(null, i);
        }

        [Test]
        public void RemoveAnnotation_Multiple_NonGeneric()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation("A string 2!");

            o.RemoveAnnotations(typeof(string));

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);
        }

        [Test]
        public void RemoveAnnotation_MultipleCalls_NonGeneric()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            o.RemoveAnnotations(typeof(string));
            o.RemoveAnnotations(typeof(Uri));

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            Uri i = o.Annotation<Uri>();
            Assert.AreEqual(null, i);
        }

        [Test]
        public void RemoveAnnotation_MultipleWithDifferentTypes()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            o.RemoveAnnotations<string>();

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            Uri i = o.Annotation<Uri>();
            Assert.AreEqual(new Uri("http://www.google.com/"), i);
        }

        [Test]
        public void RemoveAnnotation_MultipleWithDifferentTypes_NonGeneric()
        {
            JObject o = new JObject();
            o.AddAnnotation("A string!");
            o.AddAnnotation(new Uri("http://www.google.com/"));

            o.RemoveAnnotations(typeof(string));

            string s = o.Annotation<string>();
            Assert.AreEqual(null, s);

            Uri i = o.Annotation<Uri>();
            Assert.AreEqual(new Uri("http://www.google.com/"), i);
        }

        [Test]
        public void AnnotationsAreCopied()
        {
            JObject o = new JObject();
            o.AddAnnotation("string!");
            AssertCloneCopy(o, "string!");

            JProperty p = new JProperty("Name", "Content");
            p.AddAnnotation("string!");
            AssertCloneCopy(p, "string!");

            JArray a = new JArray();
            a.AddAnnotation("string!");
            AssertCloneCopy(a, "string!");

            JConstructor c = new JConstructor("Test");
            c.AddAnnotation("string!");
            AssertCloneCopy(c, "string!");

            JValue v = new JValue(true);
            v.AddAnnotation("string!");
            AssertCloneCopy(v, "string!");

            JRaw r = new JRaw("raw");
            r.AddAnnotation("string!");
            AssertCloneCopy(r, "string!");
        }

        [Test]
        public void MultipleAnnotationsAreCopied()
        {
            Version version = new Version(1, 2, 3, 4);

            JObject o = new JObject();
            o.AddAnnotation("string!");
            o.AddAnnotation(version);

            JObject o2 = (JObject)o.DeepClone();
            Assert.AreEqual("string!", o2.Annotation<string>());
            Assert.AreEqual(version, o2.Annotation<Version>());

            o2.RemoveAnnotations<Version>();
            Assert.AreEqual(1, o.Annotations<Version>().Count());
            Assert.AreEqual(0, o2.Annotations<Version>().Count());
        }

        private void AssertCloneCopy<T>(JToken t, T annotation) where T : class
        {
            Assert.AreEqual(annotation, t.DeepClone().Annotation<T>());
        }

#if !NET20
        [Test]
        public void Example()
        {
            JObject o = JObject.Parse(@"{
                'name': 'Bill G',
                'age': 58,
                'country': 'United States',
                'employer': 'Microsoft'
            }");

            o.AddAnnotation(new HashSet<string>());
            o.PropertyChanged += (sender, args) => o.Annotation<HashSet<string>>().Add(args.PropertyName);

            o["age"] = 59;
            o["employer"] = "Bill & Melinda Gates Foundation";

            HashSet<string> changedProperties = o.Annotation<HashSet<string>>();
            // age
            // employer

            Assert.AreEqual(2, changedProperties.Count);
        }
#endif
    }
}