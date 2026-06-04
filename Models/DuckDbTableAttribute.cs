namespace DuckDB.Cloud.Models;

/// <summary>
/// Overrides the default GenericRepo table name (TypeName + "s").
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DuckDbTableAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
