CREATE TABLE "Scores" (
    score_id SERIAL PRIMARY KEY,
    user_id    INT       NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    deck_id    INT       NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
    score      INT       NOT NULL DEFAULT 0,
    scored_at TIMESTAMPZ  NOT NULL DEFAULT NOW()
);
