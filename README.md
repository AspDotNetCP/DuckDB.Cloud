# DuckDB Cloud Project

A C# project for connecting to DuckDB Cloud (MotherDuck) with support for multiple database connections and SQL-based schema management.

## Features

✅ **Multi-Connection Support** - Manage multiple DuckDB cloud connections (Development, Staging, Production)
✅ **MotherDuck Integration** - Full support for MotherDuck token-based authentication
✅ **Configuration Management** - Environment-specific appsettings.json
✅ **SQL Migrations** - Version-controlled SQL schema files
✅ **Dependency Injection** - Seamless integration with .NET DI container
✅ **Async Operations** - Full async/await support for all database operations

## Quick Start

### 1. Configure Connection Strings

Edit `appsettings.json` to add your MotherDuck tokens and connection details:

```json
{
  "DuckDbConnections": {
    "DefaultConnection": "Development",
    "Connections": [
      {
        "Name": "Development",
        "Database": "my_dev_db",
        "ConnectionString": "memory:",
        "MotherDuckToken": "YOUR_TOKEN_HERE",
        "CommandTimeout": 30,
        "IsReadOnly": false
      }
    ]
  }
}
```

### 2. Register Services

In your `Program.cs`:

```csharp
using DuckDB.Cloud;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DuckDB Cloud services
builder.Services.AddDuckDbCloud(builder.Configuration);

var app = builder.Build();

// Initialize DuckDB context
await app.Services.InitializeDuckDbAsync();

app.Run();
```

### 3. Use Connection Manager

```csharp
public class MyService
{
    private readonly IDuckDbConnectionManager _connectionManager;

    public MyService(IDuckDbConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task Example()
    {
        // Test connection
        bool isConnected = await _connectionManager.TestConnectionAsync("Development");

        // Execute query
        var result = await _connectionManager.ExecuteQueryAsync("Development", 
            "SELECT COUNT(*) FROM users;");

        // Execute command
        await _connectionManager.ExecuteCommandAsync("Development",
            "INSERT INTO users (username, email) VALUES ('john', 'john@example.com');");
    }
}
```

## Connection String Formats

### Development (Local Memory)
```
ConnectionString: "memory:"
```

### MotherDuck Cloud
```
ConnectionString: "md:your_database_name"
```

### Local File
```
ConnectionString: "/path/to/database.duckdb"
```

## SQL Migrations

SQL files are organized in the `SqlMigrations/` directory with versioning:

- `001_init_schema.sql` - Initial schema setup
- `002_add_tables.sql` - Additional tables
- etc.

Each file can be applied manually or through a migration runner.

## Connection Management

### Available Connections
```csharp
var connectionNames = connectionManager.GetAvailableConnections();
```

### Test Connection
```csharp
bool isWorking = await connectionManager.TestConnectionAsync("Production");
```

## Environment-Specific Configuration

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Staging.json` - Staging overrides
- `appsettings.Production.json` - Production overrides

## Getting MotherDuck Token

1. Sign up at [https://app.motherduck.com/](https://app.motherduck.com/)
2. Navigate to your account settings
3. Copy your authentication token
4. Add it to your connection configuration

## Project Structure

```
DuckDB.Cloud/
├── Config/
│   └── DuckDbConnectionManager.cs      # Connection management logic
├── Interfaces/
│   └── IDuckDbConnectionManager.cs     # Connection manager interface
├── Models/
│   └── DuckDbConnectionConfig.cs       # Configuration models
├── SqlMigrations/
│   └── 001_init_schema.sql             # Schema definition files
├── DuckDbContext.cs                    # Main context class
├── DuckDbServiceExtensions.cs          # DI extension methods
├── DuckDB.Cloud.csproj                 # Project file
├── appsettings.json                    # Base configuration
└── README.md                           # This file
```

## NuGet Dependencies

- `DuckDB.NET.Bindings` - DuckDB .NET bindings
- `Microsoft.Extensions.Configuration` - Configuration system
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `Microsoft.Extensions.Logging` - Logging framework

## License

Part of the CSon project
