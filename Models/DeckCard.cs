namespace MTGcollector_app.Models;

public class DeckCard
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public Deck? Deck { get; set; }
    public int CollectionCardId { get; set; }
    public CollectionCard? CollectionCard { get; set; }
}
