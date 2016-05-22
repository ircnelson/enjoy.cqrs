using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.IntegrationTests.Stubs.DomainLayer
{
    public class FakeGame : SnapshotAggregate<FakeGameSnapshot>
    {
        public string NamePlayerTwo { get; private set; }

        public string NamePlayerOne { get; private set; }

        public FakeGame()
        {
        }

        public FakeGame(Guid id, string namePlayerOne, string namePlayerTwo)
        {
            Emit(new FakeGameCreated(id, namePlayerOne, namePlayerTwo));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<FakeGameCreated>(Apply);
        }

        private void Apply(FakeGameCreated obj)
        {
            Id = obj.AggregateId;
            NamePlayerOne = obj.NamePlayerOne;
            NamePlayerTwo = obj.NamePlayerTwo;
        }
        
        protected override FakeGameSnapshot CreateSnapshot()
        {
            return new FakeGameSnapshot
            {
                PlayerOne = NamePlayerOne,
                PlayerTwo = NamePlayerTwo
            };
        }

        protected override void RestoreFromSnapshot(FakeGameSnapshot snapshot)
        {
            NamePlayerOne = snapshot.PlayerOne;
            NamePlayerTwo = snapshot.PlayerTwo;
        }
    }

    public class FakeGameSnapshot : Snapshot
    {
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
    }
}