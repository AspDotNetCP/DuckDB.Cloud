# DuckDB.Cloud Usage Examples

## Example 1: Setup in Program.cs (ASP.NET Core)

```csharp
using DuckDB.Cloud;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add logging
builder.Services.AddLogging();

// Add DuckDB Cloud services
builder.Services.AddDuckDbCloud(builder.Configuration);

var app = builder.Build();

// Initialize DuckDB context
await app.Services.InitializeDuckDbAsync();

app.Run();
```

## Example 2: Using Connection Manager in a Service

```csharp
using DuckDB.Cloud.Interfaces;

public class TradingDataService
{
    private readonly IDuckDbConnectionManager _connectionManager;

    public TradingDataService(IDuckDbConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task LoadTradingData()
    {
        // Test connection
        bool isConnected = await _connectionManager.TestConnectionAsync("Production");
        
        if (!isConnected)
        {
            throw new InvalidOperationException("Failed to connect to database");
        }

        // Execute query
        var result = await _connectionManager.ExecuteQueryAsync("Production", 
            "SELECT * FROM trading_data LIMIT 10;");
    }
}
```

## Example 3: Execute Commands

```csharp
// Create a table
await _connectionManager.ExecuteCommandAsync("Production", @"
    CREATE SEQUENCE IF NOT EXISTS seq_symbols_id START 1;
    CREATE TABLE IF NOT EXISTS symbols (
        id INTEGER PRIMARY KEY DEFAULT nextval('seq_symbols_id'),
        symbol VARCHAR UNIQUE NOT NULL,
        name VARCHAR,
        sector VARCHAR
    )
");

// Insert data
await _connectionManager.ExecuteCommandAsync("Production", @"
    INSERT INTO symbols (symbol, name, sector) 
    VALUES ('AAPL', 'Apple Inc', 'Technology')
");
```

## Example 4: List Available Connections

```csharp
var connections = _connectionManager.GetAvailableConnections();
foreach (var conn in connections)
{
    Console.WriteLine($"Available connection: {conn}");
}
```

## Example 5: Configuration for Multiple Environments

**appsettings.json:**
```json
{
  "DuckDbConnections": {
    "DefaultConnection": "Production",
    "Connections": [
      {
        "Name": "Production",
        "Database": "Icon",
        "ConnectionString": "md:Icon",
        "MotherDuckToken": "your_token_here",
        "CommandTimeout": 120,
        "IsReadOnly": false
      }
    ]
  }
}
```
