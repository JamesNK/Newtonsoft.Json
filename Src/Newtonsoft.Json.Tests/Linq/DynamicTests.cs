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

#if !(NET35 || NET20 || PORTABLE40)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
#if !(NET20 || NET35 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
using Newtonsoft.Json.Linq;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class DynamicTests : TestFixtureBase
    {
        [Test]
        public void JObjectPropertyNames()
        {
            JObject o = new JObject(
                new JProperty("ChildValue", "blah blah"));

            dynamic d = o;

            d.First = "A value!";

            Assert.AreEqual(new JValue("A value!"), d.First);
            Assert.AreEqual("A value!", (string)d.First);

            d.First = null;
            Assert.AreEqual(JTokenType.Null, d.First.Type);

            Assert.IsTrue(d.Remove("First"));
            Assert.IsNull(d.First);

            JValue v1 = d.ChildValue;
            JValue v2 = d["ChildValue"];
            Assert.AreEqual(v1, v2);

            JValue newValue1 = new JValue("Blah blah");
            d.NewValue = newValue1;
            JValue newValue2 = d.NewValue;

            Assert.IsTrue(ReferenceEquals(newValue1, newValue2));
        }

        [Test]
        public void JObjectCount()
        {
            JObject o = new JObject();

            dynamic d = o;

            long? c1 = d.Count;

            o["Count"] = 99;

            long? c2 = d.Count;

            Assert.AreEqual(null, c1);
            Assert.AreEqual(99, c2);
        }

        [Test]
        public void JObjectEnumerator()
        {
            JObject o = new JObject(
                new JProperty("ChildValue", "blah blah"));

            dynamic d = o;

            foreach (JProperty value in d)
            {
                Assert.AreEqual("ChildValue", value.Name);
                Assert.AreEqual("blah blah", (string)value.Value);
            }

            foreach (dynamic value in d)
            {
                Assert.AreEqual("ChildValue", value.Name);
                Assert.AreEqual("blah blah", (string)value.Value);
            }
        }

        [Test]
        public void JObjectPropertyNameWithJArray()
        {
            JObject o = new JObject(
                new JProperty("ChildValue", "blah blah"));

            dynamic d = o;

            d.First = new JArray();
            d.First.Add("Hi");

            Assert.AreEqual(1, d.First.Count);
        }

        [Test]
        public void JObjectPropertyNameWithNonToken()
        {
            ExceptionAssert.Throws<ArgumentException>(
                "Could not determine JSON object type for type System.String[].",
                () =>
                {
                    dynamic d = new JObject();

                    d.First = new[] { "One", "II", "3" };
                });
        }

        [Test]
        public void JObjectMethods()
        {
            JObject o = new JObject(
                new JProperty("ChildValue", "blah blah"));

            dynamic d = o;

            d.Add("NewValue", 1);

            object count = d.Count;

            Assert.IsNull(count);
            Assert.IsNull(d["Count"]);

            JToken v;
            Assert.IsTrue(d.TryGetValue("ChildValue", out v));
            Assert.AreEqual("blah blah", (string)v);
        }

        [Test]
        public void JValueEquals()
        {
            JObject o = new JObject(
                new JProperty("Null", JValue.CreateNull()),
                new JProperty("Integer", new JValue(1)),
                new JProperty("Float", new JValue(1.1d)),
                new JProperty("Decimal", new JValue(1.1m)),
                new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
                new JProperty("Boolean", new JValue(true)),
                new JProperty("String", new JValue("A string lol!")),
                new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
                new JProperty("Uri", new Uri("http://json.codeplex.com/")),
                new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
                new JProperty("TimeSpan", TimeSpan.FromDays(1))
#if !(NET20 || NET35 || PORTABLE)
                , new JProperty("BigInteger", BigInteger.Parse("1"))
#endif
                );

            dynamic d = o;

            Assert.IsTrue(d.Null == d.Null);
            Assert.IsTrue(d.Null == null);
            Assert.IsTrue(d.Null == JValue.CreateNull());
            Assert.IsFalse(d.Null == 1);

            Assert.IsTrue(d.Integer == d.Integer);
            Assert.IsTrue(d.Integer > 0);
            Assert.IsTrue(d.Integer > 0.0m);
            Assert.IsTrue(d.Integer > 0.0f);
            Assert.IsTrue(d.Integer > null);
            Assert.IsTrue(d.Integer >= null);
            Assert.IsTrue(d.Integer == 1);
            Assert.IsTrue(d.Integer == 1m);
            Assert.IsTrue(d.Integer != 1.1f);
            Assert.IsTrue(d.Integer != 1.1d);

            Assert.IsTrue(d.Decimal == d.Decimal);
            Assert.IsTrue(d.Decimal > 0);
            Assert.IsTrue(d.Decimal > 0.0m);
            Assert.IsTrue(d.Decimal > 0.0f);
            Assert.IsTrue(d.Decimal > null);
            Assert.IsTrue(d.Decimal >= null);
            Assert.IsTrue(d.Decimal == 1.1);
            Assert.IsTrue(d.Decimal == 1.1m);
            Assert.IsTrue(d.Decimal != 1.0f);
            Assert.IsTrue(d.Decimal != 1.0d);
#if !(NET20 || NET35 || PORTABLE)
            Assert.IsTrue(d.Decimal > new BigInteger(0));
#endif

            Assert.IsTrue(d.Float == d.Float);
            Assert.IsTrue(d.Float > 0);
            Assert.IsTrue(d.Float > 0.0m);
            Assert.IsTrue(d.Float > 0.0f);
            Assert.IsTrue(d.Float > null);
            Assert.IsTrue(d.Float >= null);
            Assert.IsTrue(d.Float < 2);
            Assert.IsTrue(d.Float <= 1.1);
            Assert.IsTrue(d.Float == 1.1);
            Assert.IsTrue(d.Float == 1.1m);
            Assert.IsTrue(d.Float != 1.0f);
            Assert.IsTrue(d.Float != 1.0d);
#if !(NET20 || NET35 || PORTABLE)
            Assert.IsTrue(d.Float > new BigInteger(0));
#endif

#if !(NET20 || NET35 || PORTABLE)
            Assert.IsTrue(d.BigInteger == d.BigInteger);
            Assert.IsTrue(d.BigInteger > 0);
            Assert.IsTrue(d.BigInteger > 0.0m);
            Assert.IsTrue(d.BigInteger > 0.0f);
            Assert.IsTrue(d.BigInteger > null);
            Assert.IsTrue(d.BigInteger >= null);
            Assert.IsTrue(d.BigInteger < 2);
            Assert.IsTrue(d.BigInteger <= 1.1);
            Assert.IsTrue(d.BigInteger == 1);
            Assert.IsTrue(d.BigInteger == 1m);
            Assert.IsTrue(d.BigInteger != 1.1f);
            Assert.IsTrue(d.BigInteger != 1.1d);
#endif

            Assert.IsTrue(d.Bytes == d.Bytes);
            Assert.IsTrue(d.Bytes == Encoding.UTF8.GetBytes("A string lol!"));
            Assert.IsTrue(d.Bytes == new JValue(Encoding.UTF8.GetBytes("A string lol!")));

            Assert.IsTrue(d.Uri == d.Uri);
            Assert.IsTrue(d.Uri == new Uri("http://json.codeplex.com/"));
            Assert.IsTrue(d.Uri > new Uri("http://abc.org/"));
            Assert.IsTrue(d.Uri >= new Uri("http://abc.com/"));
            Assert.IsTrue(d.Uri > null);
            Assert.IsTrue(d.Uri >= null);

            Assert.IsTrue(d.Guid == d.Guid);
            Assert.IsTrue(d.Guid == new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF"));
            Assert.IsTrue(d.Guid > new Guid("AAAAAAAA-0D80-44F2-BF34-4654156FA7AF"));
            Assert.IsTrue(d.Guid >= new Guid("AAAAAAAA-0D80-44F2-BF34-4654156FA7AF"));
            Assert.IsTrue(d.Guid > null);
            Assert.IsTrue(d.Guid >= null);

            Assert.IsTrue(d.TimeSpan == d.TimeSpan);
            Assert.IsTrue(d.TimeSpan == TimeSpan.FromDays(1));
            Assert.IsTrue(d.TimeSpan > TimeSpan.FromHours(1));
            Assert.IsTrue(d.TimeSpan >= TimeSpan.FromHours(1));
            Assert.IsTrue(d.TimeSpan > null);
            Assert.IsTrue(d.TimeSpan >= null);
        }

        [Test]
        public void JValueAddition()
        {
            JObject o = new JObject(
                new JProperty("Null", JValue.CreateNull()),
                new JProperty("Integer", new JValue(1)),
                new JProperty("Float", new JValue(1.1d)),
                new JProperty("Decimal", new JValue(1.1m)),
                new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
                new JProperty("Boolean", new JValue(true)),
                new JProperty("String", new JValue("A string lol!")),
                new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
                new JProperty("Uri", new Uri("http://json.codeplex.com/")),
                new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
                new JProperty("TimeSpan", TimeSpan.FromDays(1))
#if !(NET20 || NET35 || PORTABLE)
                , new JProperty("BigInteger", new BigInteger(100))
#endif
                );

            dynamic d = o;
            dynamic r;

            #region Add
            r = d.String + " LAMO!";
            Assert.AreEqual("A string lol! LAMO!", (string)r);
            r += " gg";
            Assert.AreEqual("A string lol! LAMO! gg", (string)r);

            r = d.String + null;
            Assert.AreEqual("A string lol!", (string)r);
            r += null;
            Assert.AreEqual("A string lol!", (string)r);

            r = d.Integer + 1;
            Assert.AreEqual(2, (int)r);
            r += 2;
            Assert.AreEqual(4, (int)r);

            r = d.Integer + 1.1;
            Assert.AreEqual(2.1, (double)r);
            r += 2;
            Assert.AreEqual(4.1, (double)r);

            r = d.Integer + 1.1d;
            Assert.AreEqual(2.1m, (decimal)r);
            r += 2;
            Assert.AreEqual(4.1m, (decimal)r);

            r = d.Integer + null;
            Assert.AreEqual(null, r.Value);
            r += 2;
            Assert.AreEqual(null, r.Value);

            r = d.Float + 1;
            Assert.AreEqual(2.1d, (double)r);
            r += 2;
            Assert.AreEqual(4.1d, (double)r);

            r = d.Float + 1.1;
            Assert.AreEqual(2.2d, (double)r);
            r += 2;
            Assert.AreEqual(4.2d, (double)r);

            r = d.Float + 1.1d;
            Assert.AreEqual(2.2m, (decimal)r);
            r += 2;
            Assert.AreEqual(4.2m, (decimal)r);

            r = d.Float + null;
            Assert.AreEqual(null, r.Value);
            r += 2;
            Assert.AreEqual(null, r.Value);

            r = d.Decimal + 1;
            Assert.AreEqual(2.1m, (decimal)r);
            r += 2;
            Assert.AreEqual(4.1m, (decimal)r);

            r = d.Decimal + 1.1;
            Assert.AreEqual(2.2m, (decimal)r);
            r += 2;
            Assert.AreEqual(4.2m, (decimal)r);

            r = d.Decimal + 1.1d;
            Assert.AreEqual(2.2m, (decimal)r);
            r += 2;
            Assert.AreEqual(4.2m, (decimal)r);

            r = d.Decimal + null;
            Assert.AreEqual(null, r.Value);
            r += 2;
            Assert.AreEqual(null, r.Value);

#if !(NET20 || NET35 || PORTABLE)
            r = d.BigInteger + null;
            Assert.AreEqual(null, r.Value);
            r += 2;
            Assert.AreEqual(null, r.Value);

            r = d.BigInteger + 1;
            Assert.AreEqual(101, (int)r);
            r += 2;
            Assert.AreEqual(103, (int)r);

            r = d.BigInteger + 1.1d;
            Assert.AreEqual(101m, (decimal)r);
            r += 2;
            Assert.AreEqual(103m, (decimal)r);
#endif
            #endregion

            #region Subtract
            r = d.Integer - 1;
            Assert.AreEqual(0, (int)r);
            r -= 2;
            Assert.AreEqual(-2, (int)r);

            r = d.Integer - 1.1;
            Assert.AreEqual(-0.1d, (double)r, 0.00001);
            r -= 2;
            Assert.AreEqual(-2.1d, (double)r);

            r = d.Integer - 1.1d;
            Assert.AreEqual(-0.1m, (decimal)r);
            r -= 2;
            Assert.AreEqual(-2.1m, (decimal)r);

            r = d.Integer - null;
            Assert.AreEqual(null, r.Value);
            r -= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Float - 1;
            Assert.AreEqual(0.1d, (double)r, 0.00001);
            r -= 2;
            Assert.AreEqual(-1.9d, (double)r);

            r = d.Float - 1.1;
            Assert.AreEqual(0d, (double)r);
            r -= 2;
            Assert.AreEqual(-2d, (double)r);

            r = d.Float - 1.1d;
            Assert.AreEqual(0m, (decimal)r);
            r -= 2;
            Assert.AreEqual(-2m, (decimal)r);

            r = d.Float - null;
            Assert.AreEqual(null, r.Value);
            r -= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Decimal - 1;
            Assert.AreEqual(0.1m, (decimal)r);
            r -= 2;
            Assert.AreEqual(-1.9m, (decimal)r);

            r = d.Decimal - 1.1;
            Assert.AreEqual(0m, (decimal)r);
            r -= 2;
            Assert.AreEqual(-2m, (decimal)r);

            r = d.Decimal - 1.1d;
            Assert.AreEqual(0m, (decimal)r);
            r -= 2;
            Assert.AreEqual(-2m, (decimal)r);

            r = d.Decimal - null;
            Assert.AreEqual(null, r.Value);
            r -= 2;
            Assert.AreEqual(null, r.Value);

#if !(NET20 || NET35 || PORTABLE)
            r = d.BigInteger - null;
            Assert.AreEqual(null, r.Value);
            r -= 2;
            Assert.AreEqual(null, r.Value);

            r = d.BigInteger - 1.1d;
            Assert.AreEqual(99m, (decimal)r);
            r -= 2;
            Assert.AreEqual(97m, (decimal)r);
#endif
            #endregion

            #region Multiply
            r = d.Integer * 1;
            Assert.AreEqual(1, (int)r);
            r *= 2;
            Assert.AreEqual(2, (int)r);

            r = d.Integer * 1.1;
            Assert.AreEqual(1.1d, (double)r);
            r *= 2;
            Assert.AreEqual(2.2d, (double)r);

            r = d.Integer * 1.1d;
            Assert.AreEqual(1.1m, (decimal)r);
            r *= 2;
            Assert.AreEqual(2.2m, (decimal)r);

            r = d.Integer * null;
            Assert.AreEqual(null, r.Value);
            r *= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Float * 1;
            Assert.AreEqual(1.1d, (double)r);
            r *= 2;
            Assert.AreEqual(2.2d, (double)r);

            r = d.Float * 1.1;
            Assert.AreEqual(1.21d, (double)r, 0.00001);
            r *= 2;
            Assert.AreEqual(2.42d, (double)r, 0.00001);

            r = d.Float * 1.1d;
            Assert.AreEqual(1.21m, (decimal)r);
            r *= 2;
            Assert.AreEqual(2.42m, (decimal)r);

            r = d.Float * null;
            Assert.AreEqual(null, r.Value);
            r *= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Decimal * 1;
            Assert.AreEqual(1.1m, (decimal)r);
            r *= 2;
            Assert.AreEqual(2.2m, (decimal)r);

            r = d.Decimal * 1.1;
            Assert.AreEqual(1.21m, (decimal)r);
            r *= 2;
            Assert.AreEqual(2.42m, (decimal)r);

            r = d.Decimal * 1.1d;
            Assert.AreEqual(1.21m, (decimal)r);
            r *= 2;
            Assert.AreEqual(2.42m, (decimal)r);

            r = d.Decimal * null;
            Assert.AreEqual(null, r.Value);
            r *= 2;
            Assert.AreEqual(null, r.Value);

#if !(NET20 || NET35 || PORTABLE)
            r = d.BigInteger * 1.1d;
            Assert.AreEqual(100m, (decimal)r);
            r *= 2;
            Assert.AreEqual(200m, (decimal)r);

            r = d.BigInteger * null;
            Assert.AreEqual(null, r.Value);
            r *= 2;
            Assert.AreEqual(null, r.Value);
#endif
            #endregion

            #region Divide
            r = d.Integer / 1;
            Assert.AreEqual(1, (int)r);
            r /= 2;
            Assert.AreEqual(0, (int)r);

            r = d.Integer / 1.1;
            Assert.AreEqual(0.9090909090909091d, (double)r);
            r /= 2;
            Assert.AreEqual(0.454545454545455d, (double)r, 0.00001);

            r = d.Integer / 1.1d;
            Assert.AreEqual(0.909090909090909m, (decimal)r);
            r /= 2;
            Assert.AreEqual(0.454545454545454m, (decimal)r);

            r = d.Integer / null;
            Assert.AreEqual(null, r.Value);
            r /= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Float / 1;
            Assert.AreEqual(1.1d, (double)r);
            r /= 2;
            Assert.AreEqual(0.55d, (double)r);

            r = d.Float / 1.1;
            Assert.AreEqual(1d, (double)r, 0.00001);
            r /= 2;
            Assert.AreEqual(0.5d, (double)r, 0.00001);

            r = d.Float / 1.1d;
            Assert.AreEqual(1m, (decimal)r);
            r /= 2;
            Assert.AreEqual(0.5m, (decimal)r);

            r = d.Float / null;
            Assert.AreEqual(null, r.Value);
            r /= 2;
            Assert.AreEqual(null, r.Value);

            r = d.Decimal / 1;
            Assert.AreEqual(1.1m, (decimal)r);
            r /= 2;
            Assert.AreEqual(0.55m, (decimal)r);

            r = d.Decimal / 1.1;
            Assert.AreEqual(1m, (decimal)r);
            r /= 2;
            Assert.AreEqual(0.5m, (decimal)r);

            r = d.Decimal / 1.1d;
            Assert.AreEqual(1m, (decimal)r);
            r /= 2;
            Assert.AreEqual(0.5m, (decimal)r);

            r = d.Decimal / null;
            Assert.AreEqual(null, r.Value);
            r /= 2;
            Assert.AreEqual(null, r.Value);

#if !(NET20 || NET35 || PORTABLE)
            r = d.BigInteger / 1.1d;
            Assert.AreEqual(100m, (decimal)r);
            r /= 2;
            Assert.AreEqual(50m, (decimal)r);

            r = d.BigInteger / null;
            Assert.AreEqual(null, r.Value);
            r /= 2;
            Assert.AreEqual(null, r.Value);
#endif
            #endregion
        }

        [Test]
        public void JValueToString()
        {
            JObject o = new JObject(
                new JProperty("Null", JValue.CreateNull()),
                new JProperty("Integer", new JValue(1)),
                new JProperty("Float", new JValue(1.1)),
                new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
                new JProperty("Boolean", new JValue(true)),
                new JProperty("String", new JValue("A string lol!")),
                new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
                new JProperty("Uri", new Uri("http://json.codeplex.com/")),
                new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
                new JProperty("TimeSpan", TimeSpan.FromDays(1))
#if !(NET20 || NET35 || PORTABLE)
                , new JProperty("BigInteger", new BigInteger(100))
#endif
                );

            dynamic d = o;

            Assert.AreEqual("", d.Null.ToString());
            Assert.AreEqual("1", d.Integer.ToString());
            Assert.AreEqual("1.1", d.Float.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("12/29/2000 23:51:10", d.DateTime.ToString(null, CultureInfo.InvariantCulture));
            Assert.AreEqual("True", d.Boolean.ToString());
            Assert.AreEqual("A string lol!", d.String.ToString());
            Assert.AreEqual("System.Byte[]", d.Bytes.ToString());
            Assert.AreEqual("http://json.codeplex.com/", d.Uri.ToString());
            Assert.AreEqual("ea27fe1d-0d80-44f2-bf34-4654156fa7af", d.Guid.ToString());
            Assert.AreEqual("1.00:00:00", d.TimeSpan.ToString());
#if !(NET20 || NET35 || PORTABLE)
            Assert.AreEqual("100", d.BigInteger.ToString());
#endif
        }

        [Test]
        public void JObjectGetDynamicPropertyNames()
        {
            JObject o = new JObject(
                new JProperty("ChildValue", "blah blah"),
                new JProperty("Hello Joe", null));

            dynamic d = o;

            List<string> memberNames = o.GetDynamicMemberNames().ToList();

            Assert.AreEqual(2, memberNames.Count);
            Assert.AreEqual("ChildValue", memberNames[0]);
            Assert.AreEqual("Hello Joe", memberNames[1]);

            o = new JObject(
                new JProperty("ChildValue1", "blah blah"),
                new JProperty("Hello Joe1", null));

            d = o;

            memberNames = o.GetDynamicMemberNames().ToList();

            Assert.AreEqual(2, memberNames.Count);
            Assert.AreEqual("ChildValue1", memberNames[0]);
            Assert.AreEqual("Hello Joe1", memberNames[1]);
        }

        [Test]
        public void JValueConvert()
        {
            AssertValueConverted<bool>(true);
            AssertValueConverted<bool?>(true);
            AssertValueConverted<bool?>(false);
            AssertValueConverted<bool?>(null);
            AssertValueConverted<bool?>("true", true);
            AssertValueConverted<byte[]>(null);
            AssertValueConverted<byte[]>(Encoding.UTF8.GetBytes("blah"));
            AssertValueConverted<DateTime>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
            AssertValueConverted<DateTime?>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
            AssertValueConverted<DateTime?>(null);
            AssertValueConverted<DateTimeOffset>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
            AssertValueConverted<DateTimeOffset?>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
            AssertValueConverted<DateTimeOffset?>(null);
            AssertValueConverted<decimal>(99.9m);
            AssertValueConverted<decimal?>(99.9m);
            AssertValueConverted<decimal>(1m);
            AssertValueConverted<decimal>(1.1f, 1.1m);
            AssertValueConverted<decimal>("1.1", 1.1m);
            AssertValueConverted<double>(99.9);
            AssertValueConverted<double>(99.9d);
            AssertValueConverted<double?>(99.9d);
            AssertValueConverted<float>(99.9f);
            AssertValueConverted<float?>(99.9f);
            AssertValueConverted<int>(int.MinValue);
            AssertValueConverted<int?>(int.MinValue);
            AssertValueConverted<long>(long.MaxValue);
            AssertValueConverted<long?>(long.MaxValue);
            AssertValueConverted<short>(short.MaxValue);
            AssertValueConverted<short?>(short.MaxValue);
            AssertValueConverted<string>("blah");
            AssertValueConverted<string>(null);
            AssertValueConverted<string>(1, "1");
            AssertValueConverted<uint>(uint.MinValue);
            AssertValueConverted<uint?>(uint.MinValue);
            AssertValueConverted<uint?>("1", (uint)1);
            AssertValueConverted<ulong>(ulong.MaxValue);
            AssertValueConverted<ulong?>(ulong.MaxValue);
            AssertValueConverted<ushort>(ushort.MinValue);
            AssertValueConverted<ushort?>(ushort.MinValue);
            AssertValueConverted<ushort?>(null);
            AssertValueConverted<TimeSpan>(TimeSpan.FromDays(1));
            AssertValueConverted<TimeSpan?>(TimeSpan.FromDays(1));
            AssertValueConverted<TimeSpan?>(null);
            AssertValueConverted<Guid>(new Guid("60304274-CD13-4060-B38C-057C8557AB54"));
            AssertValueConverted<Guid?>(new Guid("60304274-CD13-4060-B38C-057C8557AB54"));
            AssertValueConverted<Guid?>(null);
            AssertValueConverted<Uri>(new Uri("http://json.codeplex.com/"));
            AssertValueConverted<Uri>(null);
#if !(NET20 || NET35 || PORTABLE)
            AssertValueConverted<BigInteger>(new BigInteger(100));
            AssertValueConverted<BigInteger?>(null);
#endif
        }

        private static void AssertValueConverted<T>(object value)
        {
            AssertValueConverted<T>(value, value);
        }

        private static void AssertValueConverted<T>(object value, object expected)
        {
            JValue v = new JValue(value);
            dynamic d = v;

            T t = d;
            Assert.AreEqual(expected, t);
        }

        [Test]
        public void DynamicSerializerExample()
        {
            dynamic value = new DynamicDictionary();

            value.Name = "Arine Admin";
            value.Enabled = true;
            value.Roles = new[] { "Admin", "User" };

            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            // {
            //   "Name": "Arine Admin",
            //   "Enabled": true,
            //   "Roles": [
            //     "Admin",
            //     "User"
            //   ]
            // }

            dynamic newValue = JsonConvert.DeserializeObject<DynamicDictionary>(json);

            string role = newValue.Roles[0];
            // Admin
        }

        [Test]
        public void DynamicLinqExample()
        {
            JObject oldAndBusted = new JObject();
            oldAndBusted["Name"] = "Arnie Admin";
            oldAndBusted["Enabled"] = true;
            oldAndBusted["Roles"] = new JArray(new[] { "Admin", "User" });

            string oldRole = (string)oldAndBusted["Roles"][0];
            // Admin


            dynamic newHotness = new JObject();
            newHotness.Name = "Arnie Admin";
            newHotness.Enabled = true;
            newHotness.Roles = new JArray(new[] { "Admin", "User" });

            string newRole = newHotness.Roles[0];
            // Admin

            Assert.AreEqual("Admin", oldRole);
            Assert.AreEqual("Admin", newRole);
        }

        [Test]
        public void ImprovedDynamicLinqExample()
        {
            dynamic product = new JObject();
            product.ProductName = "Elbow Grease";
            product.Enabled = true;
            product.Price = 4.90m;
            product.StockCount = 9000;
            product.StockValue = 44100;

            // All Elbow Grease must go sale!
            // 50% off price

            product.Price = product.Price / 2;
            product.StockValue = product.StockCount * product.Price;
            product.ProductName = product.ProductName + " (SALE)";

            string json = product.ToString();
            // {
            //   "ProductName": "Elbow Grease (SALE)",
            //   "Enabled": true,
            //   "Price": 2.45,
            //   "StockCount": 9000,
            //   "StockValue": 22050.00
            // }

            Assert.AreEqual(@"{
  ""ProductName"": ""Elbow Grease (SALE)"",
  ""Enabled"": true,
  ""Price"": 2.45,
  ""StockCount"": 9000,
  ""StockValue"": 22050.00
}", json);
        }
    }

    public class DynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _values[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = value;
            return true;
        }
    }
}

#endif