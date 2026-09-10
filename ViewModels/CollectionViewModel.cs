using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTGcollector_app.Models;
using MTGcollector_app.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTGcollector_app.ViewModels;

public partial class CollectionViewModel : ViewModelBase
{
    private readonly CollectionService collectionService;
    private readonly ScryfallService scryfallService;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Searching { get; set; }

    [ObservableProperty]
    public partial string SearchError { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ImportMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewCardImportLine { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MassImportText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Card? SelectedCard { get; set; }

    [ObservableProperty]
    public partial string EditCardName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int EditNumberOfCopies { get; set; }

    public ObservableCollection<Card> Cards { get; } = new ObservableCollection<Card>();

    public ObservableCollection<ScryfallCard> SearchResults { get; } = new ObservableCollection<ScryfallCard>();

    public ObservableCollection<ImportResult> ImportResults { get; } = new ObservableCollection<ImportResult>();

    public CollectionViewModel(
        CollectionService collectionService,
        ScryfallService scryfallService)
    {
        this.collectionService = collectionService;
        this.scryfallService = scryfallService;
    }

    public async Task InitializeAsync()
    {
        await LoadCardsAsync();
    }

    private async Task LoadCardsAsync()
    {
        var cards =
            await collectionService.GetCardsAsync();

        Cards.Clear();

        foreach (var card in cards)
            Cards.Add(card);

        if (Cards.Count == 0)
        {
            var testCard = new Card
            {
                Name = "Lightning Bolt",
                SetCode = "LEA",
                SetName = "Limited Edition Alpha",
                CollectorNumber = "1",
                Rarity = "Common",
                ManaCost = "{R}",
                TypeLine = "Instant",
                OracleText = "Lightning Bolt deals 3 damage to any target.",
                ImageUrl = "https://cards.scryfall.io/normal/front/0/8/08a1e3e3-3f9e-43ef-b9b0-4b3ad0b18cd8.jpg?1662564876",
                Copies = new System.Collections.ObjectModel.ObservableCollection<CollectionCard>
                {
                    new CollectionCard { IsFoil = false },
                    new CollectionCard { IsFoil = true }
                }
            };
            Cards.Add(testCard);

            var testCard2 = new Card
            {
                Name = "Black Lotus",
                SetCode = "LEA",
                SetName = "Limited Edition Alpha",
                CollectorNumber = "232",
                Rarity = "Rare",
                ManaCost = "{0}",
                TypeLine = "Artifact",
                OracleText = "Tap, Sacrifice Black Lotus: Add three mana of any one color to your mana pool.",
                ImageUrl = "https://cards.scryfall.io/normal/front/b/d/bd8049b8-dcb9-4abc-880d-8c756e9e0f6a.jpg?1662563618",
                Copies = new System.Collections.ObjectModel.ObservableCollection<CollectionCard>
                {
                    new CollectionCard { IsFoil = false }
                }
            };
            Cards.Add(testCard2);
        }
    }

    [RelayCommand]
    private async Task SearchScryfallAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        Searching = true;
        SearchError = string.Empty;

        SearchResults.Clear();

        try
        {
            var results =
                await scryfallService
                    .SearchCardsAsync(SearchText);

            foreach (var card in results)
                SearchResults.Add(card);
        }
        catch (Exception ex)
        {
            SearchError =
                $"Scryfall search failed: {ex.Message}";
        }
        finally
        {
            Searching = false;
        }
    }

    [RelayCommand]
    private async Task AddScryfallCardAsync(
        ScryfallCard card)
    {
        try
        {
            await collectionService
                .AddScryfallCardAsync(card);

            ImportMessage =
                $"Added {card.Name} " +
                $"({card.Set?.ToUpperInvariant()} " +
                $"{card.CollectorNumber}) " +
                "to your collection.";

            await LoadCardsAsync();
        }
        catch (Exception ex)
        {
            SearchError =
                $"Could not add card: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCardAsync(
        Card? card)
    {
        if (card == null)
            return;

        await collectionService
            .DeleteCardAsync(card.Id);

        await LoadCardsAsync();
    }

    [RelayCommand]
    private void StartEdit(Card? card)
    {
        if (card == null)
            return;

        SelectedCard = card;

        EditCardName = card.Name;

        EditNumberOfCopies =
            card.Copies.Count;
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (SelectedCard == null)
            return;

        if (string.IsNullOrWhiteSpace(EditCardName))
            return;

        if (EditNumberOfCopies < 0)
            EditNumberOfCopies = 0;

        await collectionService
            .UpdateCardAsync(
                SelectedCard.Id,
                EditCardName,
                EditNumberOfCopies);

        SelectedCard = null;

        await LoadCardsAsync();
    }
}