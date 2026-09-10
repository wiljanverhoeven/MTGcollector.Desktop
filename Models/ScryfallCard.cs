using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MTGcollector_app.Models;

public class ScryfallImageUris

{
    [JsonPropertyName("normal")]
    public string? Normal { get; set; }
}

public class ScryfallCard
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Set { get; set; }
    public string? SetName { get; set; }
    public string? CollectorNumber { get; set; }
    public string? ManaCost { get; set; }
    public decimal? Cmc { get; set; }
    public string? TypeLine { get; set; }
    public string? OracleText { get; set; }
    public string? Power { get; set; }
    public string? Toughness { get; set; }
    public string? Rarity { get; set; }
    public string? Artist { get; set; }

    [JsonPropertyName("image_uris")]
    public ScryfallImageUris? ImageUris { get; set; }

    public string GetImageUrl()
    {
        return ImageUris?.Normal ?? string.Empty;
    }
}

public class ScryfallSearchResponse
{
    [JsonPropertyName("data")]
    public List<ScryfallCard>? Data { get; set; }
}
