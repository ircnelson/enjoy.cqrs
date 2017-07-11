using EnjoyCQRS.Projections;
using EnjoyCQRS.Projections.InMemory;
using FluentAssertions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EnjoyCQRS.UnitTests.Projections
{
    [Trait("Unit", "Projection")]
    public class MemoryProjectionStoreTests : ProjectionStoreTests
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> _storeDictionary;

        public MemoryProjectionStoreTests()
        {
            _storeDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();
            Store = new MemoryProjectionStore(new StubProjectionStrategy(), _storeDictionary);
        }

        [Fact]
        public async Task Reset_all_bucket()
        {
            var bucket1 = "test-bucket1";
            var bucket2 = "test-bucket2";

            var records = new List<DocumentRecord>
                                      {
                                          new DocumentRecord("first", () => Encoding.UTF8.GetBytes("test message 1")),
                                      };

            await Store.ApplyAsync(bucket1, records);
            await Store.ApplyAsync(bucket2, records);

            ((MemoryProjectionStore)Store).ResetAll();

            var result1 = (await Store.EnumerateContentsAsync(bucket1)).ToList();
            var result2 = (await Store.EnumerateContentsAsync(bucket2)).ToList();

            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }
    }
}
