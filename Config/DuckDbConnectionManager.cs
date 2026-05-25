using System.Data;
using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Interfaces;
using DuckDB.Cloud.Models;
using DuckDB.NET.Data;

namespace DuckDB.Cloud.Config;

public class DuckDbConnectionManager : IDuckDbConnectionManager, IDisposable
{
    private readonly DuckDbConnectionSettings _settings;
    private readonly ILogger<DuckDbConnectionManager> _logger;

    private readonly Dictionary<string, DuckDBConnection> _connections = new();

    public DuckDbConnectionManager(
        DuckDbConnectionSettings settings,
        ILogger<DuckDbConnectionManager> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<dynamic> GetConnectionAsync(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            connectionName = _settings.DefaultConnection ?? "Production";

        if (_connections.TryGetValue(connectionName, out var existingConnection))
        {
            if (existingConnection.State == ConnectionState.Open)
                return existingConnection;
        }

        var config = _settings.Connections
            .FirstOrDefault(c =>
                c.Name.Equals(connectionName,
                StringComparison.OrdinalIgnoreCase));

        if (config == null)
            throw new InvalidOperationException(
                $"Connection '{connectionName}' not found.");

        try
        {
            var connectionString = BuildConnectionString(config);

            var connection = new DuckDBConnection(connectionString);

            await connection.OpenAsync();

            _connections[connectionName] = connection;

            _logger.LogInformation(
                "Connected to MotherDuck database: {Database}",
                config.Database);

            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed connecting to {Connection}",
                connectionName);

            throw;
        }
    }

    private string BuildConnectionString(
        DuckDbConnectionConfig config)
    {
        return
            $"DataSource=md:{config.Database}" +
            $"?motherduck_token={config.MotherDuckToken}";
    }

    public IEnumerable<string> GetAvailableConnections()
    {
        return _settings.Connections.Select(c => c.Name);
    }

    public async Task<object?> ExecuteQueryAsync(
        string connectionName,
        string sql)
    {
        var connection =
            (DuckDBConnection)await GetConnectionAsync(connectionName);

        using var command = connection.CreateCommand();

        command.CommandText = sql;

        return await command.ExecuteScalarAsync();
    }

    public async Task ExecuteCommandAsync(
        string connectionName,
        string sql)
    {
        var connection =
            (DuckDBConnection)await GetConnectionAsync(connectionName);

        using var command = connection.CreateCommand();

        command.CommandText = sql;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> TestConnectionAsync(
        string connectionName)
    {
        try
        {
            var result =
                await ExecuteQueryAsync(
                    connectionName,
                    "SELECT 1");

            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Connection test failed");

            return false;
        }
    }

    public void Dispose()
    {
        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }

        _connections.Clear();
    }
}