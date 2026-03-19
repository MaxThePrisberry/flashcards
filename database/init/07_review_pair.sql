CREATE TABLE "ReviewPair" (
    review_id UUID PRIMARY KEY,
    history_id    UUID      NOT NULL REFERENCES "ReviewHistory"(history_id) ON DELETE CASCADE,
    pair_id       UUID      NOT NULL REFERENCES "Pair"(pair_id) ON DELETE CASCADE
);
