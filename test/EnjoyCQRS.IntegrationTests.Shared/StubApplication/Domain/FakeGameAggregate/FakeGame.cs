using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FakeGameAggregate
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

        public void ChangePlayerName(int player, string name)
        {
            Emit(new PlayerChangedName(Id, player, name));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<FakeGameCreated>(Apply);
            SubscribeTo<PlayerChangedName>(Apply);
        }

        private void Apply(FakeGameCreated obj)
        {
            Id = obj.AggregateId;
            NamePlayerOne = obj.NamePlayerOne;
            NamePlayerTwo = obj.NamePlayerTwo;
        }

        private void Apply(PlayerChangedName obj)
        {
            if (obj.Player == 1) NamePlayerOne = obj.Name;
            else NamePlayerTwo = obj.Name;
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