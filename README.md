# Synapsical - Azure Synapse SqlPool .NET Client Library
Synapsical is a .NET Client Library enables .NET developers to connect to Azure Synapse Analytics SQL Pools and perform CRUD operations on tables and data. The library supports both direct ADO.NET-style operations and seamless integration with Entity Framework Core (EF Core), allowing developers to use familiar LINQ and DbContext patterns as with SQL Server.

## Features
- **Connect to Synapse SQL Pools** using Azure AD or SQL authentication.
- **Perform CRUD operations** on tables and data.
- **EF Core integration** for code-first and database-first development.
- **Async-first API** for scalable applications.
- **Thread-safe** client design.
- **Extensible** for future Synapse features.

## Getting Started
Prerequisites
- .NET 6.0 or later
- Azure Synapse Analytics workspace with a dedicated SQL Pool
- Azure Active Directory credentials or SQL authentication credentials

Installation
Install via NuGet:
```shell
dotnet add package Synapsical.Synapse.SqlPool.Client
```

## Usage (ADO.NET Style)
```csharp
using Synapse.SqlPool.Client;
using Azure.Identity;

// Authenticate with Azure AD
var credential = new DefaultAzureCredential();
var sqlPoolClient = new SynapseSqlPoolClient("<sqlpool-endpoint>", credential);

// Create a table
await sqlPoolClient.CreateTableAsync("Employees", "(Id INT PRIMARY KEY, Name NVARCHAR(100), Age INT)");

// Insert a row
await sqlPoolClient.InsertRowAsync("Employees", new Dictionary<string, object>
{
    ["Id"] = 1,
    ["Name"] = "Alice",
    ["Age"] = 30
});

// Query rows
var employees = await sqlPoolClient.QueryAsync("SELECT * FROM Employees WHERE Age > 25");

// Update rows
await sqlPoolClient.UpdateRowsAsync("Employees", "Id = 1", new Dictionary<string, object>
{
    ["Age"] = 31
});

// Delete rows
await sqlPoolClient.DeleteRowsAsync("Employees", "Id = 1");

// Drop table
await sqlPoolClient.DropTableAsync("Employees");
```

## Usage (Entity Framework Core Style)
a. Define Entity and DbContext
```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

public class SynapseDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQL Server provider with Synapse connection string
        optionsBuilder.UseSqlServer("<synapse-connection-string>");
    }
}
```

b. CRUD Operations with EF Core
```csharp
using (var context = new SynapseDbContext())
{
    // Create
    context.Employees.Add(new Employee { Id = 1, Name = "Alice", Age = 30 });
    context.SaveChanges();

    // Read
    var employees = context.Employees.Where(e => e.Age > 25).ToList();

    // Update
    var emp = context.Employees.First();
    emp.Age = 31;
    context.SaveChanges();

    // Delete
    context.Employees.Remove(emp);
    context.SaveChanges();
}
```

c. Azure AD Authentication with EF Core
```csharp
public class SynapseDbContext : DbContext
{
    private readonly TokenCredential _credential;

    public SynapseDbContext(TokenCredential credential) 
    {
        _credential = credential;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conn = new SqlConnection("<synapse-endpoint>");
        conn.AccessToken = _credential.GetToken(
            new TokenRequestContext(new[] { "https://database.windows.net/.default" })
        ).Token;

        optionsBuilder.UseSqlServer(conn);
    }
} 
```

## API Reference
SynapseSqlPoolClient

| Method | Description |
|-|-|
| `CreateTableAsync(string tableName, string schemaDefinition)` | Creates a new table. |
| `DropTableAsync(string tableName)` | Drops an existing table. |
| `TableExistsAsync(string tableName)` | Checks if a table exists. |
| `ListTablesAsync()` | Lists all tables. |
| `InsertRowAsync(string tableName, IDictionary<string, object> rowData)` | Inserts a row. |
| `QueryAsync(string sqlQuery)` | Executes a SELECT query. |
| `UpdateRowsAsync(string tableName, string whereClause, IDictionary<string, object> updatedValues)` | Updates rows. |
| `DeleteRowsAsync(string tableName, string whereClause)` | Deletes rows. |

## Error Handling
- All methods throw specific exceptions for connection, authentication, and SQL errors
- Errors are logged and can be traced for diagnostics.

## Thread Safety
- The client is thread-safe for concurrent operations.
- Each operation manages its own connection lifecycle.

## Limitations
- Some T-SQL features and data types may not be supported by Synapse SQL Pools.
- EF Core migrations may require customizations for Synapse compatibility.

## Extensibility
- Future support for stored procedures, bulk operations, and advanced analytics.
- Extension points for custom authentication and logging.

## Best Practices
- Use async methods for scalability.
- Use Azure AD authentication for production workloads.
- Handle exceptions and log errors for diagnostics.