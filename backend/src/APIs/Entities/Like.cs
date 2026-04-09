using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Likes")]
    public class Like
    {
        [Key]
        [Column("like_id")]
        public Guid LikeId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("deck_id")]
        public Guid DeckId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("DeckId")]
        public Deck Deck { get; set; } = null!;
    }
}
