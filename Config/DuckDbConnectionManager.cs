using System.Data;
using Npgsql;
using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Interfaces;
using DuckDB.Cloud.Models;

namespace DuckDB.Cloud.Config;

/// <summary>
/// Manages DuckDB cloud connections with support for multiple databases
/// Connects to DuckDB Cloud (MotherDuck) via PostgreSQL protocol
/// </summary>
public class DuckDbConnectionManager : IDuckDbConnectionManager
{
    private readonly DuckDbConnectionSettings _settings;
    private readonly ILogger<DuckDbConnectionManager> _logger;
    private readonly Dictionary<string, NpgsqlConnection> _connections = new();

    public DuckDbConnectionManager(DuckDbConnectionSettings settings, ILogger<DuckDbConnectionManager> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a connection to a specific database
    /// </summary>
    public async Task<dynamic> GetConnectionAsync(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            connectionName = _settings.DefaultConnection ?? "default";

        if (_connections.TryGetValue(connectionName, out var existingConnection))
        {
            if (existingConnection.State == ConnectionState.Open)
                return existingConnection;
        }

        var config = _settings.Connections.FirstOrDefault(c => c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
        if (config == null)
        {
            throw new InvalidOperationException($"Connection '{connectionName}' not found in configuration.");
        }

        try
        {
            // Build connection string for DuckDB Cloud (MotherDuck)
            // Format: Server=<host>;Port=5432;Database=<db>;User Id=<token>;Password=<token>;
            var connectionString = BuildConnectionString(config);

            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            _connections[connectionName] = connection;
            
            _logger.LogInformation($"Successfully connected to database: {config.Database}");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to connect to '{connectionName}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Build connection string from configuration
    /// </summary>
    private string BuildConnectionString(DuckDbConnectionConfig config)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = "api.motherduck.com",  // MotherDuck API endpoint
            Port = 5432,
            Database = config.Database,
            Username = config.MotherDuckToken ?? "service_user",
            Password = config.MotherDuckToken ?? string.Empty,
            Timeout = config.CommandTimeout
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Get all configured connection names
    /// </summary>
    public IEnumerable<string> GetAvailableConnections()
    {
        return _settings.Connections.Select(c => c.Name);
    }

    /// <summary>
    /// Execute a SQL query on a specific connection
    /// </summary>
    public async Task<object?> ExecuteQueryAsync(string connectionName, string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL query cannot be empty.", nameof(sql));

        var connection = await GetConnectionAsync(connectionName);
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = _settings.Connections
                .First(c => c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase))
                .CommandTimeout;
            return await command.ExecuteScalarAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Query execution failed on '{connectionName}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Execute a SQL command (non-query) on a specific connection
    /// </summary>
    public async Task ExecuteCommandAsync(string connectionName, string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL command cannot be empty.", nameof(sql));

        var connection = await GetConnectionAsync(connectionName);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = _settings.Connections
                .First(c => c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase))
                .CommandTimeout;
            await command.ExecuteNonQueryAsync();
            _logger.LogInformation($"Command executed successfully on '{connectionName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Command execution failed on '{connectionName}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Test connection to verify it works
    /// </summary>
    public async Task<bool> TestConnectionAsync(string connectionName)
    {
        try
        {
            var result = await ExecuteQueryAsync(connectionName, "SELECT 1;");
            _logger.LogInformation($"Connection test successful for '{connectionName}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Connection test failed for '{connectionName}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Dispose all connections
    /// </summary>
    public void Dispose()
    {
        foreach (var connection in _connections.Values)
        {
            connection?.Dispose();
        }
        _connections.Clear();
    }
}
