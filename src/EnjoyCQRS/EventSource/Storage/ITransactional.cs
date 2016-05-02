namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// This abstraction used on <see cref="Session"/> to garantee atomicity.
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
