using System;
using Azure.Core;
using Azure.Identity;
using Synapsical.Synapse.SqlPool.Client;

namespace Synapsical.Synapse.SqlPool.Client.Examples
{
    public class AuthModeUsageExample
    {
        public void SqlPasswordExample()
        {
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.SqlPassword,
                username: "youruser",
                password: "yourpassword"
            );
            // Use client methods here
        }

        public void ActiveDirectoryPasswordExample()
        {
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.ActiveDirectoryPassword,
                username: "aaduser@domain.com",
                password: "aadpassword"
            );
            // Use client methods here
        }

        public void ActiveDirectoryIntegratedExample()
        {
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.ActiveDirectoryIntegrated
            );
            // Use client methods here
        }

        public void ActiveDirectoryInteractiveExample()
        {
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.ActiveDirectoryInteractive,
                username: "aaduser@domain.com"
            );
            // Use client methods here
        }

        public void ActiveDirectoryServicePrincipalExample()
        {
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.ActiveDirectoryServicePrincipal,
                clientId: "your-client-id",
                password: "your-client-secret",
                tenantId: "your-tenant-id"
            );
            // Use client methods here
        }

        public void AccessTokenExample()
        {
            TokenCredential credential = new DefaultAzureCredential();
            var client = new SynapseSqlPoolClient(
                sqlPoolEndpoint: "your-server.database.windows.net",
                database: "yourdb",
                authMode: SqlAuthMode.AccessToken,
                credential: credential
            );
            // Use client methods here
        }
    }
}
