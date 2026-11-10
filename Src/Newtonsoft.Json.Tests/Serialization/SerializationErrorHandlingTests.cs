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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
#if !(NET20 || NET35 || PORTABLE40)
using System.Dynamic;
#endif
#if !(NET20 || NET35)
using System.Runtime.ExceptionServices;
using System.Threading;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
using Newtonsoft.Json.Linq;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class SerializationErrorHandlingTests : TestFixtureBase
    {
#if !(NET20 || NET35)
        [Test]
        public void DeserializationErrorsDoNotRedispatchPerContainer()
        {
            string json = string.Concat(Enumerable.Repeat("{\"Child\":", 9)) + "null" + new string('}', 9);

            foreach (bool populate in new[] { false, true })
            {
                foreach (string observer in new[] { "none", "log", "callback", "emptyCallbacks", "traceOff", "traceError" })
                {
                    int notifications = 0;
                    int callbacks = 0;
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    EventHandler<FirstChanceExceptionEventArgs> handler = (sender, args) =>
                    {
                        if (Thread.CurrentThread.ManagedThreadId == threadId && args.Exception is JsonReaderException)
                        {
                            notifications++;
                        }
                    };
                    JsonSerializerSettings settings = new JsonSerializerSettings { MaxDepth = 8 };
                    MemoryTraceWriter trace = null;
                    if (observer == "log")
                    {
                        settings.Error = (sender, args) => callbacks++;
                    }
                    else if (observer == "callback" || observer == "emptyCallbacks")
                    {
                        DefaultContractResolver resolver = new DefaultContractResolver();
                        IList<SerializationErrorCallback> errorCallbacks = resolver.ResolveContract(typeof(ErrorUnwindNode)).OnErrorCallbacks;
                        if (observer == "callback")
                        {
                            errorCallbacks.Add((obj, context, error) => callbacks++);
                        }
                        settings.ContractResolver = resolver;
                    }
                    else if (observer == "traceOff" || observer == "traceError")
                    {
                        trace = new MemoryTraceWriter { LevelFilter = observer == "traceOff" ? TraceLevel.Off : TraceLevel.Error };
                        settings.TraceWriter = trace;
                    }

                    AppDomain.CurrentDomain.FirstChanceException += handler;
                    try
                    {
                        ExceptionAssert.Throws<JsonReaderException>(() =>
                        {
                            if (populate)
                            {
                                JsonConvert.PopulateObject(json, new ErrorUnwindNode(), settings);
                            }
                            else
                            {
                                JsonConvert.DeserializeObject<ErrorUnwindNode>(json, settings);
                            }
                        });
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.FirstChanceException -= handler;
                    }

                    bool observed = observer != "none" && observer != "emptyCallbacks";
                    Assert.AreEqual(observed ? (populate ? 9 : 10) : (populate ? 2 : 3), notifications);
                    Assert.AreEqual(observer == "log" ? (populate ? 8 : 9) : (observer == "callback" ? 8 : 0), callbacks);
                    if (trace != null)
                    {
                        Assert.AreEqual(observer == "traceError" ? 1 : 0, trace.GetTraceMessages().Count());
                    }
                }
            }
        }

        [Test]
        public void UnobservedMultidimensionalArrayErrorDoesNotRedispatchPerContainer()
        {
            string json = "[[" + string.Concat(Enumerable.Repeat("{\"Child\":", 9)) + "null" + new string('}', 9) + "]]";
            int notifications = 0;
            int threadId = Thread.CurrentThread.ManagedThreadId;
            EventHandler<FirstChanceExceptionEventArgs> handler = (sender, args) =>
            {
                if (Thread.CurrentThread.ManagedThreadId == threadId && args.Exception is JsonReaderException)
                {
                    notifications++;
                }
            };

            AppDomain.CurrentDomain.FirstChanceException += handler;
            try
            {
                ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<ErrorUnwindNode[,]>(json, new JsonSerializerSettings { MaxDepth = 8 }));
            }
            finally
            {
                AppDomain.CurrentDomain.FirstChanceException -= handler;
            }

            Assert.AreEqual(3, notifications);
        }

        [Test]
        public void SerializationErrorsDoNotRedispatchWithoutObservers()
        {
            ErrorUnwindNode value = new ErrorUnwindNode();
            for (int i = 1; i < 8; i++)
            {
                value = new ErrorUnwindNode { Child = value };
            }

            foreach (string observer in new[] { "none", "log", "callback", "emptyCallbacks", "traceOff", "traceError" })
            {
                int notifications = 0;
                int callbacks = 0;
                int threadId = Thread.CurrentThread.ManagedThreadId;
                EventHandler<FirstChanceExceptionEventArgs> handler = (sender, args) =>
                {
                    if (Thread.CurrentThread.ManagedThreadId == threadId && args.Exception is JsonSerializationException)
                    {
                        notifications++;
                    }
                };
                ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
                JsonSerializerSettings settings = new JsonSerializerSettings { Converters = { converter } };
                MemoryTraceWriter trace = null;
                if (observer == "log")
                {
                    settings.Error = (sender, args) => callbacks++;
                }
                else if (observer == "callback" || observer == "emptyCallbacks")
                {
                    DefaultContractResolver resolver = new DefaultContractResolver();
                    IList<SerializationErrorCallback> errorCallbacks = resolver.ResolveContract(typeof(ErrorUnwindNode)).OnErrorCallbacks;
                    if (observer == "callback")
                    {
                        errorCallbacks.Add((obj, context, error) => callbacks++);
                    }
                    settings.ContractResolver = resolver;
                }
                else if (observer == "traceOff" || observer == "traceError")
                {
                    trace = new MemoryTraceWriter { LevelFilter = observer == "traceOff" ? TraceLevel.Off : TraceLevel.Error };
                    settings.TraceWriter = trace;
                }

                AppDomain.CurrentDomain.FirstChanceException += handler;
                try
                {
                    ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(value, settings));
                }
                finally
                {
                    AppDomain.CurrentDomain.FirstChanceException -= handler;
                }

                bool observed = observer != "none" && observer != "emptyCallbacks";
                Assert.AreEqual(observed ? 10 : 3, notifications);
                Assert.AreEqual(observer == "log" ? 9 : (observer == "callback" ? 8 : 0), callbacks);
                Assert.IsTrue(converter.CleanupCompleted);
                if (trace != null)
                {
                    Assert.AreEqual(observer == "traceError" ? 1 : 0, trace.GetTraceMessages().Count());
                }
            }
        }

        [Test]
        public void SerializationContainerErrorsDoNotRedispatchWithoutObservers()
        {
            List<object> values = new List<object>
            {
                new ErrorUnwindGetter(),
                new List<ErrorUnwindGetter> { new ErrorUnwindGetter() },
                new Dictionary<string, ErrorUnwindGetter> { { "x", new ErrorUnwindGetter() } },
                new ErrorUnwindGetter[,] { { new ErrorUnwindGetter() } }
            };
#if !(NET20 || NET35 || PORTABLE40)
            values.Add(new ErrorUnwindWriterDynamic { StaticValue = new ErrorUnwindGetter() });
            values.Add(new ErrorUnwindWriterDynamic { DynamicValue = new ErrorUnwindGetter() });
#endif

            foreach (object value in values)
            {
                foreach (string observer in new[] { "none", "log", "trace" })
                {
                    int notifications = 0;
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    EventHandler<FirstChanceExceptionEventArgs> handler = (sender, args) =>
                    {
                        if (Thread.CurrentThread.ManagedThreadId == threadId && args.Exception is JsonSerializationException)
                        {
                            notifications++;
                        }
                    };
                    List<ErrorEventArgs> errors = new List<ErrorEventArgs>();
                    MemoryTraceWriter trace = new MemoryTraceWriter { LevelFilter = TraceLevel.Error };
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    if (observer != "none")
                    {
                        settings.TraceWriter = trace;
                    }
                    if (observer == "log")
                    {
                        settings.Error = (sender, args) => errors.Add(args);
                    }

                    JsonSerializationException error;
                    AppDomain.CurrentDomain.FirstChanceException += handler;
                    try
                    {
                        error = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(value, settings));
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.FirstChanceException -= handler;
                    }

                    int catches = value is ErrorUnwindGetter ? 2 : 3;
                    Assert.AreEqual(observer == "none" ? 3 : catches + 1, notifications);
                    Assert.AreEqual(observer == "log" ? catches : 0, errors.Count);
                    foreach (ErrorEventArgs args in errors)
                    {
                        Assert.AreSame(errors[0].ErrorContext, args.ErrorContext);
                        Assert.AreSame(errors[0].CurrentObject, args.ErrorContext.OriginalObject);
                        Assert.AreSame(error, args.ErrorContext.Error);
                    }
                    string[] messages = trace.GetTraceMessages().ToArray();
                    Assert.AreEqual(observer == "none" ? 0 : 1, messages.Length);
                    if (messages.Length != 0)
                    {
                        Assert.IsTrue(messages[0].Contains("Error serializing " + typeof(ErrorUnwindGetter).FullName + "."));
                    }
                }
            }
        }
