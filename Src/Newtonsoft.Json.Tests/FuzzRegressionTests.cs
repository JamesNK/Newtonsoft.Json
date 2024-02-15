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

using System.IO;

#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class FuzzRegressionTests : TestFixtureBase
    {
        static readonly JsonSerializer jsonSerializer = new();

        public static void Roundtrip(string buffer)
        {
            var serialized1 = Serialize(Deserialize(buffer));
            var serialized2 = Serialize(Deserialize(serialized1));
            Assert.Equals(serialized1, serialized2);
        }

        private static string Serialize(object o)
        {
            using var sw1 = new StringWriter();
            jsonSerializer.Serialize(sw1, o);
            return sw1.ToString();
        }

        private static object Deserialize(string input)
        {
            using var sr = new StringReader(input);
            using var tr = new JsonTextReader(sr);
            return jsonSerializer.Deserialize(tr);
        }

        [Test]
        public void LineCommentWithStarSlashCases()
        {
            // Fuzzer found these cases:
            // The Json "// comment with */" would be serialized as "/* comment with */*/".
            // It is now serialized as "// comment with */".
            //
            // Ideally this would be a [Theory] but we are targetting both NUnit and XUnit.
            // 
            // This is a list of inputs and where they caused a crash.
            // - Note that these are not minimal/reduced inputs.
            var cases = new[] {
                "[//*/I33/\n91.//3", // ParsePositiveInfinity
                "[//*/t3.9/\n91675795,77//3", // ParseTrue
                "[//*/f33339/\n91675795878,787,[]//3", // ParseFalse
                "[//*/N333331/\n,[]//3", // ParseNumberNaN
                "[//*/{/\n,7//J:[@", // ParseProperty
                "[//*/06.6/\n,[]//3", // ParseNumber
                "[5,78,,[//*/3,3,new 4* ,,,,,,33/\n,,,,[],33,,,,,,17911055//3814", // ParseConstructor
                "[//*/---------------------------------------------------------------------------------------------------------16.6/\n,[]//3", // ParseReadNumber
                "[//6*/)3333/\n,[]//3", // ValidateEnd
                "[//*/{'33/\n,33,33//3", // ReadStringIntoBuffer
                "[//*//\n8 ,8 ", // ParseComment
                @"[//ô   [ / 
//""ÿ /'/*//// 
//[7
// 
//"" @'/// 
////
//// 
//[6
//
//
//[1
// ""  x

///***
   //[2
// 
,'/J ""' ", // WriteToken
                "[2//***;****/,7*", // ReadNumberCharIntoBuffer
                "[2///ÿ¢  ¢¢********/,,*		", // ParseValue
                @"[// *//[
7// 
// ""JJ· 
//',J
 
// 
//',o@7,7
//',o@/ / "" ]      
//',o@7,7
//',o@Ó", // ParseComment
                "[2//*/*", // ParsePostValue
                "[//*/{A73/\n]1.//3:{\"'\":", //  ReadUnquotedPropertyReportIfDone
            };

            foreach (var c in cases) {
                Roundtrip(c);
            }
        }
    }
}
