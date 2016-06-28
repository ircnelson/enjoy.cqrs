// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using System.Linq;

namespace EnjoyCQRS.EventSource
{
    public class EventSerializer : IEventSerializer
    {
        private readonly ITextSerializer _textSerializer;

        public EventSerializer(ITextSerializer textSerializer)
        {
            _textSerializer = textSerializer;
        }

        public ISerializedEvent Serialize(IAggregate aggregate, IDomainEvent @event, IEnumerable<KeyValuePair<string, string>> metadatas)
        {
            var metadata = new Metadata(metadatas);
            
            var aggregateId = metadata.GetValue(MetadataKeys.AggregateId, Guid.Parse);
            var aggregateVersion = metadata.GetValue(MetadataKeys.AggregateSequenceNumber, int.Parse);
            var serializedData = _textSerializer.Serialize(@event);
            var serializedMetadata = _textSerializer.Serialize(metadata);

            return new SerializedEvent(aggregateId, aggregateVersion, serializedData, serializedMetadata, metadata);
        }

        public IDomainEvent Deserialize(ICommitedEvent commitedEvent)
        {
            var metadata = _textSerializer.Deserialize<Metadata>(commitedEvent.SerializedMetadata);

            var eventClrType = metadata.GetValue(MetadataKeys.EventClrType);
            
            return (IDomainEvent) _textSerializer.Deserialize(commitedEvent.SerializedData, eventClrType);
        }
    }
}