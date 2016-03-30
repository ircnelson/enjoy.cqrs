namespace MyCQRS.EventStore
{
    /// <summary>
    /// Abstraction of the Unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
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