#endif

        [Test]
        public void SerializationErrorObserversRunAfterConverterCleanup()
        {
            foreach (bool callback in new[] { false, true })
            {
                ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
                ErrorUnwindCallbackNode value = new ErrorUnwindCallbackNode();
                JsonSerializerSettings settings = new JsonSerializerSettings { Converters = { converter } };
                int errors = 0;
                Action<ErrorContext> handleError = error =>
                {
                    errors++;
                    Assert.IsTrue(converter.CleanupCompleted);
                    Assert.AreSame(value, error.OriginalObject);
                    error.Handled = true;
                };
                if (callback)
                {
                    value.ErrorCallback = handleError;
                }
                else
                {
                    settings.Error = (sender, args) => handleError(args.ErrorContext);
                }

                string json = JsonConvert.SerializeObject(value, settings);

                Assert.AreEqual("{\"Child\":null,\"Value\":null}", json);
                Assert.AreEqual(1, errors);
            }
        }

        [Test]
        public void SerializationTraceLevelFilterIsNotEvaluatedBeforeConverterCleanup()
        {
            ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
            ErrorUnwindTraceWriter trace = new ErrorUnwindTraceWriter(converter);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(new ErrorUnwindNode(), new JsonSerializerSettings
            {
                Converters = { converter },
                TraceWriter = trace
            }));

            Assert.IsTrue(converter.CleanupCompleted);
            Assert.IsFalse(trace.ReadBeforeCleanup);
            Assert.AreEqual(1, trace.Errors);
        }

        [Test]
        public void DeserializationContainerErrorsKeepOriginalContext()
        {
            string[] json = { "{\"Child\":{\"Child\":1}}", "[[1]]", "{\"x\":{\"x\":1}}", "[[{\"Child\":1}]]" };
            Type[] types = { typeof(ErrorUnwindNode), typeof(List<List<ErrorUnwindNode>>), typeof(ErrorUnwindDictionary), typeof(ErrorUnwindNode[,]) };

            for (int i = 0; i < types.Length; i++)
            {
                List<ErrorEventArgs> errors = new List<ErrorEventArgs>();
                MemoryTraceWriter trace = new MemoryTraceWriter { LevelFilter = TraceLevel.Error };
                JsonException error = ExceptionAssert.Throws<JsonException>(() => JsonConvert.DeserializeObject(json[i], types[i], new JsonSerializerSettings
                {
                    TraceWriter = trace,
                    Error = (sender, args) => errors.Add(args)
                }));

                Assert.AreEqual(3, errors.Count);
                Assert.IsNotNull(errors[0].CurrentObject);
                Assert.IsNull(errors[2].CurrentObject);
                foreach (ErrorEventArgs args in errors)
                {
                    Assert.AreSame(errors[0].ErrorContext, args.ErrorContext);
                    Assert.AreSame(errors[0].CurrentObject, args.ErrorContext.OriginalObject);
                    Assert.AreSame(error, args.ErrorContext.Error);
                }
                Assert.AreEqual(1, trace.GetTraceMessages().Count());
            }
        }

        [Test]
        public void ParentOnlyCallbackKeepsDeserializationErrorOrigin()
        {
            AssertParentOnlyCallbackDeserializes<ErrorUnwindNode>("{\"Child\":{\"Value\":\"bad\"},\"After\":2}", "Value");
            AssertParentOnlyCallbackDeserializes<List<int>>("{\"Child\":[\"bad\"],\"After\":2}", 0);
            AssertParentOnlyCallbackDeserializes<Dictionary<string, int>>("{\"Child\":{\"x\":\"bad\"},\"After\":2}", "x");
            AssertParentOnlyCallbackDeserializes<RequiredErrorUnwindNode>("{\"Child\":{},\"After\":2}", "Name");
        }

        private static void AssertParentOnlyCallbackDeserializes<T>(string json, object member)
        {
            foreach (bool populate in new[] { false, true })
            {
                ParentOnlyErrorNode<T> parent;
                if (populate)
                {
                    parent = new ParentOnlyErrorNode<T>();
                    JsonConvert.PopulateObject(json, parent);
                }
                else
                {
                    parent = JsonConvert.DeserializeObject<ParentOnlyErrorNode<T>>(json);
                }

                Assert.IsNotNull(parent.Error);
                Assert.AreEqual(typeof(T), parent.Error.OriginalObject.GetType());
                Assert.AreEqual(member, parent.Error.Member);
                Assert.AreEqual(2, parent.After);
            }
        }

        [Test]
        public void ParentOnlyCallbackKeepsSerializationErrorOrigin()
        {
            ErrorUnwindGetter child = new ErrorUnwindGetter();
            object[] values =
            {
                child,
                new List<ErrorUnwindGetter> { child },
                new Dictionary<string, ErrorUnwindGetter> { { "x", child } },
                new ErrorUnwindGetter[,] { { child } }
            };

            foreach (object value in values)
            {
                ParentOnlyErrorNode<object> parent = new ParentOnlyErrorNode<object> { Child = value, After = 2 };

                string json = JsonConvert.SerializeObject(parent);

                Assert.IsNotNull(parent.Error);
                Assert.AreSame(child, parent.Error.OriginalObject);
                Assert.AreEqual("Value", parent.Error.Member);
                Assert.AreEqual(2, (int)JObject.Parse(json)["After"]);
            }
        }

        [Test]
        public void ParentErrorHandlerResumesWithoutCompletingFailedChild()
        {
            ErrorUnwindCallbackNode child = new ErrorUnwindCallbackNode();
            ErrorUnwindCallbackNode root = new ErrorUnwindCallbackNode { Child = child };
            root.ChildSetCount = 0;
            List<string> callbacks = new List<string>();
            child.ErrorCallback = context => callbacks.Add("child attribute");
            root.ErrorCallback = context =>
            {
                callbacks.Add("parent attribute");
                context.Handled = true;
            };

            JsonConvert.PopulateObject("{\"Child\":{\"Child\":1},\"Value\":2}", root, new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    Assert.AreSame(child, args.CurrentObject);
                    callbacks.Add("child event");
                }
            });

            CollectionAssert.AreEqual(new[] { "child attribute", "child event", "parent attribute" }, callbacks);
            Assert.AreSame(child, root.Child);
            Assert.AreEqual(0, child.ChildSetCount);
            Assert.AreEqual(0, root.ChildSetCount);
            Assert.IsFalse(child.Deserialized);
            Assert.IsTrue(root.Deserialized);
            Assert.AreEqual(2, root.Value);
        }

        [Test]
        public void UnhandledErrorDoesNotCompleteParents()
        {
            ErrorUnwindNode child = new ErrorUnwindNode();
            ErrorUnwindNode root = new ErrorUnwindNode { Child = child };
            root.ChildSetCount = 0;

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject("{\"Child\":{\"Child\":1},\"Value\":2}", root));

            Assert.AreSame(child, root.Child);
            Assert.AreEqual(0, child.ChildSetCount);
            Assert.AreEqual(0, root.ChildSetCount);
            Assert.IsFalse(child.Deserialized);
            Assert.IsFalse(root.Deserialized);
            Assert.AreEqual(0, root.Value);
        }

        [Test]
        public void UnhandledErrorDoesNotAddFailedCollectionItems()
        {
            List<List<ErrorUnwindNode>> root = new List<List<ErrorUnwindNode>>();
            List<object> objects = new List<object>();
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject("[[{\"Child\":1}]]", root, new JsonSerializerSettings
            {
                Error = (sender, args) => objects.Add(args.CurrentObject)
            }));

            Assert.AreEqual(3, objects.Count);
            Assert.IsFalse(((ErrorUnwindNode)objects[0]).Deserialized);
            Assert.AreEqual(0, ((List<ErrorUnwindNode>)objects[1]).Count);
            Assert.AreSame(root, objects[2]);
            Assert.AreEqual(0, root.Count);
        }

        [Test]
        public void UnhandledErrorDoesNotInvokeParameterizedCreators()
        {
            string[] json = { "{\"Child\":{\"Child\":1}}", "[{\"Child\":1}]", "{\"x\":{\"Child\":1}}" };
            Type[] types = { typeof(FailedObjectCreator), typeof(FailedCollectionCreator), typeof(FailedDictionaryCreator) };
            for (int i = 0; i < types.Length; i++)
            {
                JsonSerializationException error = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject(json[i], types[i]));
                Assert.IsTrue(error.Message.StartsWith("Error converting value 1", StringComparison.Ordinal));
            }
        }

        [Test]
        public void UnhandledErrorDoesNotSetExtensionData()
        {
            ErrorUnwindExtensionData root = new ErrorUnwindExtensionData();
            string json = "{\"extra\":{\"$type\":" + JsonConvert.ToString(typeof(ErrorUnwindNode).AssemblyQualifiedName) + ",\"Child\":1}}";
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject(json, root, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));

            Assert.AreEqual(0, root.Values.Count);
        }

        [Test]
        public void ConverterCanCatchAndWrapPendingError()
        {
            foreach (bool populate in new[] { false, true })
            {
                ErrorUnwindConverter converter = new ErrorUnwindConverter { Populate = populate };
                bool observedCleanup = false;
                bool reentered = false;
                ErrorContext firstContext = null;
                JsonSerializerSettings settings = new JsonSerializerSettings { Converters = { converter } };
                if (!populate)
                {
                    settings.Error = (sender, args) =>
                    {
                        if (firstContext == null)
                        {
                            firstContext = args.ErrorContext;
                        }
                        if (ReferenceEquals(args.ErrorContext.Error, firstContext.Error))
                        {
                            Assert.AreSame(firstContext, args.ErrorContext);
                        }
                        if (!reentered)
                        {
                            reentered = true;
                            using (JsonReader reader = new JsonTextReader(new StringReader("{\"Value\":4}")))
                            {
                                ErrorUnwindNode value = new ErrorUnwindNode();
                                converter.Serializer.Populate(reader, value);
                                Assert.AreEqual(4, value.Value);
                            }
                            using (JsonReader reader = new JsonTextReader(new StringReader("{\"Value\":3}")))
                            {
                                Assert.AreEqual(3, converter.Serializer.Deserialize<ErrorUnwindNode>(reader).Value);
                            }
                        }
                        if (args.ErrorContext.Error.Message == "Wrapped error.")
                        {
                            observedCleanup = converter.CleanupCompleted;
                        }
                    };
                }
                JsonSerializationException error = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ErrorUnwindConverted>("{\"Child\":{\"Child\":1}}", settings));

                Assert.AreEqual("Wrapped error.", error.Message);
                Assert.AreSame(converter.NestedError, error.InnerException);
                Assert.AreEqual(!populate, reentered);
                Assert.AreEqual(!populate, observedCleanup);
                Assert.IsTrue(converter.CleanupCompleted);
                Assert.IsTrue(converter.ReusedSerializer);
                Assert.IsTrue(error.InnerException.StackTrace.Contains("EnsureType"));
            }
        }

        [Test]
        public void ConverterCanCatchPopulateErrorWithObservers()
        {
            ErrorUnwindConverter converter = new ErrorUnwindConverter { Populate = true, WrapError = false };
            int errors = 0;
            ErrorUnwindConverted value = JsonConvert.DeserializeObject<ErrorUnwindConverted>("{\"Child\":{\"Child\":1}}", new JsonSerializerSettings
            {
                Converters = { converter },
                Error = (sender, args) => errors++
            });

            Assert.IsNotNull(value);
            Assert.AreEqual(2, errors);
            Assert.IsNotNull(converter.NestedError);
            Assert.IsTrue(converter.CleanupCompleted);
            Assert.IsTrue(converter.ReusedSerializer);
        }

        [Test]
        public void ConverterCanWrapPopulateErrorWithObservers()
        {
            ErrorUnwindConverter converter = new ErrorUnwindConverter { Populate = true };
            JsonSerializationException error = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ErrorUnwindConverted>("{\"Child\":{\"Child\":1}}", new JsonSerializerSettings
            {
                Converters = { converter },
                Error = (sender, args) => { }
            }));

            Assert.AreEqual("Wrapped error.", error.Message);
            Assert.AreSame(converter.NestedError, error.InnerException);
            Assert.IsTrue(converter.CleanupCompleted);
            Assert.IsTrue(converter.ReusedSerializer);
        }

        [Test]
        public void TraceOnlyErrorPreservesInnermostContract()
        {
            MemoryTraceWriter trace = new MemoryTraceWriter { LevelFilter = TraceLevel.Error };
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ErrorUnwindContainer>("{\"Child\":{\"Child\":1}}", new JsonSerializerSettings
            {
                TraceWriter = trace
            }));

            string[] messages = trace.GetTraceMessages().ToArray();
            Assert.AreEqual(1, messages.Length);
            Assert.IsTrue(messages[0].Contains("Error deserializing " + typeof(ErrorUnwindNode).FullName + "."));
            Assert.IsFalse(messages[0].Contains("Error deserializing " + typeof(ErrorUnwindContainer).FullName + "."));
        }

        [Test]
        public void ErrorEventExceptionPropagatesAfterConverterCleanup()
        {
            ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
            ErrorObserverException failure = new ErrorObserverException();
            ErrorObserverException error = ExceptionAssert.Throws<ErrorObserverException>(() => JsonConvert.PopulateObject("{\"Value\":1}", new ErrorUnwindNode(), new JsonSerializerSettings
            {
                Converters = { converter },
                Error = (sender, args) =>
                {
                    Assert.IsTrue(converter.CleanupCompleted);
                    throw failure;
                }
            }));

            Assert.AreSame(failure, error);
        }

        [Test]
        public void TraceLevelFilterIsNotEvaluatedBeforeConverterCleanup()
        {
            ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
            ErrorUnwindTraceWriter trace = new ErrorUnwindTraceWriter(converter);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject("{\"Value\":1}", new ErrorUnwindNode(), new JsonSerializerSettings
            {
                Converters = { converter },
                TraceWriter = trace
            }));

            Assert.IsTrue(converter.CleanupCompleted);
            Assert.IsFalse(trace.ReadBeforeCleanup);
            Assert.AreEqual(1, trace.Errors);
        }

        [Test]
        public void TraceWriterExceptionPropagatesAfterConverterCleanup()
        {
            ErrorUnwindValueConverter converter = new ErrorUnwindValueConverter();
            ErrorObserverException failure = new ErrorObserverException();
            ErrorUnwindTraceWriter trace = new ErrorUnwindTraceWriter(converter) { Failure = failure };
            ErrorObserverException error = ExceptionAssert.Throws<ErrorObserverException>(() => JsonConvert.PopulateObject("{\"Value\":1}", new ErrorUnwindNode(), new JsonSerializerSettings
            {
                Converters = { converter },
                TraceWriter = trace
            }));

            Assert.AreSame(failure, error);
            Assert.IsTrue(converter.CleanupCompleted);
            Assert.IsFalse(trace.ReadBeforeCleanup);
            Assert.AreEqual(1, trace.Errors);
        }

        [Test]
        public void ParentCanHandleRequiredPropertyError()
        {
            RequiredErrorUnwindNode failedChild = null;
            RequiredErrorUnwindNode root = JsonConvert.DeserializeObject<RequiredErrorUnwindNode>("{\"Child\":{},\"Name\":\"ok\"}", new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    if (failedChild == null)
                    {
                        failedChild = (RequiredErrorUnwindNode)args.CurrentObject;
                    }
                    else
                    {
                        args.ErrorContext.Handled = true;
                    }
                }
            });

            Assert.AreEqual("ok", root.Name);
            Assert.IsNull(root.Child);
            Assert.IsTrue(root.Deserialized);
            Assert.IsNotNull(failedChild);
            Assert.IsFalse(failedChild.Deserialized);
        }

