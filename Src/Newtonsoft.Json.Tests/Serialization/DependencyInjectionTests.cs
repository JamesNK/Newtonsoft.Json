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

#if !(NET40 || NET35 || NET20 || DNXCORE50 || PORTABLE || PORTABLE40)
using Autofac;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.FSharp.Collections;
using Newtonsoft.Json.Tests.TestObjects.Organization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    public interface IBase
    {
        DateTime CreatedOn { get; set; }
    }

    public interface ITaskRepository : IBase
    {
        string ConnectionString { get; set; }
    }

    public interface ILogger
    {
        DateTime DateTime { get; }
        string Level { get; set; }
    }

    public class Base : IBase
    {
        public DateTime CreatedOn { get; set; }
    }

    public class TaskRepository : Base, ITaskRepository
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

#if !NET20
    [DataContract]
    public class User
    {
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "company")]
        public ICompany Company { get; set; }
    }

    public interface ICompany
    {
        string CompanyName { get; set; }
    }

    [DataContract]
    public class Company : ICompany
    {
        [DataMember(Name = "company_name")]
        public string CompanyName { get; set; }
    }
#endif

    public class AutofacContractResolver : DefaultContractResolver
    {
        private readonly IContainer _container;

        public AutofacContractResolver(IContainer container)
        {
            _container = container;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            // use Autofac to create types that have been registered with it
            if (_container.IsRegistered(objectType))
            {
                JsonObjectContract contract = ResolveContact(objectType);
                contract.DefaultCreator = () => _container.Resolve(objectType);

                return contract;
            }

            return base.CreateObjectContract(objectType);
        }

        private JsonObjectContract ResolveContact(Type objectType)
        {
            // attempt to create the contact from the resolved type
            IComponentRegistration registration;
            if (_container.ComponentRegistry.TryGetRegistration(new TypedService(objectType), out registration))
            {
                Type viewType = (registration.Activator as ReflectionActivator)?.LimitType;
                if (viewType != null)
                {
                    return base.CreateObjectContract(viewType);
                }
            }

            // fall back to using the registered type
            return base.CreateObjectContract(objectType);
        }
    }

    [TestFixture]
    public class DependencyInjectionTests : TestFixtureBase
    {
        [Test]
        public void ResolveContractFromAutofac()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<Company>().As<ICompany>();
            IContainer container = builder.Build();

            AutofacContractResolver resolver = new AutofacContractResolver(container);

            User user = JsonConvert.DeserializeObject<User>("{'company':{'company_name':'Company name!'}}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual("Company name!", user.Company.CompanyName);
        }

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
                    'ConnectionString': 'server=.',
                    'CreatedOn': '2015-04-01 20:00'
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
            Assert.AreEqual(o.Repository.CreatedOn, DateTime.Parse("2015-04-01 20:00"));

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