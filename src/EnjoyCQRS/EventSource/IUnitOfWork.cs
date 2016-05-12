namespace EnjoyCQRS.EventSource
{
    /// <summary>
    /// Abstraction of the Unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Save modifications.
        /// </summary>
        void Commit();
    }
}