#if HAVE_ASYNC
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.TestObjects
{
    internal class AsyncTestHelper
    {

        public AsyncTestHelper()
        {
            Serializer = new JsonSerializer();
            Serializer.Formatting = Formatting.Indented;
            _gen = new Castle.DynamicProxy.ProxyGenerator();
            ResetStream();
        }

        public JsonSerializer Serializer { get; private set; }

        public Stream Stream { get; private set; }

        public TextReader GetReader()
        {
            var reader = new StreamReader(Stream, Encoding.UTF8, false, 1024, true);
            var result = _gen.CreateClassProxyWithTarget(typeof(TextReader), reader, new AsyncOnlyInterceptor("Read", "Flush"));
            Stream.Seek(0, SeekOrigin.Begin);
            return (TextReader)result;
        }

        public TextWriter GetWriter()
        {
            var writer = new StreamWriter(Stream, Encoding.UTF8, 1024, true);
            var result = _gen.CreateClassProxyWithTarget(typeof(TextWriter), writer, new AsyncOnlyInterceptor("Read", "Write", "Flush"));
            Stream.Seek(0, SeekOrigin.Begin);
            return (TextWriter)result;
        }

        public Task<string> GetStreamContentAsync()
        {
            using (var reader = GetReader())
            {
                return reader.ReadToEndAsync();
            }
        }

        public async Task<string> SerializeAsync(object data)
        {
            var writer = GetWriter();
            await Serializer.SerializeAsync(writer, data);
            await writer.FlushAsync();
            return await GetStreamContentAsync();
        }

        public async Task<T> DeserializeAsync<T>(string data)
        {

            var writer = GetWriter();
            await writer.WriteAsync(data);
            await writer.FlushAsync();
            var reader = GetReader();
            var result = await Serializer.DeserializeAsync(reader, typeof(T));
            return (T)result;
        }

        public void ResetStream()
        {
            var stream = new MemoryStream();
            Stream = (Stream)_gen.CreateClassProxyWithTarget(typeof(Stream), stream, new AsyncOnlyInterceptor("Read", "Write", "Flush"));
        }

        private Castle.DynamicProxy.ProxyGenerator _gen;
    }
}

#endif