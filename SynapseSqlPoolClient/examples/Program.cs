// See https://aka.ms/new-console-template for more information
using Synapsical.Synapse.SqlPool.Client;
Console.WriteLine("Hello, World!");

var sqlClient = new SynapseSqlPoolClient(
    sqlPoolEndpoint: "your-server.database.windows.net",
    database: "yourdb",
    authMode: SqlAuthMode.SqlPassword,
    username: "youruser",
    password: "yourpassword"
);

// Create a table
await sqlClient.CreateTableAsync("Employees", "(Id INT PRIMARY KEY, Name NVARCHAR(100), Age INT)");

// Insert a row
await sqlClient.InsertRowAsync("Employees", new Dictionary<string, object>
{
    ["Id"] = 1,
    ["Name"] = "Alice",
    ["Age"] = 30
});

// Query rows
var employees = await sqlClient.QueryAsync("SELECT * FROM Employees WHERE Age > 25");

// Update rows
await sqlClient.UpdateRowsAsync("Employees", "Id = 1", new Dictionary<string, object>
{
    ["Age"] = 31
});

// Delete rows
await sqlClient.DeleteRowsAsync("Employees", "Id = 1");

// Drop table
await sqlClient.DropTableAsync("Employees");