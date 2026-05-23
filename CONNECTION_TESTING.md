# Health Check & Connection Testing Guide

## Quick Test: Is Connection Ready?

### Method 1: Simple Test (Fastest)
```csharp
var connectionManager = serviceProvider.GetRequiredService<IDuckDbConnectionManager>();

bool isReady = await connectionManager.TestConnectionAsync("Production");
Console.WriteLine(isReady ? "✓ Connected" : "✗ Not connected");
```

### Method 2: Detailed Status Check
```csharp
// Use the ConnectionTests utility class
var tests = new ConnectionTests(connectionManager, logger);
var status = await tests.GetConnectionStatusAsync("Production");

Console.WriteLine($"Connected: {status.IsConnected}");
Console.WriteLine($"Error: {status.ErrorMessage}");
Console.WriteLine($"Query Result: {status.QueryResult}");
```

### Method 3: Performance Metrics
```csharp
var tests = new ConnectionTests(connectionManager, logger);
var perf = await tests.MeasureConnectionPerformanceAsync("Production");

Console.WriteLine($"Connection Time: {perf.ConnectionTimeMs}ms");
Console.WriteLine($"Query Time: {perf.QueryTimeMs}ms");
Console.WriteLine($"Healthy: {perf.IsHealthy}");
```

---

## ASP.NET Core Health Check Endpoints

Add these endpoints to your application's controller:

```csharp
using DuckDB.Cloud.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDuckDbConnectionManager _connectionManager;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IDuckDbConnectionManager connectionManager, ILogger<HealthController> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    // ENDPOINT 1: Quick Health Check
    // GET /api/health/db
    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            bool isHealthy = await _connectionManager.TestConnectionAsync("Production");
            
            if (isHealthy)
            {
                return Ok(new { status = "HEALTHY", message = "Database connection is ready ✓" });
            }
            else
            {
                _logger.LogWarning("Database health check failed");
                return StatusCode(503, new { status = "UNHEALTHY", message = "Database connection is not responding" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Health check exception: {ex.Message}");
            return StatusCode(503, new { status = "ERROR", error = ex.Message });
        }
    }

    // ENDPOINT 2: Detailed Health Check
    // GET /api/health/db-detailed
    [HttpGet("db-detailed")]
    public async Task<IActionResult> CheckDatabaseDetailed()
    {
        try
        {
            var isConnected = await _connectionManager.TestConnectionAsync("Production");
            
            if (!isConnected)
            {
                return StatusCode(503, new { status = "DISCONNECTED" });
            }

            // Try executing a query
            try
            {
                var result = await _connectionManager.ExecuteQueryAsync("Production", "SELECT 1 as test;");
                return Ok(new { 
                    status = "CONNECTED", 
                    message = "All systems operational ✓",
                    queryExecutable = result != null 
                });
            }
            catch (Exception queryEx)
            {
                return StatusCode(503, new { 
                    status = "CONNECTED_BUT_QUERY_FAILED", 
                    error = queryEx.Message 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Detailed health check failed: {ex.Message}");
            return StatusCode(503, new { status = "ERROR", error = ex.Message });
        }
    }

    // ENDPOINT 3: Full Diagnostic Check with Performance
    // GET /api/health/db-diagnostic
    [HttpGet("db-diagnostic")]
    public async Task<IActionResult> DiagnosticCheck()
    {
        var response = new
        {
            timestamp = DateTime.UtcNow,
            availableConnections = _connectionManager.GetAvailableConnections().ToList()
        };

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var isConnected = await _connectionManager.TestConnectionAsync("Production");
            sw.Stop();

            if (!isConnected)
            {
                return StatusCode(503, new { 
                    status = "DISCONNECTED",
                    connectionTimeMs = sw.ElapsedMilliseconds,
                    timestamp = response.timestamp,
                    availableConnections = response.availableConnections
                });
            }

            // Test query execution
            sw.Restart();
            try
            {
                await _connectionManager.ExecuteQueryAsync("Production", "SELECT 1 as test;");
                sw.Stop();

                return Ok(new {
                    status = "OPERATIONAL",
                    message = "All checks passed ✓",
                    isConnected = true,
                    queryExecutable = true,
                    connectionTimeMs = sw.ElapsedMilliseconds,
                    timestamp = response.timestamp,
                    availableConnections = response.availableConnections
                });
            }
            catch (Exception queryEx)
            {
                sw.Stop();
                return StatusCode(503, new {
                    status = "DEGRADED",
                    message = "Some checks failed - see details",
                    isConnected = true,
                    queryExecutable = false,
                    queryError = queryEx.Message,
                    connectionTimeMs = sw.ElapsedMilliseconds,
                    timestamp = response.timestamp,
                    availableConnections = response.availableConnections
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Diagnostic check failed: {ex.Message}");
            return StatusCode(503, new { 
                status = "ERROR", 
                error = ex.Message,
                timestamp = response.timestamp 
            });
        }
    }

    // ENDPOINT 4: Test Specific Connection
    // GET /api/health/db/test?connectionName=Production
    [HttpGet("db/test")]
    public async Task<IActionResult> TestConnection([FromQuery] string connectionName = "Production")
    {
        try
        {
            var isConnected = await _connectionManager.TestConnectionAsync(connectionName);
            var message = isConnected 
                ? $"✓ Connection '{connectionName}' is ready" 
                : $"✗ Connection '{connectionName}' failed";
            
            var statusCode = isConnected ? 200 : 503;
            return StatusCode(statusCode, new { 
                isConnected, 
                message, 
                connectionName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection test failed for '{connectionName}': {ex.Message}");
            return StatusCode(503, new { 
                isConnected = false, 
                message = $"✗ Error: {ex.Message}",
                connectionName,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
```

