using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Models;

namespace DuckDB.Cloud;

/// <summary>
/// Main DuckDB context for managing cloud database operations
/// </summary>
public class DuckDbContext
{
    private readonly DuckDbConnectionSettings _settings;
    private readonly ILogger<DuckDbContext> _logger;

    public DuckDbContext(DuckDbConnectionSettings settings, ILogger<DuckDbContext> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }

    /// <summary>
    /// Get connection settings
    /// </summary>
    public DuckDbConnectionSettings GetSettings() => _settings;

    /// <summary>
    /// Get configuration for a specific connection
    /// </summary>
    public DuckDbConnectionConfig? GetConnectionConfig(string connectionName)
    {
        return _settings.Connections.FirstOrDefault(c => 
            c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// List all available connections
    /// </summary>
    public IEnumerable<string> GetConnectionNames() => _settings.Connections.Select(c => c.Name);
}
