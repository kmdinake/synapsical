using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Synapsical.Synapse.SqlPool.Client
{
    /// <summary>
    /// Abstraction for creating SqlConnection instances.
    /// </summary>
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
    }
}