---

## Testing from Code

### Test All Connections
```csharp
var tests = new ConnectionTests(connectionManager, logger);
var results = await tests.TestAllConnectionsAsync();

foreach (var result in results)
{
    Console.WriteLine($"{result.ConnectionName}: {result.IsConnected}");
}
```

### Get Database Information
```csharp
var tests = new ConnectionTests(connectionManager, logger);
var dbInfo = await tests.GetDatabaseInfoAsync("Production");

Console.WriteLine($"Available: {dbInfo.IsAvailable}");
Console.WriteLine($"Table Count: {dbInfo.TableCount}");
```

---

## cURL Testing (from Command Line)

```bash
# Quick health check
curl http://localhost:5000/api/health/db

# Detailed check
curl http://localhost:5000/api/health/db-detailed

# Full diagnostic
curl http://localhost:5000/api/health/db-diagnostic

# Test specific connection
curl "http://localhost:5000/api/health/db/test?connectionName=Production"
```

---

## Postman Collection

```json
{
  "info": {
    "name": "DuckDB Health Checks",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Quick Health Check",
      "request": {
        "method": "GET",
        "url": "{{baseUrl}}/api/health/db"
      }
    },
    {
      "name": "Detailed Check",
      "request": {
        "method": "GET",
        "url": "{{baseUrl}}/api/health/db-detailed"
      }
    },
    {
      "name": "Diagnostic Check",
      "request": {
        "method": "GET",
        "url": "{{baseUrl}}/api/health/db-diagnostic"
      }
    },
    {
      "name": "Test Connection",
      "request": {
        "method": "GET",
        "url": "{{baseUrl}}/api/health/db/test?connectionName=Production"
      }
    }
  ]
}
```

---

## Docker Health Check Example

Add to your `Dockerfile`:

```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/api/health/db || exit 1
```

---

## Kubernetes Liveness Probe

Add to your deployment:

```yaml
livenessProbe:
  httpGet:
    path: /api/health/db
    port: 5000
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /api/health/db-detailed
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 2
```

---

## Expected Responses

### Success (Connected)
```json
HTTP 200 OK
{
  "status": "HEALTHY",
  "message": "Database connection is ready ✓",
  "connected": true,
  "timestamp": "2026-05-24T00:35:07Z"
}
```

### Failure (Not Connected)
```json
HTTP 503 Service Unavailable
{
  "status": "UNHEALTHY",
  "message": "Database connection is not responding",
  "connected": false,
  "timestamp": "2026-05-24T00:35:07Z"
}
```

### Error
```json
HTTP 503 Service Unavailable
{
  "status": "ERROR",
  "message": "Connection error: Unable to connect",
  "error": "Authentication failed",
  "connected": false
}
```

---

## Troubleshooting Connection Issues

### Issue: Connection times out
- Check your MotherDuck token is valid
- Verify network connectivity to api.motherduck.com
- Check firewall/proxy settings
- Increase CommandTimeout in configuration

### Issue: Authentication failed
- Regenerate token from MotherDuck dashboard
- Verify token format (should be a JWT)
- Check token hasn't expired
- Ensure token is correct in appsettings.json

### Issue: Database not found
- Verify database name matches MotherDuck
- Check connection string format (should be `md:database_name`)
- Confirm database exists in MotherDuck account

### Issue: Query execution fails but connection works
- Check if you have permission to access the database
- Verify table names and SQL syntax
- Check if data exists in the database
- Review error message for specific SQL issues
