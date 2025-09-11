using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Synapsical.Synapse.SqlPool.Client
{
    /// <summary>
    /// Abstraction for creating DbConnection instances.
    /// </summary>
    public interface ISqlConnectionFactory
    {
        Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
    }
}
