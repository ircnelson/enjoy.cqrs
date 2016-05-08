namespace EnjoyCQRS.Configuration
{
    public enum Lifetime
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        Singleton,
        /// <summary>
        /// Create a new instance per dependency.
        /// </summary>
        Transient,
        /// <summary>
        /// Keep the same instance per scope.
        /// </summary>
        Scope
    }
}