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

#if !(PORTABLE || PORTABLE40 || DNXCORE50)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
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
    public class Issue1642 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            AssemblyName aName = new AssemblyName("TempAssembly");
            AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(
                aName, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            TypeBuilder typeBuilder = mb.DefineType("TestEnum", TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(Enum));
            typeBuilder.DefineField("value__", typeof(int), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

            FieldBuilder fieldBuilder = typeBuilder.DefineField("TestValue", typeBuilder, FieldAttributes.Family | FieldAttributes.Static | FieldAttributes.Literal);
            fieldBuilder.SetConstant(0);

            Type enumType = typeBuilder.CreateType();

            object o = Activator.CreateInstance(enumType);

            string json = JsonConvert.SerializeObject(o, new JsonSerializerSettings { Converters = { new StringEnumConverter() } });
            Assert.AreEqual(@"""TestValue""", json);
        }
    }
}
#endif