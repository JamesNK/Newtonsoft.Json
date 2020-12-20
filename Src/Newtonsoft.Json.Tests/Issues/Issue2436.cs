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

#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2436
	{
		[Test]
		public void TestMaxDepth()
		{
			// With a value of 150, this stackoverflows when run in .NET Core and passes when run in .NET Framework.
			// Even much higher values (e. g. 500) pass on .NET Framework without incident.
			const int MaxDepth = 150;

			var path = new List<string>();

			var deeplyNested = new Nested();
			var inner = deeplyNested;
			for (var i = 0; i < MaxDepth + 1; ++i)
			{
				inner.Inner = new Nested();
				inner = inner.Inner;

				path.Add("Inner");
			}

			var serialized = JsonConvert.SerializeObject(deeplyNested);

			var exception = ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Nested>(serialized, new JsonSerializerSettings { MaxDepth = MaxDepth }));
			
			string joinedPath = string.Join(".", Enumerable.Repeat("Inner", MaxDepth).ToArray());
			string message = $@"The reader's MaxDepth of 150 has been exceeded. Path '{joinedPath}', line 1, position 1351.";
			Assert.AreEqual(message, exception.Message);
		}

		internal class Nested
		{
			public Nested Inner { get; set; }
		}
	}
}