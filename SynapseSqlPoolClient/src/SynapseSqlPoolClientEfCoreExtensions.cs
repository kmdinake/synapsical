using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Synapsical.Synapse.SqlPool.Client
{
    public static class SynapseSqlPoolClientEfCoreExtensions
    {
        /// <summary>
        /// Configures a DbContextOptionsBuilder to use a connection from SynapseSqlPoolClient.
        /// </summary>
        /// <param name="optionsBuilder">The DbContextOptionsBuilder to configure.</param>
        /// <param name="client">The SynapseSqlPoolClient instance.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public static async Task UseSynapseSqlPoolClientAsync(
            this DbContextOptionsBuilder optionsBuilder,
            SynapseSqlPoolClient client,
            CancellationToken cancellationToken = default)
        {
            var conn = await client.GetOpenConnectionAsync(cancellationToken);
            optionsBuilder.UseSqlServer(conn);
        }
    }
}
