using EnjoyCQRS.Projections.InMemory;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace EnjoyCQRS.UnitTests.Projections
{
    [Trait("Unit", "Projection")]
    public class MemoryDocumentReaderWriterTest : DocumentReaderWriterTest
    {
        public MemoryDocumentReaderWriterTest()
        {
            var documentStrategy = new StubDocumentStrategy();
            var concurrentDictionary = new ConcurrentDictionary<string, byte[]>();

            _reader = new MemoryDocumentReaderWriter<Guid, int>(documentStrategy, concurrentDictionary);
            _writer = new MemoryDocumentReaderWriter<Guid, int>(documentStrategy, concurrentDictionary);
            _guidKeyClassReader = new MemoryDocumentReaderWriter<Guid, Test1>(documentStrategy, concurrentDictionary);
            _guidKeyClassWriter = new MemoryDocumentReaderWriter<Guid, Test1>(documentStrategy, concurrentDictionary);
        }
    }
}
