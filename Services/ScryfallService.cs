using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MTGcollector_app.Models;

namespace MTGcollector_app.Services;

public class ScryfallService
{
    private readonly HttpClient httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public class ScryfallSearchResponse
    {
        public List<ScryfallCard>? Data { get; set; }
    }

    public ScryfallService(HttpClient httpClient)
    {
        this.httpClient = httpClient;

        httpClient.DefaultRequestHeaders.UserAgent.Clear();

        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "MTGcollector",
                "1.0"));

        httpClient.DefaultRequestHeaders.Accept.Clear();

        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));
    }

    public async Task<List<ScryfallCard>> SearchCardsAsync(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<ScryfallCard>();

        var query =
            $"name:\"{name.Trim()}\"";

        var url =
            $"https://api.scryfall.com/cards/search" +
            $"?q={Uri.EscapeDataString(query)}";

        var response =
            await httpClient.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Scryfall returned " +
                $"{(int)response.StatusCode}: " +
                responseBody);
        }

        var result =
            JsonSerializer.Deserialize<ScryfallSearchResponse>(
                responseBody,
                JsonOptions);

        return result?.Data ?? new List<ScryfallCard>();
    }

    public async Task<ScryfallCard?> GetCardBySetAndCollectorNumberAsync(
        string setCode,
        string collectorNumber)
    {
        if (string.IsNullOrWhiteSpace(setCode) ||
            string.IsNullOrWhiteSpace(collectorNumber))
        {
            return null;
        }

        var url =
            $"https://api.scryfall.com/cards/" +
            $"{Uri.EscapeDataString(setCode.Trim().ToLowerInvariant())}/" +
            $"{Uri.EscapeDataString(collectorNumber.Trim())}";

        var response =
            await httpClient.GetAsync(url);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Scryfall returned " +
                $"{(int)response.StatusCode}: " +
                responseBody);
        }

        return JsonSerializer.Deserialize<ScryfallCard>(
            responseBody,
            JsonOptions);
    }
}