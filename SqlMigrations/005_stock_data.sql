-- Migration: 005_stock_data.sql
-- FMP quote snapshots linked to the user who scanned and optional vision scan row

CREATE SEQUENCE IF NOT EXISTS seq_stock_data_id START 1;

CREATE TABLE IF NOT EXISTS stock_data (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_stock_data_id'),
    user_id INTEGER NOT NULL,
    ai_vision_icon_detail_id INTEGER,
    symbol VARCHAR NOT NULL,
    name VARCHAR,
    query VARCHAR,
    price DECIMAL(18, 4),
    price_change DECIMAL(18, 4),
    change_percent DECIMAL(18, 4),
    market_cap DECIMAL(20, 2),
    volume BIGINT,
    exchange VARCHAR,
    day_low DECIMAL(18, 4),
    day_high DECIMAL(18, 4),
    year_low DECIMAL(18, 4),
    year_high DECIMAL(18, 4),
    open_price DECIMAL(18, 4),
    previous_close DECIMAL(18, 4),
    price_avg_50 DECIMAL(18, 4),
    price_avg_200 DECIMAL(18, 4),
    quote_timestamp VARCHAR,
    raw_response TEXT,
    fetched_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_stock_data_user_id ON stock_data(user_id);
CREATE INDEX IF NOT EXISTS idx_stock_data_symbol ON stock_data(symbol);
CREATE INDEX IF NOT EXISTS idx_stock_data_ai_vision_icon_detail_id ON stock_data(ai_vision_icon_detail_id);
