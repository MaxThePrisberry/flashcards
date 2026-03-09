CREATE TABLE "ReviewHistory" (
    history_id SERIAL PRIMARY KEY,
    user_id    INT       NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    pair_id    INT       NOT NULL REFERENCES "Pair"(pair_id) ON DELETE CASCADE,
    got_it     BOOLEAN   NOT NULL DEFAULT FALSE,
    reviewed_at TIMESTAMPZ NOT NULL DEFAULT NOW()
);
