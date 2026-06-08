namespace DuckDB.Cloud.Models;

/// <summary>
/// FMP quote snapshot stored when stock data is resolved for a scan.
/// </summary>
[DuckDbTable("stock_data")]
public class StockData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? AiVisionIconDetailId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Query { get; set; }
    public decimal? Price { get; set; }
    public decimal? PriceChange { get; set; }
    public decimal? ChangePercent { get; set; }
    public decimal? MarketCap { get; set; }
    public long? Volume { get; set; }
    public string? Exchange { get; set; }
    public decimal? DayLow { get; set; }
    public decimal? DayHigh { get; set; }
    public decimal? YearLow { get; set; }
    public decimal? YearHigh { get; set; }
    public decimal? OpenPrice { get; set; }
    public decimal? PreviousClose { get; set; }
    public decimal? PriceAvg_50 { get; set; }
    public decimal? PriceAvg_200 { get; set; }
    public string? QuoteTimestamp { get; set; }
    public string? RawResponse { get; set; }
    public DateTime? FetchedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
