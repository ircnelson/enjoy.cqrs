namespace EnjoyCQRS.EventStore.Storage
{
    /// <summary>
    /// This abstraction used on <see cref="EventStoreUnitOfWork"/> to garantee atomicity.
    /// </summary>
    public interface ITransactional
    {
        /// <summary>
        /// Start the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Confirm modifications.
        /// </summary>
        void Commit();

        /// <summary>
        /// Revert modifications.
        /// </summary>
        void Rollback();
    }
}
