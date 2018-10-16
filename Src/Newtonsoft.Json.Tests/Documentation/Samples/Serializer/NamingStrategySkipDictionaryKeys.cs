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
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class NamingStrategySkipDictionaryKeys : TestFixtureBase
    {
        #region Types
        public class DailyHighScores
        {
            public DateTime Date { get; set; }
            public string Game { get; set; }
            public Dictionary<string, int> UserPoints { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            DailyHighScores dailyHighScores = new DailyHighScores
            {
                Date = new DateTime(2016, 6, 27, 0, 0, 0, DateTimeKind.Utc),
                Game = "Donkey Kong",
                UserPoints = new Dictionary<string, int>
                {
                    ["JamesNK"] = 9001,
                    ["JoC"] = 1337,
                    ["JessicaN"] = 1000
                }
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            };

            string json = JsonConvert.SerializeObject(dailyHighScores, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            Console.WriteLine(json);
            // {
            //   "date": "2016-06-27T00:00:00Z",
            //   "game": "Donkey Kong",
            //   "userPoints": {
            //     "JamesNK": 9001,
            //     "JoC": 1337,
            //     "JessicaN": 1000
            //   }
            // }
            #endregion

            StringAssert.AreEqual(@"{
  ""date"": ""2016-06-27T00:00:00Z"",
  ""game"": ""Donkey Kong"",
  ""userPoints"": {
    ""JamesNK"": 9001,
    ""JoC"": 1337,
    ""JessicaN"": 1000
  }
}", json);
        }
    }
}