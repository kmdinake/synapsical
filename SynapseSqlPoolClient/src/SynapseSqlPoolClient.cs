using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Synapsical.Synapse.SqlPool.Client
{
    /// <summary>
    /// Client for performing CRUD operations on Azure Synapse SQL Pools.
    /// </summary>
    public class SynapseSqlPoolClient
    {
        private readonly string _connectionString;
        private readonly TokenCredential? _credential;
        private readonly string? _username;
        private readonly string? _password;
        private readonly ILogger<SynapseSqlPoolClient>? _logger;
        private static readonly string s_diagnosticNamespace = "Synapsical.Synapse.SqlPool.Client";

        /// <summary>
        /// Initializes a new instance using Azure AD authentication.
        /// </summary>
        /// <param name="sqlPoolEndpoint">The Synapse SQL Pool endpoint.</param>
        /// <param name="credential">Azure credential.</param>
        public SynapseSqlPoolClient(string sqlPoolEndpoint, TokenCredential credential, ILogger<SynapseSqlPoolClient>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(sqlPoolEndpoint))
                throw new ArgumentException("SQL Pool endpoint must not be null or empty.", nameof(sqlPoolEndpoint));
            _connectionString = $"Server={sqlPoolEndpoint};Database=master;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance using SQL authentication.
        /// </summary>
        /// <param name="sqlPoolEndpoint">The Synapse SQL Pool endpoint.</param>
        /// <param name="username">SQL username.</param>
        /// <param name="password">SQL password.</param>
        public SynapseSqlPoolClient(string sqlPoolEndpoint, string username, string password, ILogger<SynapseSqlPoolClient>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(sqlPoolEndpoint))
                throw new ArgumentException("SQL Pool endpoint must not be null or empty.", nameof(sqlPoolEndpoint));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must not be null or empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must not be null or empty.", nameof(password));
            _connectionString = $"Server={sqlPoolEndpoint};Database=master;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            _username = username;
            _password = password;
            _logger = logger;
        }

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            if (_credential != null)
            {
                var token = await _credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                conn.AccessToken = token.Token;
            }
            await conn.OpenAsync();
            return conn;
        }

        /// <summary>
        /// Creates a new table in the SQL pool.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="schemaDefinition">Table schema definition (e.g., column definitions).</param>
        public async Task CreateTableAsync(string tableName, string schemaDefinition)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            if (string.IsNullOrWhiteSpace(schemaDefinition))
                throw new ArgumentException("Schema definition must not be null or empty.", nameof(schemaDefinition));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.CreateTable", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Creating table {TableName}", tableName);
            try
            {
                var sql = $"CREATE TABLE {tableName} {schemaDefinition}";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Successfully created table {TableName}", tableName);
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to create table {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to create table '{tableName}'.", ex);
            }
            catch (ArgumentException ex)
            {
                _logger?.LogError(ex, "Invalid argument for creating table {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Invalid argument for creating table '{tableName}'.", ex);
            }
        }

        /// <summary>
        /// Drops a table if it exists.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        public async Task DropTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.DropTable", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Dropping table {TableName}", tableName);
            try
            {
                var sql = $"DROP TABLE IF EXISTS {tableName}";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Successfully dropped table {TableName}", tableName);
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to drop table {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to drop table '{tableName}'.", ex);
            }
        }

        /// <summary>
        /// Checks if a table exists in the SQL pool.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <returns>True if the table exists, otherwise false.</returns>
        public async Task<bool> TableExistsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.TableExists", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Checking if table {TableName} exists", tableName);
            try
            {
                var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tableName", tableName);
                var result = (int)await cmd.ExecuteScalarAsync();
                _logger?.LogInformation("Table {TableName} exists: {Exists}", tableName, result > 0);
                return result > 0;
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to check if table {TableName} exists", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to check if table '{tableName}' exists.", ex);
            }
        }

        /// <summary>
        /// Lists all base tables in the SQL pool.
        /// </summary>
        /// <returns>Enumerable of table names.</returns>
        public async Task<IEnumerable<string>> ListTablesAsync()
        {
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.ListTables", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Listing all tables");
            try
            {
                var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                var tables = new List<string>();
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                _logger?.LogInformation("Found {Count} tables", tables.Count);
                return tables;
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to list tables");
                scope.Failed(ex);
                throw new SynapseSqlPoolException("Failed to list tables.", ex);
            }
        }

        /// <summary>
        /// Inserts a row into a table.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="rowData">Dictionary of column names and values.</param>
        public async Task InsertRowAsync(string tableName, IDictionary<string, object> rowData)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            if (rowData == null || rowData.Count == 0)
                throw new ArgumentException("Row data must not be null or empty.", nameof(rowData));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.InsertRow", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Inserting row into {TableName}", tableName);
            try
            {
                var columns = string.Join(", ", rowData.Keys);
                var parameters = string.Join(", ", rowData.Keys.Select(k => "@" + k));
                var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                foreach (var kvp in rowData)
                {
                    cmd.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                }
                await cmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Successfully inserted row into {TableName}", tableName);
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to insert row into {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to insert row into '{tableName}'.", ex);
            }
        }

        /// <summary>
        /// Executes a query and returns the results as a list of dictionaries.
        /// </summary>
        /// <param name="sqlQuery">The SQL SELECT query to execute.</param>
        /// <returns>Enumerable of rows as dictionaries.</returns>
        public async Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string sqlQuery)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException("SQL query must not be null or empty.", nameof(sqlQuery));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.Query", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Executing query");
            try
            {
                var results = new List<IDictionary<string, object>>();
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sqlQuery, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                _logger?.LogInformation("Query returned {Count} rows", results.Count);
                return results;
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to execute query");
                scope.Failed(ex);
                throw new SynapseSqlPoolException("Failed to execute query.", ex);
            }
        }

        /// <summary>
        /// Updates rows in a table matching the where clause.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="whereClause">WHERE clause to select rows.</param>
        /// <param name="updatedValues">Dictionary of columns and new values.</param>
        public async Task UpdateRowsAsync(string tableName, string whereClause, IDictionary<string, object> updatedValues)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("Where clause must not be null or empty.", nameof(whereClause));
            if (updatedValues == null || updatedValues.Count == 0)
                throw new ArgumentException("Updated values must not be null or empty.", nameof(updatedValues));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.UpdateRows", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Updating rows in {TableName} where {WhereClause}", tableName, whereClause);
            try
            {
                var setClause = string.Join(", ", updatedValues.Keys.Select(k => $"{k} = @{k}"));
                var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                foreach (var kvp in updatedValues)
                {
                    cmd.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                }
                await cmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Successfully updated rows in {TableName}", tableName);
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to update rows in {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to update rows in '{tableName}'.", ex);
            }
        }

        /// <summary>
        /// Deletes rows from a table matching the where clause.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="whereClause">WHERE clause to select rows.</param>
        public async Task DeleteRowsAsync(string tableName, string whereClause)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be null or empty.", nameof(tableName));
            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("Where clause must not be null or empty.", nameof(whereClause));
            using var scope = new DiagnosticScope($"{s_diagnosticNamespace}.DeleteRows", DiagnosticListener.Default, null);
            scope.Start();
            _logger?.LogInformation("Deleting rows from {TableName} where {WhereClause}", tableName, whereClause);
            try
            {
                var sql = $"DELETE FROM {tableName} WHERE {whereClause}";
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Successfully deleted rows from {TableName}", tableName);
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "Failed to delete rows from {TableName}", tableName);
                scope.Failed(ex);
                throw new SynapseSqlPoolException($"Failed to delete rows from '{tableName}'.", ex);
            }
        }
    }

    // Custom exceptions for error handling
    public class SynapseSqlPoolException : Exception
    {
        public SynapseSqlPoolException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
