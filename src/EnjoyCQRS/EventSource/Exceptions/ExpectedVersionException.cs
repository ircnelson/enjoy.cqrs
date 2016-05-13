using System;

namespace EnjoyCQRS.EventSource.Exceptions
{
    public class ExpectedVersionException<TAggregate> : Exception
        where TAggregate : Aggregate
    {

        public TAggregate Aggregate { get; }
        public int ExpectedVersion { get; }

        public ExpectedVersionException(TAggregate aggregate, int expectedVersion)
        {
            Aggregate = aggregate;
            ExpectedVersion = expectedVersion;
        }
    }
}