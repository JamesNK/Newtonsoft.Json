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

//only perform on non .netcore platform
#if!DNXCORE50
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Isue2406
    {
        [Test]
        public void Can_Deserialize_Sql_Exception()
        {
            SqlException exception;
            
            try
            {
                
                throw SqlExceptionCreator.NewSqlException(101);
            } catch(SqlException ex)
            {
                exception = ex;
            }
            
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.TypeNameHandling = TypeNameHandling.Auto;
                        
            string serializedResult = JsonConvert.SerializeObject(exception, Formatting.None, jsonSettings);
            var deserializedresult = JsonConvert.DeserializeObject<SqlException>(serializedResult, jsonSettings);
            Assert.AreEqual( exception.ToString(), deserializedresult.ToString(), "Exception ToString() results where not the same!.");
        }

        public class SqlExceptionCreator
        {
            private static T Construct<T>(params object[] p)
            {
                var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
            }

            internal static SqlException NewSqlException(int number = 1)
            {
                SqlErrorCollection collection = Construct<SqlErrorCollection>();
                SqlError error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", "error message", "proc", 100);

                typeof(SqlErrorCollection)
                    .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(collection, new object[] { error });


                return typeof(SqlException)
                    .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
                        null,
                        CallingConventions.ExplicitThis,
                        new[] { typeof(SqlErrorCollection), typeof(string) },
                        new ParameterModifier[] { })
                    .Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
            }
        }     
    }
}
#endif