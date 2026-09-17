namespace MTGcollector_app.Models;

public class CollectionCard
{
    public int Id { get; set; }

    public int CardId { get; set; }

    public Card? Card { get; set; }

    public string? Category { get; set; }

    public bool IsFoil { get; set; }

    public bool IsUsed { get; set; }
}