using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Synapsical.Synapse.SqlPool.Client;
using Xunit;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class SynapseSqlPoolClientEfCoreExtensionsTests
    {
        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_ConfiguresOptionsBuilder_WithOpenConnection()
        {
            var mockConn = new Mock<SqlConnection>(MockBehavior.Strict, "Server=.;Database=Test;");
            var mockClient = new Mock<SynapseSqlPoolClient>(MockBehavior.Strict, "server", Mock.Of<ISqlConnectionFactory>(), null);
            mockClient.Setup(c => c.GetOpenConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockConn.Object);

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            await optionsBuilder.UseSynapseSqlPoolClientAsync(mockClient.Object);

            // The extension should have set the connection as the underlying option
            var ext = optionsBuilder.Options.Extensions;
            Assert.Contains(ext, e => e is Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension);
        }

        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_AllowsAsyncContextCreation()
        {
            var mockConn = new Mock<SqlConnection>(MockBehavior.Strict, "Server=.;Database=Test;");
            var mockClient = new Mock<SynapseSqlPoolClient>(MockBehavior.Strict, "server", Mock.Of<ISqlConnectionFactory>(), null);
            mockClient.Setup(c => c.GetOpenConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockConn.Object);

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            await optionsBuilder.UseSynapseSqlPoolClientAsync(mockClient.Object);
            using var context = new DbContext(optionsBuilder.Options);
            Assert.NotNull(context.Database.GetDbConnection());
        }

        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_CanBeUsedWithContextPooling()
        {
            var mockConn = new Mock<SqlConnection>(MockBehavior.Strict, "Server=.;Database=Test;");
            var mockClient = new Mock<SynapseSqlPoolClient>(MockBehavior.Strict, "server", Mock.Of<ISqlConnectionFactory>(), null);
            mockClient.Setup(c => c.GetOpenConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockConn.Object);

            var services = new ServiceCollection();
            services.AddDbContextPool<DbContext>(async options =>
            {
                await options.UseSynapseSqlPoolClientAsync(mockClient.Object);
            });
            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<DbContext>();
            Assert.NotNull(context.Database.GetDbConnection());
        }

        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_CanBeUsedWithDbContextFactory()
        {
            var mockConn = new Mock<SqlConnection>(MockBehavior.Strict, "Server=.;Database=Test;");
            var mockClient = new Mock<SynapseSqlPoolClient>(MockBehavior.Strict, "server", Mock.Of<ISqlConnectionFactory>(), null);
            mockClient.Setup(c => c.GetOpenConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockConn.Object);

            var services = new ServiceCollection();
            services.AddDbContextFactory<DbContext>((provider, options) =>
            {
                options.UseSqlServer(mockConn.Object);
            });
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
            var context = factory.CreateDbContext();
            Assert.NotNull(context.Database.GetDbConnection());
        }
    }
}
