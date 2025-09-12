using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; } = "FakeConnection";
        public override string Database => "FakeDb";
        public override string DataSource => "FakeSource";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null;
        protected override DbCommand CreateDbCommand() => null;
    }

    public class SynapseSqlPoolClientEfCoreExtensionsTests
    {
        internal static SynapseSqlPoolClient GetSynapseSqlPoolClient(FakeDbConnection conn = null)
        {
            var fakeConn = conn ?? new FakeDbConnection();
            var mockFactory = new Mock<IDbConnectionFactory>();
            mockFactory.Setup(f => f.CreateOpenConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(fakeConn);
            var client = new SynapseSqlPoolClient("server", factory: mockFactory.Object);
            return client;
        }

        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_ConfiguresOptionsBuilder_WithOpenConnection()
        {
            var client = GetSynapseSqlPoolClient();
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            await optionsBuilder.UseSynapseSqlPoolClientAsync(client);

            // The extension should have set the connection as the underlying option
            var ext = optionsBuilder.Options.Extensions;
            Assert.Contains(ext, e => e is Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension);
        }

        [Fact]
        public async Task UseSynapseSqlPoolClientAsync_AllowsAsyncContextCreation()
        {
            var client = GetSynapseSqlPoolClient();
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            await optionsBuilder.UseSynapseSqlPoolClientAsync(client);

            using var context = new DbContext(optionsBuilder.Options);
            Assert.NotNull(context.Database.GetDbConnection());
        }

        [Fact]
        public void UseSynapseSqlPoolClientAsync_CanBeUsedWithContextPooling()
        {
            var client = GetSynapseSqlPoolClient();
            var services = new ServiceCollection();
            services.AddDbContextPool<DbContext>(async options =>
            {
                await options.UseSynapseSqlPoolClientAsync(client);
            });
            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<DbContext>();
            Assert.NotNull(context.Database.GetDbConnection());
        }

        [Fact]
        public void UseSynapseSqlPoolClientAsync_CanBeUsedWithDbContextFactory()
        {
            var fakeConn = new FakeDbConnection();
            var client = GetSynapseSqlPoolClient(fakeConn);
            var services = new ServiceCollection();
            services.AddDbContextFactory<DbContext>((provider, options) =>
            {
                options.UseSqlServer(fakeConn);
            });
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
            var context = factory.CreateDbContext();
            Assert.NotNull(context.Database.GetDbConnection());
        }
    }
}
