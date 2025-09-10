namespace Synapsical.Synapse.SqlPool.Client
{
    /// <summary>
    /// Supported authentication modes for SQL connections.
    /// </summary>
    public enum SqlAuthMode
    {
        SqlPassword,
        ActiveDirectoryPassword,
        ActiveDirectoryIntegrated,
        ActiveDirectoryInteractive,
        ActiveDirectoryServicePrincipal,
        AccessToken
    }
}
