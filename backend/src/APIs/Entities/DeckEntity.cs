namespace Flashcards.APIs.Entities {

    public class DeckEntity{
        public int DeckId {get; set;}
        public int UserId {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public bool IsPublic {get; set;} = false;
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
        public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

        public User User {get; set;}
        public List<Card> Cards {get; set;} = new ();
    }   
}