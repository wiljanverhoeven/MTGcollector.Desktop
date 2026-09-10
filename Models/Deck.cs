using System.Collections.Generic;

namespace MTGcollector_app.Models;

public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<DeckCard> Cards { get; set; } = new List<DeckCard>();
}
