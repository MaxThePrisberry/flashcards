using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Item")]
    public class Item
    {
        [Key]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Column("deck_id")]
        public Guid DeckId { get; set; }

        [Column("type_id")]
        public Guid TypeId { get; set; }

        [Required]
        [Column("value")]
        public string Value { get; set; } = null!;

        [Column("position")]
        public int Position { get; set; } = 0;

        public Deck Deck { get; set; } = null!;

        public CardType Type { get; set; } = null!;
    }
}
