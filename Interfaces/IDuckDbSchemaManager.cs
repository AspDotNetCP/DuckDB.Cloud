namespace DuckDB.Cloud.Interfaces;

/// <summary>
/// Ensures MotherDuck schema is created and incrementally updated from SqlMigrations.
/// </summary>
public interface IDuckDbSchemaManager
{
    /// <summary>Runs <see cref="EnsureCreateTablesAsync"/> then <see cref="EnsureTableUpdatesAsync"/> once per process.</summary>
    Task EnsureSchemaAsync(string connectionName = "Production", CancellationToken cancellationToken = default);

    /// <summary>Applies numbered CREATE scripts in SqlMigrations (001_*.sql …).</summary>
    Task EnsureCreateTablesAsync(string connectionName = "Production", CancellationToken cancellationToken = default);

    /// <summary>Applies idempotent ALTER scripts in SqlMigrations/updates.</summary>
    Task EnsureTableUpdatesAsync(string connectionName = "Production", CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EnsureCreateTablesAsync"/>
    Task EnsureCreateTableAsync(string connectionName = "Production", CancellationToken cancellationToken = default)
        => EnsureCreateTablesAsync(connectionName, cancellationToken);

    /// <inheritdoc cref="EnsureTableUpdatesAsync"/>
    Task EnsureTableUpdatedAsync(string connectionName = "Production", CancellationToken cancellationToken = default)
        => EnsureTableUpdatesAsync(connectionName, cancellationToken);
}