#if !(NET20 || NET35 || PORTABLE40)
        [Test]
        public void DynamicParentCanHandlePendingError()
        {
            ErrorUnwindDynamic root = JsonConvert.DeserializeObject<ErrorUnwindDynamic>("{\"Child\":{\"Child\":1},\"Value\":2}", new JsonSerializerSettings
            {
                Error = (sender, args) => args.ErrorContext.Handled = args.CurrentObject is ErrorUnwindDynamic
            });

            Assert.IsNull(root.Child);
            Assert.AreEqual(2, root.Value);
        }
#endif

#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD2_0 || NET6_0_OR_GREATER
        [Test]
        public void SerializableConstructorCanCatchPendingError()
        {
            ErrorUnwindSerializable value = JsonConvert.DeserializeObject<ErrorUnwindSerializable>("{\"Child\":{\"Child\":1},\"Valid\":{\"Value\":3}}");

            Assert.IsNotNull(value.NestedError);
            Assert.IsTrue(value.NestedError.Message.StartsWith("Error converting value 1", StringComparison.Ordinal));
            Assert.AreEqual(3, value.Value);
        }
#endif

        public class ErrorUnwindNode
        {
            private ErrorUnwindNode _child;

            public ErrorUnwindNode Child
            {
                get => _child;
                set
                {
                    ChildSetCount++;
                    _child = value;
                }
            }

            public int Value { get; set; }
            [JsonIgnore]
            public int ChildSetCount { get; set; }
            [JsonIgnore]
            public bool Deserialized { get; set; }
            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                Deserialized = true;
            }
        }

        public class ErrorUnwindCallbackNode : ErrorUnwindNode
        {
            [JsonIgnore]
            public Action<ErrorContext> ErrorCallback { get; set; }

            [OnError]
            private void OnError(StreamingContext context, ErrorContext error)
            {
                ErrorCallback?.Invoke(error);
            }
        }

        public class ErrorUnwindContainer
        {
            public ErrorUnwindNode Child { get; set; }
        }

        public class ParentOnlyErrorNode<T>
        {
            public T Child { get; set; }
            public int After { get; set; }
            [JsonIgnore]
            public ErrorContext Error { get; private set; }

            [OnError]
            private void OnError(StreamingContext context, ErrorContext error)
            {
                Error = error;
                error.Handled = !ReferenceEquals(error.OriginalObject, this) && error.OriginalObject is T;
            }
        }

        public class ErrorUnwindGetter
        {
            public int Value => throw new ErrorObserverException();
        }

        public class ErrorUnwindDictionary : Dictionary<string, ErrorUnwindDictionary>
        {
        }

        public class RequiredErrorUnwindNode
        {
            public RequiredErrorUnwindNode Child { get; set; }
            [JsonProperty(Required = Required.Always)]
            public string Name { get; set; }
            [JsonIgnore]
            public bool Deserialized { get; set; }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                Deserialized = true;
            }
        }

