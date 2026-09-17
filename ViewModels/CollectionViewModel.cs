using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTGcollector_app.Models;
using MTGcollector_app.Services;
using System;
using System.Collections.ObjectModel;
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
    public partial decimal EditNumberOfCopies { get; set; }

    [ObservableProperty]
    public partial bool HasNoCards { get; set; } = true;

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

        HasNoCards = Cards.Count == 0;
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
    private async Task AddScryfallCardAsync(ScryfallCard? card)
    {
        if (card == null)
            return;

        try
        {
            SearchError = string.Empty;

            await collectionService.AddScryfallCardAsync(card);

            ImportMessage =
                $"Added {card.Name} " +
                $"({card.Set?.ToUpperInvariant()} {card.CollectorNumber}) " +
                "to your collection.";

            await LoadCardsAsync();
        }
        catch (Exception ex)
        {
            SearchError = $"Could not add card: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCardAsync(Card? card)
    {
        if (card == null)
            return;

        try
        {
            SearchError = string.Empty;
            await collectionService.DeleteCardAsync(card.Id);

            if (SelectedCard?.Id == card.Id)
                CancelEdit();

            ImportMessage = $"Removed {card.Name}.";
            await LoadCardsAsync();
        }
        catch (Exception ex)
        {
            SearchError = $"Could not remove card: {ex.Message}";
        }
    }

    [RelayCommand]
    private void StartEdit(Card? card)
    {
        if (card == null)
            return;

        SelectedCard = card;
        EditCardName = card.Name;
        EditNumberOfCopies = card.Copies.Count;
        SearchError = string.Empty;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        SelectedCard = null;
        EditCardName = string.Empty;
        EditNumberOfCopies = 0;
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (SelectedCard == null)
            return;

        if (string.IsNullOrWhiteSpace(EditCardName))
        {
            SearchError = "Card name cannot be empty.";
            return;
        }

        if (EditNumberOfCopies < 0)
            EditNumberOfCopies = 0;

        try
        {
            await collectionService.UpdateCardAsync(
                SelectedCard.Id,
                EditCardName,
                (int)EditNumberOfCopies);

            ImportMessage = $"Updated {EditCardName.Trim()}.";
            CancelEdit();
            await LoadCardsAsync();
        }
        catch (Exception ex)
        {
            SearchError = $"Could not save card: {ex.Message}";
        }
    }
}