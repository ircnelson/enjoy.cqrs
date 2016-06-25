using System;

namespace EnjoyCQRS.IntegrationTests.Shared.TestSuit
{
    [Flags]
    internal enum EventStoreMethods
    {
        Ctor = 0,
        Dispose = 1,
        BeginTransaction = 2,
        Rollback = 4,
        CommitAsync = 8,
        SaveAsync = 16,
        SaveSnapshotAsync = 32,
        GetAllEventsAsync = 64,
        GetLatestSnapshotByIdAsync = 128,
        GetEventsForwardAsync = 256
    }
}