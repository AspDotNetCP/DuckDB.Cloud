// USAGE EXAMPLES FOR DuckDB.Cloud
// This file demonstrates how to use the DuckDB.Cloud library
// These code snippets should be placed in your application's Program.cs or services

/*

EXAMPLE 1: Setup in Program.cs (ASP.NET Core)
==============================================

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


EXAMPLE 2: Using Connection Manager in a Service
==================================================

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


EXAMPLE 3: Execute Commands
=============================

// Create a table
await _connectionManager.ExecuteCommandAsync("Production", @"
    CREATE TABLE IF NOT EXISTS symbols (
        id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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


EXAMPLE 4: List Available Connections
======================================

var connections = _connectionManager.GetAvailableConnections();
foreach (var conn in connections)
{
    Console.WriteLine($"Available connection: {conn}");
}


EXAMPLE 5: Configuration for Multiple Environments
===================================================

appsettings.json:
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

*/

namespace DuckDB.Cloud.Examples
{
    /// <summary>
    /// This file contains documentation on how to use the DuckDB.Cloud library.
    /// See the comments above for code examples.
    /// </summary>
    public class UsageExamples
    {
    }
}

