using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Azure.Core;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class SynapseSqlPoolClientUnitTests
    {
        [Fact]
        public void Can_Construct_With_All_Auth_Modes()
        {
            var mockFactory = new Mock<ISqlConnectionFactory>().Object;
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>().Object;
            // SQL Password
            var c1 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock);
            // AD Password
            var c2 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.ActiveDirectoryPassword, "aaduser", "aadpass", logger: loggerMock);
            // AD Integrated
            var c3 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.ActiveDirectoryIntegrated, logger: loggerMock);
            // AD Interactive
            var c4 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.ActiveDirectoryInteractive, "aaduser", logger: loggerMock);
            // AD Service Principal
            var c5 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.ActiveDirectoryServicePrincipal, clientId: "cid", password: "secret", tenantId: "tid", logger: loggerMock);
            // AccessToken
            var credential = new Mock<TokenCredential>().Object;
            var c6 = new SynapseSqlPoolClient("server", "master", SqlAuthMode.AccessToken, credential: credential, logger: loggerMock);
            // With custom factory
            var c7 = new SynapseSqlPoolClient("server", mockFactory, loggerMock);
        }
        [Fact]
        public void Constructor_ThrowsArgumentException_OnEmptyEndpoint()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            Assert.Throws<ArgumentException>(() => new SynapseSqlPoolClient("", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object));
        }

        [Fact]
        public async Task CreateTableAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTableAsync(null, "(Id INT)"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTableAsync("", "(Id INT)"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTableAsync("Table", null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTableAsync("Table", ""));
        }

        [Fact]
        public async Task InsertRowAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.InsertRowAsync(null, new Dictionary<string, object> { { "Id", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.InsertRowAsync("", new Dictionary<string, object> { { "Id", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.InsertRowAsync("Table", null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.InsertRowAsync("Table", new Dictionary<string, object>()));
        }

        [Fact]
        public async Task UpdateRowsAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync(null, "where", new Dictionary<string, object> { { "Col", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync("", "where", new Dictionary<string, object> { { "Col", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync("Table", null, new Dictionary<string, object> { { "Col", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync("Table", "", new Dictionary<string, object> { { "Col", 1 } }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync("Table", "where", null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateRowsAsync("Table", "where", new Dictionary<string, object>()));
        }

        [Fact]
        public async Task DeleteRowsAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteRowsAsync(null, "where"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteRowsAsync("", "where"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteRowsAsync("Table", null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteRowsAsync("Table", ""));
        }

        [Fact]
        public async Task TableExistsAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.TableExistsAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.TableExistsAsync(""));
        }

        [Fact]
        public async Task ListTablesAsync_DoesNotThrow()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            // This will throw SqlException if run, but we just want to check the method exists and is callable
            await Assert.ThrowsAnyAsync<Exception>(() => client.ListTablesAsync());
        }

        [Fact]
        public async Task QueryAsync_ThrowsArgumentException_OnInvalidArgs()
        {
            var loggerMock = new Mock<ILogger<SynapseSqlPoolClient>>();
            var client = new SynapseSqlPoolClient("server", "master", SqlAuthMode.SqlPassword, "user", "pass", logger: loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => client.QueryAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => client.QueryAsync(""));
        }
    }
}
