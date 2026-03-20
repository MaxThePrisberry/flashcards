using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("ReviewHistory")]
    public class ReviewHistory
    {
        [Key]
        [Column("history_id")]
        public Guid HistoryId { get; set; }

        [Column("deck_id")]
        public Guid DeckId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("reviewed_at")]
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DeckId")]
        public Deck Deck { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<ReviewPair> ReviewPairs { get; set; } = new List<ReviewPair>();
    }
}
