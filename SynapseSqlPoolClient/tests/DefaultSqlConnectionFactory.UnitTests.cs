using System;
using System.Threading.Tasks;
using Azure.Core;
using Moq;
using Xunit;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class DefaultSqlConnectionFactoryUnitTests
    {
        [Fact]
        public async Task Throws_NotSupportedException_For_Unknown_AuthMode()
        {
            var factory = new DefaultSqlConnectionFactory("server", "db", (SqlAuthMode)999);
            await Assert.ThrowsAsync<NotSupportedException>(() => factory.CreateOpenConnectionAsync());
        }

        [Fact]
        public async Task Throws_If_Server_Or_Database_Is_Missing()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultSqlConnectionFactory(null, "db", SqlAuthMode.SqlPassword));
            Assert.Throws<ArgumentNullException>(() => new DefaultSqlConnectionFactory("server", null, SqlAuthMode.SqlPassword));
        }

        [Fact]
        public void Can_Construct_For_All_AuthModes()
        {
            var cred = new Mock<TokenCredential>().Object;
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.SqlPassword, "user", "pass");
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.ActiveDirectoryPassword, "aaduser", "aadpass");
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.ActiveDirectoryIntegrated);
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.ActiveDirectoryInteractive, "aaduser");
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.ActiveDirectoryServicePrincipal, clientId: "cid", password: "secret", tenantId: "tid");
            _ = new DefaultSqlConnectionFactory("server", "db", SqlAuthMode.AccessToken, credential: cred);
        }
    }
}
