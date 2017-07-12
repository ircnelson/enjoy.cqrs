using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EnjoyCQRS.Projections;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Projections
{
    public abstract class ProjectionStoreTests
    {
        protected IProjectionStore Store { get; set; }

        [Fact]
        public async Task Get_not_created_bucket()
        {
            var bucket = Guid.NewGuid().ToString();

            (await Store.EnumerateContentsAsync(bucket)).Should().BeEmpty();
        }


        [Fact]
        public async Task Write_bucket()
        {
            var bucket = "test-bucket";
            var records = new List<ProjectionRecord>
                                      {
                                          new ProjectionRecord("first", () => Encoding.UTF8.GetBytes("test message 1")),
                                          new ProjectionRecord("second", () => Encoding.UTF8.GetBytes("test message 2")),
                                      };
            await Store.ApplyAsync(bucket, records);

            var actualRecords = (await Store.EnumerateContentsAsync(bucket)).ToList();

            records.Count.Should().Be(actualRecords.Count);

            for (int i = 0; i < records.Count; i++)
            {
                (actualRecords.Count(x => x.Key == records[i].Key) == 1).Should().BeTrue();

                Encoding.UTF8.GetString(records[i].Read()).Should().Be(Encoding.UTF8.GetString(actualRecords.First(x => x.Key == records[i].Key).Read()));
            }
        }

        [Fact]
        public async Task Reset_bucket()
        {
            var bucket1 = "test-bucket1";
            var bucket2 = "test-bucket2";

            var records = new List<ProjectionRecord>
                                      {
                                          new ProjectionRecord("first", () => Encoding.UTF8.GetBytes("test message 1")),
                                      };

            await Store.ApplyAsync(bucket1, records);
            await Store.ApplyAsync(bucket2, records);

            Store.Cleanup(bucket1);

            var result1 = (await Store.EnumerateContentsAsync(bucket1)).ToList();
            var result2 = (await Store.EnumerateContentsAsync(bucket2)).ToList();
            result1.Should().BeEmpty();

            records.Count.Should().Be(result2.Count);
        }
    }
}