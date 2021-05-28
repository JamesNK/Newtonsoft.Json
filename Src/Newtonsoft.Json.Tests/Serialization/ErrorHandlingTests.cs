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

#if DNXCORE50
	using Xunit;
	using Test = Xunit.FactAttribute;
	using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
	using NUnit.Framework;
#endif

using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.TestObjects
{
	internal class ClassWithObjectProperty
	{
		public Aa Prop1;
		public String Prop2;
	}

	internal class ClassWithValueProperty
	{
		public String Prop1;
		public String Prop2;
	}
}

namespace Newtonsoft.Json.Tests.Serialization
{
	[TestFixture]
	public class ErrorHandlingTests : TestFixtureBase
	{
		[Test]
		public void DeserializePropertyWithValueTypeFromTypeName()
		{
			string json = @"{
  ""Prop1"": 1,
  ""Prop2"": ""Test Value 2""
}";
			var value = JsonConvert.DeserializeObject<ClassWithObjectProperty>(json, new JsonSerializerSettings
			{
				Error = (sender, args) => { args.ErrorContext.Handled = true; }

			});

			Assert.IsNotNull(value);
			Assert.AreEqual("Test Value 2", value.Prop2);
		}


		[Test]
		public void DeserializePropertyWithTypeNameFromValueType()
		{
			string json = @"{
  ""Prop1"": {
    ""Prop1"": 1
  },
  ""Prop2"": ""Test Value 2""
}";
			var value = JsonConvert.DeserializeObject<ClassWithValueProperty>(json, new JsonSerializerSettings
			{
				Error = (sender, args) => { args.ErrorContext.Handled = true; }			
			});

			Assert.IsNotNull(value);
			Assert.AreEqual("Test Value 2", value.Prop2);
		}
	}
}