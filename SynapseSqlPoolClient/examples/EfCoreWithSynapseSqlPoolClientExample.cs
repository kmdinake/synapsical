using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synapsical.Synapse.SqlPool.Client;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreUsageExample
{
    public class EfCoreWithSynapseSqlPoolClientExample
    {
        public async Task ConfigureDbContextOptions(DbContextOptionsBuilder optionsBuilder, SynapseSqlPoolClient client)
        {
            // This will open a connection and configure EF Core to use it
            await optionsBuilder.UseSynapseSqlPoolClientAsync(client);
        }

        // Advanced: Using with context pooling
        public async Task ConfigureWithContextPooling(IServiceCollection services, SynapseSqlPoolClient client)
        {
            var conn = await client.GetOpenConnectionAsync();
            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(conn);
            });
        }

        // Advanced: Using with dependency injection and factory
        public static void AddSynapseDbContext(IServiceCollection services, SynapseSqlPoolClient client)
        {
            services.AddDbContextFactory<AppDbContext>((provider, options) =>
            {
                var conn = client.GetOpenConnectionAsync().GetAwaiter().GetResult();
                options.UseSqlServer(conn);
            });
        }
    }
}
