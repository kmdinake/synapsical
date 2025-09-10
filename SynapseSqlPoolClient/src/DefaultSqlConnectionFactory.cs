using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace Synapsical.Synapse.SqlPool.Client
{
    /// <summary>
    /// Default implementation of ISqlConnectionFactory.
    /// </summary>
    public class DefaultSqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _server;
        private readonly string _database;
        private readonly string? _username;
        private readonly string? _password;
        private readonly string? _clientId;
        private readonly string? _tenantId;
        private readonly TokenCredential? _credential;
        private readonly SqlAuthMode _authMode;

        public DefaultSqlConnectionFactory(
            string server,
            string database,
            SqlAuthMode authMode,
            string? username = null,
            string? password = null,
            string? clientId = null,
            string? tenantId = null,
            TokenCredential? credential = null)
        {
            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentNullException(nameof(server));
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(database));
            
            _server = server;
            _database = database;
            _authMode = authMode;
            _username = username;
            _password = password;
            _clientId = clientId;
            _tenantId = tenantId;
            _credential = credential;
        }

        public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                InitialCatalog = _database,
                Encrypt = true,
                TrustServerCertificate = false,
                ConnectTimeout = 30
            };

            switch (_authMode)
            {
                case SqlAuthMode.SqlPassword:
                    builder.UserID = _username;
                    builder.Password = _password;
                    break;
                case SqlAuthMode.ActiveDirectoryPassword:
                    builder.UserID = _username;
                    builder.Password = _password;
                    builder["Authentication"] = "Active Directory Password";
                    break;
                case SqlAuthMode.ActiveDirectoryIntegrated:
                    builder["Authentication"] = "Active Directory Integrated";
                    break;
                case SqlAuthMode.ActiveDirectoryInteractive:
                    builder.UserID = _username;
                    builder["Authentication"] = "Active Directory Interactive";
                    break;
                case SqlAuthMode.ActiveDirectoryServicePrincipal:
                    builder.UserID = _clientId;
                    builder.Password = _password;
                    builder["Authentication"] = "Active Directory Service Principal";
                    if (!string.IsNullOrEmpty(_tenantId))
                        builder["Authority Id"] = _tenantId;
                    break;
                case SqlAuthMode.AccessToken:
                    // Use TokenCredential to get access token
                    break;
                default:
                    throw new NotSupportedException($"Authentication mode '{_authMode}' is not supported.");
            }

            var conn = new SqlConnection(builder.ConnectionString);

            if (_authMode == SqlAuthMode.AccessToken && _credential != null)
            {
                var token = await _credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }), cancellationToken);
                conn.AccessToken = token.Token;
            }

            await conn.OpenAsync(cancellationToken);
            return conn;
        }
    }
}
