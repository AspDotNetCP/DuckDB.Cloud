# DuckDB Cloud Configuration Guide

## Getting Started with MotherDuck Token

### Step 1: Sign Up for MotherDuck
1. Visit: https://app.motherduck.com/
2. Create your account
3. Navigate to Account Settings

### Step 2: Get Your Authentication Token
1. In MotherDuck dashboard, go to "API Credentials" or "Tokens"
2. Generate a new token (or copy existing one)
3. Keep this token secure - treat it like a password

### Step 3: Configure Connection String

Update your `appsettings.json`:

```json
{
  "DuckDbConnections": {
    "DefaultConnection": "Development",
    "Connections": [
      {
        "Name": "Development",
        "Database": "my_dev_database",
        "ConnectionString": "memory:",
        "MotherDuckToken": "YOUR_MOTHERDUCK_TOKEN_HERE",
        "CommandTimeout": 30,
        "IsReadOnly": false
      },
      {
        "Name": "Production",
        "Database": "my_prod_database",
        "ConnectionString": "md:my_prod_database",
        "MotherDuckToken": "YOUR_PROD_TOKEN_HERE",
        "CommandTimeout": 120,
        "IsReadOnly": false
      }
    ]
  }
}
```

## Connection String Types

### Local Memory (Development Testing)
```
ConnectionString: "memory:"
```
- No persistent storage
- Perfect for testing
- Fastest performance

### MotherDuck Cloud
```
ConnectionString: "md:database_name"
```
- Requires valid MotherDuckToken
- Secure cloud storage
- Shared infrastructure

### Local File
```
ConnectionString: "/path/to/file.duckdb"
```
- Persistent local storage
- Good for development
- Can also use relative paths

## Environment-Specific Configurations

Create environment-specific files:

### appsettings.Development.json
```json
{
  "DuckDbConnections": {
    "DefaultConnection": "Development",
    "Connections": [
      {
        "Name": "Development",
        "Database": "trader_dev",
        "ConnectionString": "memory:",
        "MotherDuckToken": "dev_token_here",
        "CommandTimeout": 30,
        "IsReadOnly": false
      }
    ]
  }
}
```

### appsettings.Production.json
```json
{
  "DuckDbConnections": {
    "DefaultConnection": "Production",
    "Connections": [
      {
        "Name": "Production",
        "Database": "trader_prod",
        "ConnectionString": "md:trader_prod",
        "MotherDuckToken": "prod_token_from_env",
        "CommandTimeout": 120,
        "IsReadOnly": true
      }
    ]
  }
}
```

## Using Environment Variables

Secure sensitive data with environment variables:

```csharp
// In appsettings.json
{
  "DuckDbConnections": {
    "Connections": [
      {
        "Name": "Production",
        "Database": "trader_prod",
        "ConnectionString": "md:trader_prod",
        "MotherDuckToken": "${MOTHERDUCK_TOKEN}",  // Will be replaced by env variable
        "CommandTimeout": 120
      }
    ]
  }
}
```

Then set the environment variable:
```bash
# Windows
set MOTHERDUCK_TOKEN=your_token_here

# Linux/Mac
export MOTHERDUCK_TOKEN=your_token_here
```

## Security Best Practices

✅ **DO:**
- Use environment variables for tokens in production
- Store tokens in secure credential managers
- Use different tokens for different environments
- Enable read-only mode for production when possible
- Rotate tokens regularly

❌ **DON'T:**
- Commit tokens to source control
- Share tokens via email or chat
- Use the same token for multiple environments
- Store tokens in plain text files

## Testing Your Connection

Use the included Example.cs or this code:

```csharp
var connectionManager = serviceProvider.GetRequiredService<IDuckDbConnectionManager>();

// Test connection
bool isConnected = await connectionManager.TestConnectionAsync("Development");
Console.WriteLine($"Connected: {isConnected}");

// List all available connections
var connections = connectionManager.GetAvailableConnections();
foreach (var conn in connections)
{
    Console.WriteLine($"  - {conn}");
}
```

## Troubleshooting

### Connection Timeout
- Increase `CommandTimeout` value
- Check network connectivity
- Verify MotherDuck token is valid

### Authentication Failed
- Verify MotherDuckToken is correct
- Check token hasn't expired
- Generate a new token from MotherDuck dashboard

### Database Not Found
- Verify database name in configuration
- Check if database exists in MotherDuck
- For MotherDuck, use format: `md:database_name`

## Resources

- MotherDuck Documentation: https://docs.motherduck.com/
- DuckDB Documentation: https://duckdb.org/docs/
- GitHub Issues: Report problems here with reproduction steps
