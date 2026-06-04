using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Config;
using DuckDB.Cloud.Interfaces;
using DuckDB.Cloud.Models;
using DuckDB.Cloud.Schema;

namespace DuckDB.Cloud;

/// <summary>
/// Extension methods for registering DuckDB services with dependency injection
/// </summary>
public static class DuckDbServiceExtensions
{
    /// <summary>
    /// Add DuckDB Cloud services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddDuckDbCloud(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Load DuckDB connection settings from configuration
        var settings = new DuckDbConnectionSettings();
        configuration.GetSection("DuckDbConnections").Bind(settings);

        // Validate that at least one connection is configured
        if (settings.Connections.Count == 0)
        {
            throw new InvalidOperationException(
                "No DuckDB connections configured. Add 'DuckDbConnections' section to appsettings.json");
        }

        // Register configuration
        services.AddSingleton(settings);

        // Register manager first (needed by context)
        services.AddSingleton<IDuckDbConnectionManager, DuckDbConnectionManager>();
        services.AddSingleton<IDuckDbSchemaManager, DuckDbSchemaManager>();

        // Register context (depends on manager)
        services.AddSingleton<DuckDbContext>();

        return services;
    }

    /// <summary>
    /// Initialize DuckDB context asynchronously
    /// </summary>
    public static async Task InitializeDuckDbAsync(this IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<DuckDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<DuckDbContext>>();

        try
        {
            await context.InitializeAsync();
            logger.LogInformation("DuckDB Cloud context initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to initialize DuckDB context: {ex.Message}");
            throw;
        }
    }
}