#if !(NET20 || NET35 || PORTABLE40)
        public class ErrorUnwindDynamic : DynamicObject
        {
            public ErrorUnwindNode Child { get; set; }
            public int Value { get; set; }
        }

        public class ErrorUnwindWriterDynamic : DynamicObject
        {
            [JsonProperty]
            public object StaticValue { get; set; }

            [JsonIgnore]
            public object DynamicValue { get; set; }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return new[] { "Member" };
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = DynamicValue;
                return binder.Name == "Member";
            }
        }
#endif

#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD2_0 || NET6_0_OR_GREATER
        [Serializable]
        public class ErrorUnwindSerializable : ISerializable
        {
            public JsonException NestedError { get; private set; }
            public int Value { get; private set; }

            private ErrorUnwindSerializable(SerializationInfo info, StreamingContext context)
            {
                try
                {
                    info.GetValue("Child", typeof(ErrorUnwindNode));
                    throw new InvalidOperationException("Expected a deserialization error.");
                }
                catch (JsonException ex)
                {
                    NestedError = ex;
                    Value = ((ErrorUnwindNode)info.GetValue("Valid", typeof(ErrorUnwindNode))).Value;
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotSupportedException();
            }
        }
#endif

        public class FailedObjectCreator
        {
            [JsonConstructor]
            public FailedObjectCreator(ErrorUnwindNode child)
            {
                throw new InvalidOperationException("Constructor must not run after an unhandled error.");
            }
        }

        public class FailedCollectionCreator : List<ErrorUnwindNode>
        {
            public FailedCollectionCreator(IEnumerable<ErrorUnwindNode> values)
            {
                throw new InvalidOperationException("Constructor must not run after an unhandled error.");
            }
        }

        public class FailedDictionaryCreator : Dictionary<string, ErrorUnwindNode>
        {
            public FailedDictionaryCreator(IDictionary<string, ErrorUnwindNode> values)
            {
                throw new InvalidOperationException("Constructor must not run after an unhandled error.");
            }
        }

        public class ErrorUnwindExtensionData
        {
            [JsonExtensionData]
            public IDictionary<string, object> Values { get; } = new Dictionary<string, object>();
        }

        public class ErrorUnwindConverted
        {
        }

        public class ErrorUnwindConverter : JsonConverter
        {
            public bool Populate { get; set; }
            public bool WrapError { get; set; } = true;
            public bool CleanupCompleted { get; set; }
            public bool ReusedSerializer { get; set; }
            public Exception NestedError { get; set; }
            public JsonSerializer Serializer { get; set; }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ErrorUnwindConverted);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                Serializer = serializer;
                int initialDepth = reader.Depth;
                try
                {
                    if (Populate)
                    {
                        serializer.Populate(reader, new ErrorUnwindNode());
                    }
                    else
                    {
                        serializer.Deserialize<ErrorUnwindNode>(reader);
                    }
                    throw new InvalidOperationException("Expected a deserialization error.");
                }
                catch (JsonException ex)
                {
                    NestedError = ex;
                    using (JsonReader validReader = new JsonTextReader(new StringReader("{\"Value\":3}")))
                    {
                        ReusedSerializer = serializer.Deserialize<ErrorUnwindNode>(validReader).Value == 3;
                    }
                    if (WrapError)
                    {
                        throw new JsonSerializationException("Wrapped error.", ex);
                    }
                    while (reader.Depth > initialDepth)
                    {
                        reader.ReadAndAssert();
                    }
                    return new ErrorUnwindConverted();
                }
                finally
                {
                    CleanupCompleted = true;
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        public class ErrorObserverException : Exception
        {
        }

        public class ErrorUnwindValueConverter : JsonConverter
        {
            public bool FailureStarted { get; private set; }
            public bool CleanupCompleted { get; private set; }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(int);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return Fail();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Fail();
            }

            private object Fail()
            {
                FailureStarted = true;
                try
                {
                    throw new JsonSerializationException("Converter error.");
                }
                finally
                {
                    CleanupCompleted = true;
                }
            }

        }

        public class ErrorUnwindTraceWriter : ITraceWriter
        {
            private readonly ErrorUnwindValueConverter _converter;

            public ErrorUnwindTraceWriter(ErrorUnwindValueConverter converter)
            {
                _converter = converter;
            }

            public bool ReadBeforeCleanup { get; private set; }
            public int Errors { get; private set; }
            public Exception Failure { get; set; }

            public TraceLevel LevelFilter
            {
                get
                {
                    if (_converter.FailureStarted && !_converter.CleanupCompleted)
                    {
                        ReadBeforeCleanup = true;
                        throw new ErrorObserverException();
                    }
                    return TraceLevel.Error;
                }
            }

            public void Trace(TraceLevel level, string message, Exception ex)
            {
                Errors++;
                if (Failure != null)
                {
                    throw Failure;
                }
            }
        }

        [Test]
        public void ErrorHandlingMetadata()
        {
            List<Exception> errors = new List<Exception>();

            AAA a2 = JsonConvert.DeserializeObject<AAA>(@"{""MyTest"":{""$type"":""<Namespace>.JsonTest+MyTest2, <Assembly>""}}", new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Error = (object sender, Json.Serialization.ErrorEventArgs e) =>
                {
                    errors.Add(e.ErrorContext.Error);
                    e.ErrorContext.Handled = true;
                }
            });

            Assert.IsNotNull(a2);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Error resolving type specified in JSON '<Namespace>.JsonTest+MyTest2, <Assembly>'. Path 'MyTest.$type', line 1, position 61.", errors[0].Message);
        }

        [Test]
        public void ErrorHandlingMetadata_TopLevel()
        {
            List<Exception> errors = new List<Exception>();

            JObject a2 = (JObject)JsonConvert.DeserializeObject(@"{""$type"":""<Namespace>.JsonTest+MyTest2, <Assembly>""}", new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Error = (object sender, Json.Serialization.ErrorEventArgs e) =>
                {
                    errors.Add(e.ErrorContext.Error);
                    e.ErrorContext.Handled = true;
                }
            });

            Assert.IsNull(a2);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Error resolving type specified in JSON '<Namespace>.JsonTest+MyTest2, <Assembly>'. Path '$type', line 1, position 51.", errors[0].Message);
        }

        public class AAA
        {
            public ITest MyTest { get; set; }
        }

        public interface ITest { }
        public class MyTest : ITest { }

        public class MyClass1
        {
            [JsonProperty("myint")]
            public int MyInt { get; set; }
            [JsonProperty("Mybool")]
            public bool Mybool { get; set; }
        }

        [Test]
        public void ErrorDeserializingIntegerInObject()
        {
            var errors = new List<string>();
            var json = "{\"myint\":3554860000,\"Mybool\":false}";
            var i = JsonConvert.DeserializeObject<MyClass1>(json, new JsonSerializerSettings
            {
                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("JSON integer 3554860000 is too large or small for an Int32. Path 'myint', line 1, position 19.", errors[0]);
        }

        [Test]
        public void ErrorDeserializingListHandled()
        {
            string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

            var possibleMsgs = new[]
            {
                "[1] - Error message for member 1 = An item with the same key has already been added.",
                "[1] - Error message for member 1 = An element with the same key already exists in the dictionary.", // mono
                "[1] - Error message for member 1 = An item with the same key has already been added. Key: Jim" // netcore
            };
            VersionKeyedCollection c = JsonConvert.DeserializeObject<VersionKeyedCollection>(json);
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual(1, c.Messages.Count);

            Console.WriteLine(c.Messages[0]);
            Assert.IsTrue(possibleMsgs.Any(m => m == c.Messages[0]), "Expected One of: " + Environment.NewLine + string.Join(Environment.NewLine, possibleMsgs) + Environment.NewLine + "Was: " + Environment.NewLine + c.Messages[0]);
        }

        [Test]
        public void DeserializingErrorInChildObject()
        {
            ListErrorObjectCollection c = JsonConvert.DeserializeObject<ListErrorObjectCollection>(@"[
  {
    ""Member"": ""Value1"",
    ""Member2"": null
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""ThrowError"": ""Value"",
    ""Object"": {
      ""Array"": [
        1,
        2
      ]
    }
  },
  {
    ""ThrowError"": ""Handle this!"",
    ""Member"": ""Value3""
  }
]");

            Assert.AreEqual(3, c.Count);
            Assert.AreEqual("Value1", c[0].Member);
            Assert.AreEqual("Value2", c[1].Member);
            Assert.AreEqual("Value3", c[2].Member);
            Assert.AreEqual("Handle this!", c[2].ThrowError);
        }

        [Test]
        public void SerializingErrorIn3DArray()
        {
            ListErrorObject[,,] c = new ListErrorObject[,,]
            {
                {
                    {
                        new ListErrorObject
                        {
                            Member = "Value1",
                            ThrowError = "Handle this!",
                            Member2 = "Member1"
                        },
                        new ListErrorObject
                        {
                            Member = "Value2",
                            Member2 = "Member2"
                        },
                        new ListErrorObject
                        {
                            Member = "Value3",
                            ThrowError = "Handle that!",
                            Member2 = "Member3"
                        }
                    },
                    {
                        new ListErrorObject
                        {
                            Member = "Value1",
                            ThrowError = "Handle this!",
                            Member2 = "Member1"
                        },
                        new ListErrorObject
                        {
                            Member = "Value2",
                            Member2 = "Member2"
                        },
                        new ListErrorObject
                        {
                            Member = "Value3",
                            ThrowError = "Handle that!",
                            Member2 = "Member3"
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Error = (s, e) =>
                {
                    if (e.CurrentObject.GetType().IsArray)
                    {
                        e.ErrorContext.Handled = true;
                    }
                }
            });

            StringAssert.AreEqual(@"[
  [
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ],
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ]
  ]
]", json);
        }

        [Test]
        public void SerializingErrorInChildObject()
        {
            ListErrorObjectCollection c = new ListErrorObjectCollection
            {
                new ListErrorObject
                {
                    Member = "Value1",
                    ThrowError = "Handle this!",
                    Member2 = "Member1"
                },
                new ListErrorObject
                {
                    Member = "Value2",
                    Member2 = "Member2"
                },
                new ListErrorObject
                {
                    Member = "Value3",
                    ThrowError = "Handle that!",
                    Member2 = "Member3"
                }
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"[
  {
    ""Member"": ""Value1"",
    ""ThrowError"": ""Handle this!"",
    ""Member2"": ""Member1""
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""Member"": ""Value3"",
    ""ThrowError"": ""Handle that!"",
    ""Member2"": ""Member3""
  }
]", json);
        }

        [Test]
        public void DeserializingErrorInDateTimeCollection()
        {
            DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(@"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]", new IsoDateTimeConverter());

            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
            Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);
        }

        [Test]
        public void DeserializingErrorHandlingUsingEvent()
        {
            List<string> errors = new List<string>();

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Error = delegate(object sender, ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
                Converters = { new IsoDateTimeConverter() }
            });
            var c = serializer.Deserialize<List<DateTime>>(new JsonTextReader(new StringReader(@"[
        ""2009-09-09T00:00:00Z"",
        ""I am not a date and will error!"",
        [
          1
        ],
        ""1977-02-20T00:00:00Z"",
        null,
        ""2000-12-01T00:00:00Z""
      ]")));

            // 2009-09-09T00:00:00Z
            // 1977-02-20T00:00:00Z
            // 2000-12-01T00:00:00Z

            // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
            // Unexpected token parsing date. Expected String, got StartArray.
            // Cannot convert null value to System.DateTime.

            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
            Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

            Assert.AreEqual(3, errors.Count);
            var possibleErrs = new[]
            {
#if !(NET20 || NET35)
                "[1] - 1 - The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.",
                "[1] - 1 - String was not recognized as a valid DateTime.",
                "[1] - 1 - The string 'I am not a date and will error!' was not recognized as a valid DateTime. There is an unknown word starting at index '0'."
#else
    // handle typo fix in later versions of .NET
                "[1] - 1 - The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.",
                "[1] - 1 - The string was not recognized as a valid DateTime. There is a unknown word starting at index 0."
#endif
            };

            Assert.IsTrue(possibleErrs.Any(m => m == errors[0]),
                "Expected One of: " + string.Join(Environment.NewLine, possibleErrs) + Environment.NewLine + "But was: " + errors[0]);

            Assert.AreEqual("[2] - 2 - Unexpected token parsing date. Expected String, got StartArray. Path '[2]', line 4, position 9.", errors[1]);
            Assert.AreEqual("[4] - 4 - Cannot convert null value to System.DateTime. Path '[4]', line 8, position 12.", errors[2]);
        }

        [Test]
        public void DeserializingErrorInDateTimeCollectionWithAttributeWithEventNotCalled()
        {
            bool eventErrorHandlerCalled = false;

            DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(
                @"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]",
                new JsonSerializerSettings
                {
                    Error = (s, a) => eventErrorHandlerCalled = true,
                    Converters =
                    {
                        new IsoDateTimeConverter()
                    }
                });

            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
            Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

            Assert.AreEqual(false, eventErrorHandlerCalled);
        }

        [Test]
        public void SerializePerson()
        {
            PersonError person = new PersonError
            {
                Name = "George Michael Bluth",
                Age = 16,
                Roles = null,
                Title = "Mister Manager"
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""George Michael Bluth"",
  ""Age"": 16,
  ""Title"": ""Mister Manager""
}", json);
        }

        [Test]
        public void DeserializeNestedUnhandled()
        {
            List<string> errors = new List<string>();

            string json = @"[[""kjhkjhkjhkjh""]]";

            Exception e = null;
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Error += delegate(object sender, ErrorEventArgs args)
                {
                    // only log an error once
                    if (args.CurrentObject == args.ErrorContext.OriginalObject)
                    {
                        errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                    }
                };

                serializer.Deserialize(new StringReader(json), typeof(List<List<DateTime>>));
            }
            catch (Exception ex)
            {
                e = ex;
            }

            Assert.AreEqual(@"Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", e.Message);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(@"[0][0] - 0 - Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", errors[0]);
        }

        [Test]
        public void MultipleRequiredPropertyErrors()
        {
            string json = "{}";
            List<string> errors = new List<string>();
            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
            serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(MyTypeWithRequiredMembers));

            Assert.AreEqual(2, errors.Count);
            Assert.IsTrue(errors[0].StartsWith(" - Required1 - Required property 'Required1' not found in JSON. Path '', line 1, position 2."));
            Assert.IsTrue(errors[1].StartsWith(" - Required2 - Required property 'Required2' not found in JSON. Path '', line 1, position 2."));
        }

        [Test]
        public void HandlingArrayErrors()
        {
            string json = "[\"a\",\"b\",\"45\",34]";

            List<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };

            serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(int[]));

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual("[0] - 0 - Could not convert string to integer: a. Path '[0]', line 1, position 4.", errors[0]);
            Assert.AreEqual("[1] - 1 - Could not convert string to integer: b. Path '[1]', line 1, position 8.", errors[1]);
        }

        [Test]
        public void HandlingMultidimensionalArrayErrors()
        {
            string json = "[[\"a\",\"45\"],[\"b\",34]]";

            List<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };

            serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(int[,]));

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual("[0][0] - 0 - Could not convert string to integer: a. Path '[0][0]', line 1, position 5.", errors[0]);
            Assert.AreEqual("[1][0] - 0 - Could not convert string to integer: b. Path '[1][0]', line 1, position 16.", errors[1]);
        }

        [Test]
        public void ErrorHandlingAndAvoidingRecursiveDepthError()
        {
            string json = "{'A':{'A':{'A':{'A':{'A':{}}}}}}";
            JsonSerializer serializer = new JsonSerializer() { };
            IList<string> errors = new List<string>();
            serializer.Error += (sender, e) =>
            {
                e.ErrorContext.Handled = true;
                errors.Add(e.ErrorContext.Path);
            };

            serializer.Deserialize<Nest>(new JsonTextReader(new StringReader(json)) { MaxDepth = 3 });

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("A.A.A", errors[0]);
        }

        public class Nest
        {
            public Nest A { get; set; }
        }

        [Test]
        public void InfiniteErrorHandlingLoopFromInputError()
        {
            IList<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += (sender, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            ErrorPerson[] result = serializer.Deserialize<ErrorPerson[]>(new JsonTextReader(new ThrowingReader()));

            Assert.IsNull(result);
            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual("too far", errors[0]);
            Assert.AreEqual("too far", errors[1]);
            Assert.AreEqual("Infinite loop detected from error handling. Path '[1023]', line 1, position 65536.", errors[2]);
        }

        [Test]
        public void ArrayHandling()
        {
            IList<string> errors = new List<string>();

            object o = JsonConvert.DeserializeObject(
                "[0,x]",
                typeof(int[]),
                new JsonSerializerSettings
                {
                    Error = (sender, arg) =>
                    {
                        errors.Add(arg.ErrorContext.Error.Message);
                        arg.ErrorContext.Handled = true;
                    }
                });

            Assert.IsNotNull(o);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Unexpected character encountered while parsing value: x. Path '[0]', line 1, position 4.", errors[0]);

            Assert.AreEqual(1, ((int[])o).Length);
            Assert.AreEqual(0, ((int[])o)[0]);
        }

        [Test]
        public void ArrayHandling_JTokenReader()
        {
            IList<string> errors = new List<string>();

            JTokenReader reader = new JTokenReader(new JArray(0, true));

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Error = (sender, arg) =>
                {
                    errors.Add(arg.ErrorContext.Error.Message);
                    arg.ErrorContext.Handled = true;
                }
            });
            object o = serializer.Deserialize(reader, typeof(int[]));

            Assert.IsNotNull(o);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Error reading integer. Unexpected token: Boolean. Path '[1]'.", errors[0]);

            Assert.AreEqual(1, ((int[])o).Length);
            Assert.AreEqual(0, ((int[])o)[0]);
        }

        [Test]
        public void ArrayHandlingInObject()
        {
            IList<string> errors = new List<string>();

            Dictionary<string, int[]> o = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(
                "{'badarray':[0,x,2],'goodarray':[0,1,2]}",
                new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Default,
                    Error = (sender, arg) =>
                    {
                        errors.Add(arg.ErrorContext.Error.Message);
                        arg.ErrorContext.Handled = true;
                    }
                });

            Assert.IsNotNull(o);

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual("Unexpected character encountered while parsing value: x. Path 'badarray[0]', line 1, position 16.", errors[0]);
            Assert.AreEqual("Unexpected character encountered while parsing value: ,. Path 'badarray[1]', line 1, position 17.", errors[1]);

            Assert.AreEqual(2, o.Count);
            Assert.AreEqual(2, o["badarray"].Length);
            Assert.AreEqual(0, o["badarray"][0]);
            Assert.AreEqual(2, o["badarray"][1]);
        }

        [Test]
        public void ErrorHandlingEndOfContent()
        {
            IList<string> errors = new List<string>();

            const string input = "{\"events\":[{\"code\":64411},{\"code\":64411,\"prio";

            const int maxDepth = 256;
            using (var jsonTextReader = new JsonTextReader(new StringReader(input)) { MaxDepth = maxDepth })
            {
                JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    MaxDepth = maxDepth,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });
                jsonSerializer.Error += (sender, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                };

                LogMessage logMessage = jsonSerializer.Deserialize<LogMessage>(jsonTextReader);

                Assert.IsNotNull(logMessage.Events);
                Assert.AreEqual(1, logMessage.Events.Count);
                Assert.AreEqual("64411", logMessage.Events[0].Code);
            }

            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual(@"Unterminated string. Expected delimiter: "". Path 'events[1].code', line 1, position 45.", errors[0]);
            Assert.AreEqual(@"Unexpected end when deserializing array. Path 'events[1].code', line 1, position 45.", errors[1]);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events[1].code', line 1, position 45.", errors[2]);
        }

        [Test]
        public void ErrorHandlingEndOfContentDictionary()
        {
            IList<string> errors = new List<string>();

            const string input = "{\"events\":{\"code\":64411},\"events2\":{\"code\":64412,";

            const int maxDepth = 256;
            using (var jsonTextReader = new JsonTextReader(new StringReader(input)) { MaxDepth = maxDepth })
            {
                JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { MaxDepth = maxDepth, MetadataPropertyHandling = MetadataPropertyHandling.Default });
                jsonSerializer.Error += (sender, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                };

                IDictionary<string, LogEvent> logEvents = jsonSerializer.Deserialize<IDictionary<string, LogEvent>>(jsonTextReader);

                Assert.IsNotNull(logEvents);
                Assert.AreEqual(2, logEvents.Count);
                Assert.AreEqual("64411", logEvents["events"].Code);
                Assert.AreEqual("64412", logEvents["events2"].Code);
            }

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[0]);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[1]);
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void ErrorHandlingEndOfContentDynamic()
        {
            IList<string> errors = new List<string>();

            string json = @"{
  ""Explicit"": true,
  ""Decimal"": 99.9,
  ""Int"": 1,
  ""ChildObject"": {
    ""Integer"": 123";

            TestDynamicObject newDynamicObject = JsonConvert.DeserializeObject<TestDynamicObject>(json, new JsonSerializerSettings
            {
                Error = (sender, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                },
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            });
            Assert.AreEqual(true, newDynamicObject.Explicit);

            dynamic d = newDynamicObject;

            Assert.AreEqual(99.9, d.Decimal);
            Assert.AreEqual(1, d.Int);
            Assert.AreEqual(123, d.ChildObject.Integer);

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 18.", errors[0]);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 18.", errors[1]);
        }
