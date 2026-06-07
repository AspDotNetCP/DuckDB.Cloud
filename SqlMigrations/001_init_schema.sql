-- Schema initialization for DuckDB Cloud
-- File: 001_init_schema.sql
-- This file sets up the initial database schema

CREATE SEQUENCE IF NOT EXISTS seq_users_id START 1;

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_users_id'),
    username VARCHAR NOT NULL UNIQUE,
    email VARCHAR NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE SEQUENCE IF NOT EXISTS seq_trading_data_id START 1;

CREATE TABLE IF NOT EXISTS trading_data (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_trading_data_id'),
    user_id INTEGER NOT NULL,
    symbol VARCHAR NOT NULL,
    price DECIMAL(10, 2),
    quantity INTEGER,
    transaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_trading_data_user_id ON trading_data(user_id);
CREATE INDEX IF NOT EXISTS idx_trading_data_symbol ON trading_data(symbol);
