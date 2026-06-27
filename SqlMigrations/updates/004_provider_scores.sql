-- Provider scoring table for AI router ranking
-- Score = SuccessRate * 0.4 + LatencyScore * 0.4 + CostScore * 0.2
-- Higher score = better rank

CREATE TABLE IF NOT EXISTS ProviderScores (
    provider VARCHAR PRIMARY KEY,
    success_count INTEGER DEFAULT 0,
    failure_count INTEGER DEFAULT 0,
    quota_hits INTEGER DEFAULT 0,
    avg_latency_ms DOUBLE DEFAULT 0,
    score DOUBLE DEFAULT 0.5,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
