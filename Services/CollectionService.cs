using Microsoft.EntityFrameworkCore;
using MTGcollector_app.Data;
using MTGcollector_app.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGcollector_app.Services;

public class CollectionService
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;

    public CollectionService(
        IDbContextFactory<AppDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<List<Card>> GetCardsAsync()
    {
        await using var db =
            await dbFactory.CreateDbContextAsync();

        return await db.Cards
            .Include(c => c.Copies)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.SetCode)
            .ThenBy(c => c.CollectorNumber)
            .ToListAsync();
    }

    public async Task<Card> AddScryfallCardAsync(
        ScryfallCard scryfallCard,
        bool foil = false,
        int amount = 1)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync();

        var card =
            await db.Cards
                .Include(c => c.Copies)
                .FirstOrDefaultAsync(
                    c => c.ScryfallId ==
                         scryfallCard.Id);

        if (card == null)
        {
            card =
                CreateCardFromScryfall(
                    scryfallCard);

            db.Cards.Add(card);
        }

        for (var i = 0; i < amount; i++)
        {
            card.Copies.Add(
                new CollectionCard
                {
                    IsFoil = foil
                });
        }

        await db.SaveChangesAsync();

        return card;
    }

    public async Task DeleteCardAsync(int cardId)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync();

        var card =
            await db.Cards
                .FirstOrDefaultAsync(
                    c => c.Id == cardId);

        if (card == null)
            return;

        db.Cards.Remove(card);

        await db.SaveChangesAsync();
    }

    public async Task UpdateCardAsync(
        int cardId,
        string name,
        int numberOfCopies)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync();

        var card =
            await db.Cards
                .Include(c => c.Copies)
                .FirstOrDefaultAsync(
                    c => c.Id == cardId);

        if (card == null)
            return;

        card.Name = name.Trim();

        var currentCopies =
            card.Copies.Count;

        if (numberOfCopies > currentCopies)
        {
            var copiesToAdd =
                numberOfCopies - currentCopies;

            for (var i = 0; i < copiesToAdd; i++)
            {
                card.Copies.Add(
                    new CollectionCard
                    {
                        IsFoil = false
                    });
            }
        }
        else if (numberOfCopies < currentCopies)
        {
            var copiesToRemove =
                currentCopies - numberOfCopies;

            var removable =
                card.Copies
                    .OrderByDescending(c => c.Id)
                    .Take(copiesToRemove)
                    .ToList();

            db.CollectionCards
                .RemoveRange(removable);
        }

        await db.SaveChangesAsync();
    }

    private static Card CreateCardFromScryfall(
        ScryfallCard scryfallCard)
    {
        return new Card
        {
            Name = scryfallCard.Name,
            ScryfallId = scryfallCard.Id,
            SetCode = scryfallCard.Set,
            SetName = scryfallCard.SetName,
            CollectorNumber = scryfallCard.CollectorNumber,
            ManaCost = scryfallCard.ManaCost,
            ManaValue = scryfallCard.Cmc,
            TypeLine = scryfallCard.TypeLine,
            OracleText = scryfallCard.OracleText,
            Power = scryfallCard.Power,
            Toughness = scryfallCard.Toughness,
            Rarity = scryfallCard.Rarity,
            Artist = scryfallCard.Artist,
            ImageUrl = scryfallCard.GetImageUrl()
        };
    }
}