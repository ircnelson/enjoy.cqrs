using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs.DomainLayer
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