#endif

        [Test]
        public void WriteEndOnPropertyState()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error += (obj, args) => { args.ErrorContext.Handled = true; };

            var data = new List<ErrorPerson2>()
            {
                new ErrorPerson2 { FirstName = "Scott", LastName = "Hanselman" },
                new ErrorPerson2 { FirstName = "Scott", LastName = "Hunter" },
                new ErrorPerson2 { FirstName = "Scott", LastName = "Guthrie" },
            };

            Dictionary<string, IEnumerable<IErrorPerson2>> dictionary = data.GroupBy(person => person.FirstName).ToDictionary(group => @group.Key, group => @group.Cast<IErrorPerson2>());
            string output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);
            Assert.AreEqual(@"{""Scott"":[]}", output);
        }

        [Test]
        public void WriteEndOnPropertyState2()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error += (obj, args) => { args.ErrorContext.Handled = true; };

            var data = new List<ErrorPerson2>
            {
                new ErrorPerson2 { FirstName = "Scott", LastName = "Hanselman" },
                new ErrorPerson2 { FirstName = "Scott", LastName = "Hunter" },
                new ErrorPerson2 { FirstName = "Scott", LastName = "Guthrie" },
                new ErrorPerson2 { FirstName = "James", LastName = "Newton-King" },
            };

            Dictionary<string, IEnumerable<IErrorPerson2>> dictionary = data.GroupBy(person => person.FirstName).ToDictionary(group => @group.Key, group => @group.Cast<IErrorPerson2>());
            string output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);

            Assert.AreEqual(@"{""Scott"":[],""James"":[]}", output);
        }

        [Test]
        public void NoObjectWithEvent()
        {
            string json = "{\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            JsonTextReader jReader = new JsonTextReader(new StreamReader(stream));
            JsonSerializer s = new JsonSerializer();
            s.Error += (sender, args) => { args.ErrorContext.Handled = true; };
            ErrorPerson2 obj = s.Deserialize<ErrorPerson2>(jReader);

            Assert.IsNull(obj);
        }

        [Test]
        public void NoObjectWithAttribute()
        {
            string json = "{\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            JsonTextReader jReader = new JsonTextReader(new StreamReader(stream));
            JsonSerializer s = new JsonSerializer();

            ExceptionAssert.Throws<JsonReaderException>(() => { ErrorTestObject obj = s.Deserialize<ErrorTestObject>(jReader); }, @"Unterminated string. Expected delimiter: "". Path '', line 1, position 3.");
        }

        public class RootThing
        {
            public Something Something { get; set; }
        }

        public class RootSomethingElse
        {
            public SomethingElse SomethingElse { get; set; }
        }

        /// <summary>
        /// This could be an object we are passing up in an interface.
        /// </summary>
        [JsonConverter(typeof(SomethingConverter))]
        public class Something
        {
            public class SomethingConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return true;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    try
                    {
                        // Do own stuff.
                        // Then call deserialise for inner object.
                        var innerObject = serializer.Deserialize(reader, typeof(SomethingElse));

                        return null;
                    }
                    catch (Exception ex)
                    {
                        // If we get an error wrap it in something less scary.
                        throw new Exception("An error occurred.", ex);
                    }
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    try
                    {
                        Something s = (Something)value;

                        // Do own stuff.
                        // Then call serialise for inner object.
                        serializer.Serialize(writer, s.RootSomethingElse);
                    }
                    catch (Exception ex)
                    {
                        // If we get an error wrap it in something less scary.
                        throw new Exception("An error occurred.", ex);
                    }
                }
            }

            public RootSomethingElse RootSomethingElse { get; set; }

            public Something()
            {
                this.RootSomethingElse = new RootSomethingElse();
            }
        }

        /// <summary>
        /// This is an object that is contained in the interface object.
        /// </summary>
        [JsonConverter(typeof(SomethingElseConverter))]
        public class SomethingElse
        {
            public class SomethingElseConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return true;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Test]
        public void DeserializeWrappingErrorsAndErrorHandling()
        {
            var serialiser = JsonSerializer.Create(new JsonSerializerSettings() { });

            string foo = "{ something: { rootSomethingElse { somethingElse: 0 } } }";
            var reader = new System.IO.StringReader(foo);

            ExceptionAssert.Throws<Exception>(() => { serialiser.Deserialize(reader, typeof(Something)); }, "An error occurred.");
        }

        [Test]
        public void SerializeWrappingErrorsAndErrorHandling()
        {
            var serialiser = JsonSerializer.Create(new JsonSerializerSettings() { });

            Something s = new Something
            {
                RootSomethingElse = new RootSomethingElse
                {
                    SomethingElse = new SomethingElse()
                }
            };
            RootThing r = new RootThing
            {
                Something = s
            };

            var writer = new System.IO.StringWriter();

            ExceptionAssert.Throws<Exception>(() => { serialiser.Serialize(writer, r); }, "An error occurred.");
        }

        [Test]
        public void DeserializeRootConverter()
        {
            SomethingElse result = JsonConvert.DeserializeObject<SomethingElse>("{}", new JsonSerializerSettings
            {
                Error = (o, e) => { e.ErrorContext.Handled = true; }
            });

            Assert.IsNull(result);
        }

        [Test]
        public void SerializeRootConverter()
        {
            string result = JsonConvert.SerializeObject(new SomethingElse(), new JsonSerializerSettings
            {
                Error = (o, e) => { e.ErrorContext.Handled = true; }
            });

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void IntegerToLarge_ReadNextValue()
        {
            IList<string> errorMessages = new List<string>();

            JsonReader reader = new JsonTextReader(new StringReader(@"{
  ""string1"": ""blah"",
  ""int1"": 2147483648,
  ""string2"": ""also blah"",
  ""int2"": 2147483648,
  ""string3"": ""more blah"",
  ""dateTime1"": ""200NOTDATE"",
  ""string4"": ""even more blah""
}"));
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error = (sender, args) =>
            {
                errorMessages.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
            JsonSerializer serializer = JsonSerializer.Create(settings);

            DataModel data = new DataModel();
            serializer.Populate(reader, data);

            Assert.AreEqual("blah", data.String1);
            Assert.AreEqual(0, data.Int1);
            Assert.AreEqual("also blah", data.String2);
            Assert.AreEqual(0, data.Int2);
            Assert.AreEqual("more blah", data.String3);
            Assert.AreEqual(default(DateTime), data.DateTime1);
            Assert.AreEqual("even more blah", data.String4);

            //Assert.AreEqual(2, errorMessages.Count);
            Assert.AreEqual("JSON integer 2147483648 is too large or small for an Int32. Path 'int1', line 3, position 20.", errorMessages[0]);
            Assert.AreEqual("JSON integer 2147483648 is too large or small for an Int32. Path 'int2', line 5, position 20.", errorMessages[1]);
            Assert.AreEqual("Could not convert string to DateTime: 200NOTDATE. Path 'dateTime1', line 7, position 27.", errorMessages[2]);
        }

        [Test]
        public void HandleErrorInDictionaryObject()
        {
            var json = @"{
                model1: { String1: 's1', Int1: 'x' },
                model2: { String1: 's2', Int1: 2 }
            }";
            var dictionary = JsonConvert.DeserializeObject<TolerantDictionary<string, DataModel>>(json);

            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("model2"));
            Assert.AreEqual("s2", dictionary["model2"].String1);
            Assert.AreEqual(2, dictionary["model2"].Int1);
        }

        private class DataModel
        {
            public string String1 { get; set; }
            public int Int1 { get; set; }
            public string String2 { get; set; }
            public int Int2 { get; set; }
            public string String3 { get; set; }
            public DateTime DateTime1 { get; set; }
            public string String4 { get; set; }
        }
    }

    internal interface IErrorPerson2
    {
    }

    internal class ErrorPerson2 //:IPerson - oops! Forgot to implement the person interface
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    public class ThrowingReader : TextReader
    {
        private int _position = 0;
        private static string element = "{\"FirstName\":\"Din\",\"LastName\":\"Rav\",\"Item\":{\"ItemName\":\"temp\"}}";
        private bool _firstRead = true;
        private bool _readComma = false;

        public ThrowingReader()
        {
        }

        public override int Read(char[] buffer, int index, int count)
        {
            char[] temp = new char[buffer.Length];
            int charsRead = 0;
            if (_firstRead)
            {
                charsRead = new StringReader("[").Read(temp, index, count);
                _firstRead = false;
            }
            else
            {
                if (_readComma)
                {
                    charsRead = new StringReader(",").Read(temp, index, count);
                    _readComma = false;
                }
                else
                {
                    charsRead = new StringReader(element).Read(temp, index, count);
                    _readComma = true;
                }
            }

            _position += charsRead;
            if (_position > 65536)
            {
                throw new Exception("too far");
            }
            Array.Copy(temp, index, buffer, index, charsRead);
            return charsRead;
        }
    }

    public class ErrorPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ErrorItem Item { get; set; }
    }

    public class ErrorItem
    {
        public string ItemName { get; set; }
    }

    [JsonObject]
    public class MyTypeWithRequiredMembers
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string Required1;

        [JsonProperty(Required = Required.AllowNull)]
        public string Required2;
    }

    public class LogMessage
    {
        public string DeviceId { get; set; }
        public IList<LogEvent> Events { get; set; }
    }

    public class LogEvent
    {
        public string Code { get; set; }
        public int Priority { get; set; }
    }

    public class ErrorTestObject
    {
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
        }
    }
    
    /// <summary>
    /// A dictionary that ignores deserialization errors and excludes bad items
    /// </summary>
    public class TolerantDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        [OnError]
        public void OnDeserializationError(StreamingContext streamingContext, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
