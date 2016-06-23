using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FakeGameAggregate
{
    public class FakeGameCreated : DomainEvent
    {
        public string NamePlayerOne { get; }
        public string NamePlayerTwo { get; }

        public FakeGameCreated(Guid aggregateId, string namePlayerOne, string namePlayerTwo) : base(aggregateId)
        {
            NamePlayerOne = namePlayerOne;
            NamePlayerTwo = namePlayerTwo;
        }
    }
}