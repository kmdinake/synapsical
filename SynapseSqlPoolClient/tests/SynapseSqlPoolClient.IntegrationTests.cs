using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class SynapseSqlPoolClientIntegrationTests
    {
        // Set these to your test Synapse SQL Pool endpoint and credentials for real integration tests
        private const string SqlPoolEndpoint = "<your-synapse-sqlpool-endpoint>";
        private const string Database = "<your-database>";
        private const string Username = "<your-username>";
        private const string Password = "<your-password>";
        private const string AadUser = "<aad-user>";
        private const string AadPassword = "<aad-password>";
        private const string ClientId = "<client-id>";
        private const string TenantId = "<tenant-id>";
        private const string ClientSecret = "<client-secret>";

        [Fact(Skip = "Set real endpoint and credentials to run integration test.")]
        public async Task FullCrudLifecycle_Works()
        {
            // Example: SQL Password
            var client = new SynapseSqlPoolClient(
                SqlPoolEndpoint,
                Database,
                SqlAuthMode.SqlPassword,
                username: Username,
                password: Password
            );

            // Example: Active Directory Password
            // var client = new SynapseSqlPoolClient(
            //     SqlPoolEndpoint,
            //     Database,
            //     SqlAuthMode.ActiveDirectoryPassword,
            //     username: AadUser,
            //     password: AadPassword
            // );

            // Example: Active Directory Integrated
            // var client = new SynapseSqlPoolClient(
            //     SqlPoolEndpoint,
            //     Database,
            //     SqlAuthMode.ActiveDirectoryIntegrated
            // );

            // Example: Active Directory Interactive
            // var client = new SynapseSqlPoolClient(
            //     SqlPoolEndpoint,
            //     Database,
            //     SqlAuthMode.ActiveDirectoryInteractive,
            //     username: AadUser
            // );

            // Example: Active Directory Service Principal
            // var client = new SynapseSqlPoolClient(
            //     SqlPoolEndpoint,
            //     Database,
            //     SqlAuthMode.ActiveDirectoryServicePrincipal,
            //     clientId: ClientId,
            //     password: ClientSecret,
            //     tenantId: TenantId
            // );

            // Example: AccessToken (DefaultAzureCredential)
            // var credential = new DefaultAzureCredential();
            // var client = new SynapseSqlPoolClient(
            //     SqlPoolEndpoint,
            //     Database,
            //     SqlAuthMode.AccessToken,
            //     credential: credential
            // );
            var tableName = "TestTable";
            var schema = "(Id INT PRIMARY KEY, Name NVARCHAR(100))";

            // Clean up if exists
            await client.DropTableAsync(tableName);

            // Create
            await client.CreateTableAsync(tableName, schema);
            Assert.True(await client.TableExistsAsync(tableName));

            // Insert
            await client.InsertRowAsync(tableName, new Dictionary<string, object> { ["Id"] = 1, ["Name"] = "Alice" });

            // Query
            var rows = await client.QueryAsync($"SELECT * FROM {tableName}");
            Assert.Single(rows);
            Assert.Equal("Alice", rows.First()["Name"]);

            // Update
            await client.UpdateRowsAsync(tableName, "Id = 1", new Dictionary<string, object> { ["Name"] = "Bob" });
            var updatedRows = await client.QueryAsync($"SELECT * FROM {tableName} WHERE Name = 'Bob'");
            Assert.Single(updatedRows);

            // Delete
            await client.DeleteRowsAsync(tableName, "Id = 1");
            var emptyRows = await client.QueryAsync($"SELECT * FROM {tableName}");
            Assert.Empty(emptyRows);

            // Drop
            await client.DropTableAsync(tableName);
            Assert.False(await client.TableExistsAsync(tableName));
        }
    }
}
