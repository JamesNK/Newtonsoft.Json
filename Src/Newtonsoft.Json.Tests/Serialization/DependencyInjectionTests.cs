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

using Autofac;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if !(NET35 || NET20)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
    public interface ITaskRepository
    {
        string ConnectionString { get; set; }
    }

    public interface ILogger
    {
        DateTime DateTime { get; }
        string Level { get; set; }
    }

    public class TaskRepository : ITaskRepository
    {
        public string ConnectionString { get; set; }
    }

    public class LogManager : ILogger
    {
        private readonly DateTime _dt;

        public LogManager(DateTime dt)
        {
            _dt = dt;
        }

        public DateTime DateTime
        {
            get { return _dt; }
        }

        public string Level { get; set; }
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

    public class HasSettableProperty
    {
        public ILogger Logger { get; set; }
        public ITaskRepository Repository { get; set; }
        public IList<Person> People { get; set; }
        public Person Person { get; set; }

        public HasSettableProperty(ILogger logger)
        {
            Logger = logger;
        }
    }

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
                contract.DefaultCreator = () => _container.Resolve(objectType);

            return contract;
        }
    }

    [TestFixture]
    public class DependencyInjectionTests : TestFixtureBase
    {
        [Test]
        public void CreateObjectWithParameters()
        {
            int count = 0;

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<TaskRepository>().As<ITaskRepository>();
            builder.RegisterType<TaskController>();
            builder.Register(c =>
            {
                count++;
                return new LogManager(new DateTime(2000, 12, 12));
            }).As<ILogger>();

            IContainer container = builder.Build();

            AutofacContractResolver contractResolver = new AutofacContractResolver(container);

            TaskController controller = JsonConvert.DeserializeObject<TaskController>(@"{
                'Logger': {
                    'Level':'Debug'
                }
            }", new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Logger);

            Assert.AreEqual(1, count);

            Assert.AreEqual(new DateTime(2000, 12, 12), controller.Logger.DateTime);
            Assert.AreEqual("Debug", controller.Logger.Level);
        }

        [Test]
        public void CreateObjectWithSettableParameter()
        {
            int count = 0;

            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c =>
            {
                count++;
                return new TaskRepository();
            }).As<ITaskRepository>();
            builder.RegisterType<HasSettableProperty>();
            builder.Register(c =>
            {
                count++;
                return new LogManager(new DateTime(2000, 12, 12));
            }).As<ILogger>();

            IContainer container = builder.Build();

            AutofacContractResolver contractResolver = new AutofacContractResolver(container);

            HasSettableProperty o = JsonConvert.DeserializeObject<HasSettableProperty>(@"{
                'Logger': {
                    'Level': 'Debug'
                },
                'Repository': {
                    'ConnectionString': 'server=.'
                },
                'People': [
                    {
                        'Name': 'Name1!'
                    },
                    {
                        'Name': 'Name2!'
                    }
                ],
                'Person': {
                    'Name': 'Name3!'
                }
            }", new JsonSerializerSettings
              {
                  ContractResolver = contractResolver
              });

            Assert.IsNotNull(o);
            Assert.IsNotNull(o.Logger);
            Assert.IsNotNull(o.Repository);

            Assert.AreEqual(2, count);

            Assert.AreEqual(new DateTime(2000, 12, 12), o.Logger.DateTime);
            Assert.AreEqual("Debug", o.Logger.Level);
            Assert.AreEqual("server=.", o.Repository.ConnectionString);
            Assert.AreEqual(2, o.People.Count);
            Assert.AreEqual("Name1!", o.People[0].Name);
            Assert.AreEqual("Name2!", o.People[1].Name);
            Assert.AreEqual("Name3!", o.Person.Name);
        }
    }
}
#endif