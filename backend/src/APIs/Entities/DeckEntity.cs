public class Deck {
    public Guid DeckId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
    public List<Item> Items { get; set; } = new();
    public List<Pair> Pairs { get; set; } = new();
}
