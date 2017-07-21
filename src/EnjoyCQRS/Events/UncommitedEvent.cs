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
using EnjoyCQRS.EventSource;
using EnjoyCQRS.Collections;

namespace EnjoyCQRS.Events
{
    internal class UncommitedEvent : IUncommitedEvent
    {
        private readonly long _ticks;
        public DateTime CreatedAt => new DateTime(_ticks);
        public Aggregate Aggregate { get; }
        public Guid AggregateId => Aggregate.Id;
        public IDomainEvent Data { get; }
        public int Version { get; }
        public IMetadataCollection Metadata { get; internal set; } = MetadataCollection.Empty;
        
        public UncommitedEvent(Aggregate aggregate, IDomainEvent @event, int version) : 
            this (aggregate, @event, version, DateTime.Now.Ticks, MetadataCollection.Empty)
        {
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private UncommitedEvent(Aggregate aggregate, IDomainEvent @event, int version, long ticks, IMetadataCollection metadata)
        {
            Aggregate = aggregate;
            Data = @event;
            Version = version;
            Metadata = Metadata.Merge(metadata);
            _ticks = ticks;
        }
    }
}