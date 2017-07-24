using EnjoyCQRS.Projections.InMemory;
using EnjoyCQRS.UnitTests.Shared;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace EnjoyCQRS.UnitTests.Projections
{
    [Trait("Unit", "Projection")]
    public class MemoryProjectionReaderWriterTests : ProjectionReaderWriterTests
    {
        public MemoryProjectionReaderWriterTests()
        {
            var documentStrategy = new NewtonsoftJsonProjectionStrategy();
            var concurrentDictionary = new ConcurrentDictionary<string, byte[]>();

            _reader = new MemoryProjectionReaderWriter<Guid, int>(documentStrategy, concurrentDictionary);
            _writer = new MemoryProjectionReaderWriter<Guid, int>(documentStrategy, concurrentDictionary);
            _guidKeyClassReader = new MemoryProjectionReaderWriter<Guid, Test1>(documentStrategy, concurrentDictionary);
            _guidKeyClassWriter = new MemoryProjectionReaderWriter<Guid, Test1>(documentStrategy, concurrentDictionary);
        }
    }
}
