CREATE TABLE "ReviewHistory" (
    history_id UUID PRIMARY KEY,
    user_id    INT       NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    deck_id    INT       NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
    reviewed_at TIMESTAMPZ NOT NULL DEFAULT NOW()
);
