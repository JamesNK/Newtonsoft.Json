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

#if !(NET20 || NET35 || NET40 || DNXCORE50)

using System;
using System.Collections.Generic;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Autofac;
using Newtonsoft.Json.Tests.Serialization;
using LogService = Newtonsoft.Json.Tests.Serialization.LogManager;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class DeserializeWithDependencyInjection : TestFixtureBase
    {
        #region Types
        public class AutofacContractResolver : DefaultContractResolver
        {
            private readonly IContainer _container;

            public AutofacContractResolver(IContainer container)
            {
                _container = container;
            }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);

                // use Autofac to create types that have been registered with it
                if (_container.IsRegistered(objectType))
                {
                    contract.DefaultCreator = () => _container.Resolve(objectType);
                }

                return contract;
            }
        }

        public class TaskController
        {
            private readonly ITaskRepository _repository;
            private readonly ILogger _logger;

            public TaskController(ITaskRepository repository, ILogger logger)
            {
                _repository = repository;
                _logger = logger;
            }

            public ITaskRepository Repository
            {
                get { return _repository; }
            }

            public ILogger Logger
            {
                get { return _logger; }
            }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<TaskRepository>().As<ITaskRepository>();
            builder.RegisterType<TaskController>();
            builder.Register(c => new LogService(new DateTime(2000, 12, 12))).As<ILogger>();

            IContainer container = builder.Build();

            AutofacContractResolver contractResolver = new AutofacContractResolver(container);

            string json = @"{
              'Logger': {
                'Level':'Debug'
              }
            }";

            // ITaskRespository and ILogger constructor parameters are injected by Autofac 
            TaskController controller = JsonConvert.DeserializeObject<TaskController>(json, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            Console.WriteLine(controller.Repository.GetType().Name);
            // TaskRepository
            #endregion

            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Logger);

            Assert.AreEqual(new DateTime(2000, 12, 12), controller.Logger.DateTime);
            Assert.AreEqual("Debug", controller.Logger.Level);
        }
    }
}

#endif