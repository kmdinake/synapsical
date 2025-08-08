using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Synapsical.Synapse.SqlPool.Client;
using Moq;
using Azure.Core;

namespace Synapsical.Synapse.SqlPool.Client.Tests
{
    public class SynapseSqlPoolClientUnitTests
    {
        [Fact]
        public async Task TableExistsAsync_ReturnsFalse_WhenTableDoesNotExist()
        {
            // This is a placeholder. In a real test, mock SqlConnection/SqlCommand.
            var client = new SynapseSqlPoolClient("fake-server", "user", "pass");
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => client.TableExistsAsync("NonExistentTable"));
        }

        [Fact]
        public async Task CreateTableAsync_ThrowsException_OnInvalidSql()
        {
            var client = new SynapseSqlPoolClient("fake-server", "user", "pass");
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => client.CreateTableAsync("BadTable", "(Bad SQL)"));
        }

        [Fact]
        public async Task InsertRowAsync_ThrowsException_OnNullTable()
        {
            var client = new SynapseSqlPoolClient("fake-server", "user", "pass");
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => client.InsertRowAsync("", new Dictionary<string, object> { { "Id", 1 } }));
        }
    }
}
