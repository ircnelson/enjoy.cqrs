using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.UnitTests.Shared;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests
{
    public class EventSerializerTests
    {
        [Fact]
        public void Try_deserialize_event_with_not_found_ClrType()
        {
            var textSerializer = new JsonTextSerializer();
            var eventSerializer = new EventSerializer(textSerializer);

            var serializedData = textSerializer.Serialize(new SpokeSomething("Hi"));

            var metadata = new EventSource.Metadata(new[]
            {
                new KeyValuePair<string, string>(MetadataKeys.AggregateId, Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>(MetadataKeys.AggregateSequenceNumber, 1.ToString()),
                new KeyValuePair<string, string>(MetadataKeys.EventClrType, "EnjoyCQRS.UnitTests.EventSerializerTests+NotFoundClrType, EnjoyCQRS.UnitTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
            });
            
            var mockCommitedEvent = new Mock<ICommitedEvent>();
            mockCommitedEvent.Setup(e => e.AggregateVersion).Returns(1);
            mockCommitedEvent.Setup(e => e.AggregateId).Returns(Guid.NewGuid);
            mockCommitedEvent.Setup(e => e.SerializedData).Returns(serializedData);
            mockCommitedEvent.Setup(e => e.SerializedMetadata).Returns(textSerializer.Serialize(metadata));

            Action act = () => eventSerializer.Deserialize(mockCommitedEvent.Object);

            act.ShouldThrowExactly<EventTypeNotFoundException>();
        }
    }

}