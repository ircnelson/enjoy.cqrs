﻿using System.Linq;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.MetadataProviders;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Xunit;
using EnjoyCQRS.Collections;

namespace EnjoyCQRS.UnitTests.Metadata
{
    public class MetadataProviderTests
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Metadata";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Event_MetadataProvider()
        {
            var stubAggregate = StubAggregate.Create("Test");

            var metadataProvider = new EventTypeMetadataProvider();

            var metadata = stubAggregate.UncommittedEvents.SelectMany(e => metadataProvider.Provide(stubAggregate, e.Data, MetadataCollection.Empty));

            metadata.Count().Should().Be(2);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Aggregate_MetadataProvider()
        {
            var stubAggregate = StubAggregate.Create("Test");

            var metadataProvider = new AggregateTypeMetadataProvider();

            var metadata = stubAggregate.UncommittedEvents.SelectMany(e => metadataProvider.Provide(stubAggregate, e.Data, MetadataCollection.Empty));

            metadata.Count().Should().Be(3);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void CorrelationId_MetadataProvider()
        {
            var stubAggregate = StubAggregate.Create("Test");
            stubAggregate.ChangeName("Test 1");
            stubAggregate.ChangeName("Test 2");

            var metadataProvider = new CorrelationIdMetadataProvider();

            var metadatas = stubAggregate.UncommittedEvents.SelectMany(e => metadataProvider.Provide(stubAggregate, e.Data, MetadataCollection.Empty));

            metadatas.Select(e => e.Value).Distinct().Count().Should().Be(1);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_take_event_name_based_on_attribute()
        {
            var stubAggregate = StubAggregate.Create("Test");
            var metadataProvider = new EventTypeMetadataProvider();
            var metadatas = stubAggregate.UncommittedEvents.SelectMany(e => metadataProvider.Provide(stubAggregate, e.Data, MetadataCollection.Empty));

            var metadata = new MetadataCollection(metadatas);

            metadata.GetValue(MetadataKeys.EventName).Should().Be("StubCreated");
        }
    }
}