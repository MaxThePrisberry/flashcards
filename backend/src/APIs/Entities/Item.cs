using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Item")]
    public class Item
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("deck_id")]
        public int DeckId { get; set; }

        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [Column("value")]
        public string Value { get; set; } = null!;

        [Column("position")]
        public int Position { get; set; } = 0;

        [ForeignKey("DeckId")]
        public Deck Deck { get; set; } = null!;

        [ForeignKey("TypeId")]
        public CardType Type { get; set; } = null!;
    }
}
