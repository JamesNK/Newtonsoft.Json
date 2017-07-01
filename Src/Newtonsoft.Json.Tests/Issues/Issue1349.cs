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

#if NETSTANDARD1_3
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1349 : TestFixtureBase
    {
        // --------------------------------------------------------
        // Sample Classes Group1 to test usage with ModelMetadataType
        // --------------------------------------------------------
        // assume that this class is auto generated one
        public partial class UserModel1
        {
            public string Username { get; set; }
            public string NameSurname { get; set; }
            public string Password { get; set; }
        }

        // we extend it by another partial class within different file
        [DataContract]
        [ModelMetadataType(typeof(UserModelMetaData1))]
        partial class UserModel1
        {

        }

        // this is the metadata class for model
        public class UserModelMetaData1
        {
            [DataMember]
            public string Username { get; set; }

            [DataMember]
            public string NameSurname { get; set; }

            //[DataMember]
            public string Password { get; set; }
        }



        // --------------------------------------------------------
        // Sample Classes Group2 to test usage without ModelMetadataType
        // --------------------------------------------------------
        // assume that this class is auto generated one

        [DataContract]
        public partial class UserModel2
        {
            [DataMember]
            public string Username { get; set; }

            [DataMember]
            public string NameSurname { get; set; }

            //[DataMember]
            public string Password { get; set; }
        }
        

        [Test]
        public void Test1_with_ModelMetadataType()
        {
            var model = new UserModel1()
            {
                Username = "tursoft",
                NameSurname = "Muhammet Tursak",
                Password = "12345"
            }; 

            var json = JsonConvert.SerializeObject(model);
            Assert.IsTrue(!json.Contains("\"Password\""));
        }

        [Test]
        public void Test2_without_ModelMetadataType()
        {
            var model = new UserModel2()
            {
                Username = "tursoft",
                NameSurname = "Muhammet Tursak",
                Password = "12345"
            };

            var json = JsonConvert.SerializeObject(model);
            Assert.IsTrue(!json.Contains("\"Password\""));
        }
    }
}
#endif
