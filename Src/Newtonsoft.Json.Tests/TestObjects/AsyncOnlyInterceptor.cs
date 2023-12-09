#if HAVE_ASYNC
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.TestObjects
{
    internal class AsyncOnlyInterceptor : IInterceptor
    {
        private string[] _disallowedVerbs;
        public AsyncOnlyInterceptor(params string[] disallowedVerbs)
        {
            _disallowedVerbs = disallowedVerbs;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_disallowedVerbs.Any(v => invocation.Method.Name.StartsWith(v))
                && !invocation.Method.Name.EndsWith("Async"))
            {
                Assert.Fail($"Synchronous {invocation.Method.Name} called.");
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
#endif