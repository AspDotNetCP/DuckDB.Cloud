using DuckDB.Cloud.Interfaces;
using Microsoft.Extensions.Logging;

namespace DuckDB.Cloud;

/// <summary>
/// Connection testing utilities for DuckDB Cloud
/// </summary>
public class ConnectionTests
{
    private readonly IDuckDbConnectionManager _connectionManager;
    private readonly ILogger<ConnectionTests> _logger;

    public ConnectionTests(IDuckDbConnectionManager connectionManager, ILogger<ConnectionTests> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    /// <summary>
    /// Simple test: Verify connection is ready
    /// </summary>
    public async Task<bool> IsConnectionReadyAsync(string connectionName = "Production")
    {
        try
        {
            bool result = await _connectionManager.TestConnectionAsync(connectionName);
            _logger.LogInformation($"Connection test for '{connectionName}': {(result ? "READY ✓" : "NOT READY ✗")}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection test failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Detailed test: Check connection with query execution
    /// </summary>
    public async Task<ConnectionStatus> GetConnectionStatusAsync(string connectionName = "Production")
    {
        var status = new ConnectionStatus
        {
            ConnectionName = connectionName,
            IsConnected = false,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Test 1: Basic connectivity
            _logger.LogInformation($"Testing connection: {connectionName}");
            if (!await _connectionManager.TestConnectionAsync(connectionName))
            {
                status.ErrorMessage = "Connection test failed";
                return status;
            }

            // Test 2: Execute simple query
            _logger.LogInformation("Executing test query...");
            var result = await _connectionManager.ExecuteQueryAsync(connectionName, "SELECT 1 as test_value;");
            if (result == null)
            {
                status.ErrorMessage = "Query returned no result";
                return status;
            }

            // Test 3: Get available tables
            _logger.LogInformation("Checking tables...");
            var tablesResult = await _connectionManager.ExecuteQueryAsync(connectionName, 
                "SELECT table_name FROM information_schema.tables LIMIT 1;");

            status.IsConnected = true;
            status.ErrorMessage = null;
            status.QueryResult = result?.ToString();
            _logger.LogInformation("✓ Connection is READY");
        }
        catch (Exception ex)
        {
            status.ErrorMessage = ex.Message;
            _logger.LogError($"✗ Connection test failed: {ex.Message}");
        }

        return status;
    }

    /// <summary>
    /// Test all available connections
    /// </summary>
    public async Task<List<ConnectionStatus>> TestAllConnectionsAsync()
    {
        var results = new List<ConnectionStatus>();
        var connections = _connectionManager.GetAvailableConnections();

        _logger.LogInformation($"Testing {connections.Count()} connection(s)...");

        foreach (var connName in connections)
        {
            var status = await GetConnectionStatusAsync(connName);
            results.Add(status);
        }

        return results;
    }

    /// <summary>
    /// Performance test: Measure connection response time
    /// </summary>
    public async Task<ConnectionPerformance> MeasureConnectionPerformanceAsync(string connectionName = "Production")
    {
        var perf = new ConnectionPerformance
        {
            ConnectionName = connectionName,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var startTime = DateTime.UtcNow;
            
            // Test connection establishment
            var connectionReady = await _connectionManager.TestConnectionAsync(connectionName);
            perf.ConnectionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (!connectionReady)
            {
                perf.ErrorMessage = "Connection test failed";
                return perf;
            }

            // Test query execution
            startTime = DateTime.UtcNow;
            await _connectionManager.ExecuteQueryAsync(connectionName, "SELECT 1;");
            perf.QueryTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            perf.IsHealthy = perf.ConnectionTimeMs < 5000 && perf.QueryTimeMs < 5000;
            _logger.LogInformation($"Performance - Connection: {perf.ConnectionTimeMs:F2}ms, Query: {perf.QueryTimeMs:F2}ms");
        }
        catch (Exception ex)
        {
            perf.ErrorMessage = ex.Message;
            _logger.LogError($"Performance test failed: {ex.Message}");
        }

        return perf;
    }

    /// <summary>
    /// Database availability test: Check if tables exist
    /// </summary>
    public async Task<DatabaseInfo> GetDatabaseInfoAsync(string connectionName = "Production")
    {
        var info = new DatabaseInfo
        {
            ConnectionName = connectionName,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Check if connection works
            if (!await _connectionManager.TestConnectionAsync(connectionName))
            {
                info.ErrorMessage = "Cannot connect to database";
                return info;
            }

            // Get table count
            try
            {
                var tableCountResult = await _connectionManager.ExecuteQueryAsync(connectionName,
                    "SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema = 'main';");
                
                if (tableCountResult != null)
                {
                    info.TableCount = int.TryParse(tableCountResult.ToString(), out var count) ? count : 0;
                }
            }
            catch
            {
                info.TableCount = 0;
            }

            info.IsAvailable = true;
            _logger.LogInformation($"Database info: {info.TableCount} tables found");
        }
        catch (Exception ex)
        {
            info.ErrorMessage = ex.Message;
            _logger.LogError($"Database info check failed: {ex.Message}");
        }

        return info;
    }
}

/// <summary>
/// Connection status result
/// </summary>
public class ConnectionStatus
{
    public string? ConnectionName { get; set; }
    public bool IsConnected { get; set; }
    public string? ErrorMessage { get; set; }
    public string? QueryResult { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Connection performance metrics
/// </summary>
public class ConnectionPerformance
{
    public string? ConnectionName { get; set; }
    public double ConnectionTimeMs { get; set; }
    public double QueryTimeMs { get; set; }
    public bool IsHealthy { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Database information
/// </summary>
public class DatabaseInfo
{
    public string? ConnectionName { get; set; }
    public bool IsAvailable { get; set; }
    public int TableCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}
