using System.Collections.Generic;

namespace MTGcollector_app.Models;

public class Card
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ScryfallId { get; set; }

    public string? SetCode { get; set; }

    public string? SetName { get; set; }

    public string? CollectorNumber { get; set; }

    public string? ManaCost { get; set; }

    public decimal? ManaValue { get; set; }

    public string? TypeLine { get; set; }

    public string? OracleText { get; set; }

    public string? Power { get; set; }

    public string? Toughness { get; set; }

    public string? Rarity { get; set; }

    public string? Artist { get; set; }

    public string? ImageUrl { get; set; }

    public ICollection<CollectionCard> Copies { get; set; }
        = new List<CollectionCard>();
}