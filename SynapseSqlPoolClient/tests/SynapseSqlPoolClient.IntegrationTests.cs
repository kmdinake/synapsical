using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Synapsical.Synapse.SqlPool.Client;
using Azure.Identity;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class SynapseSqlPoolClientIntegrationTests
    {
        // Set these to your test Synapse SQL Pool endpoint and credentials for real integration tests
        private const string SqlPoolEndpoint = "<your-synapse-sqlpool-endpoint>";
        private const string Username = "<your-username>";
        private const string Password = "<your-password>";

        [Fact(Skip = "Set real endpoint and credentials to run integration test.")]
        public async Task FullCrudLifecycle_Works()
        {
            var client = new SynapseSqlPoolClient(SqlPoolEndpoint, Username, Password);
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
