using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Models;
using DuckDB.Cloud.Interfaces;
using DuckDB.Cloud.Schema;

namespace DuckDB.Cloud;

/// <summary>
/// Main DuckDB context for managing cloud database operations
/// </summary>
public class DuckDbContext
{
    private readonly DuckDbConnectionSettings _settings;
    private readonly ILogger<DuckDbContext> _logger;
    private readonly IDuckDbConnectionManager _connectionManager;

    public DuckDbContext(DuckDbConnectionSettings settings, ILogger<DuckDbContext> logger, IDuckDbConnectionManager connectionManager)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
    }

    /// <summary>
    /// Initialize DuckDB context - loads configuration and validates connections
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation($"Initializing DuckDB context with {_settings.Connections.Count} connection(s)");

        foreach (var connection in _settings.Connections)
        {
            _logger.LogInformation($"Registered connection: {connection.Name} -> {connection.Database}");
        }

        if (string.IsNullOrEmpty(_settings.DefaultConnection))
        {
            _settings.DefaultConnection = _settings.Connections.FirstOrDefault()?.Name ?? "default";
            _logger.LogWarning($"No default connection specified. Using: {_settings.DefaultConnection}");
        }

        await RunMigrationsAsync();
    }

    private async Task RunMigrationsAsync()
    {
        try
        {
            var connectionName = _settings.DefaultConnection ?? "Production";
            var schemaManager = new DuckDbSchemaManager(
                _connectionManager,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<DuckDbSchemaManager>.Instance);
            await schemaManager.EnsureSchemaAsync(connectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run DuckDB migrations");
        }
    }

    public DuckDbConnectionSettings GetSettings() => _settings;

    public DuckDbConnectionConfig? GetConnectionConfig(string connectionName)
    {
        return _settings.Connections.FirstOrDefault(c =>
            c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetConnectionNames() => _settings.Connections.Select(c => c.Name);
}
