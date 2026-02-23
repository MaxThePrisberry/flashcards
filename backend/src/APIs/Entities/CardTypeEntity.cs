public class CardType {
    public Guid TypeId { get; set; }
    public string TypeName { get; set; }

    public List<Item> Items { get; set; } = new();
}
