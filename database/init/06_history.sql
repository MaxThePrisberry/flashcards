CREATE TABLE "History" (
    history_id SERIAL PRIMARY KEY,
    user_id    INT       NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    pair_id    INT       NOT NULL REFERENCES "Pair"(pair_id) ON DELETE CASCADE,
    score      INT       NOT NULL,
    got_it     BOOLEAN   NOT NULL,
    reviewed_at TIMESTAMP NOT NULL DEFAULT NOW()
);
