using Microsoft.EntityFrameworkCore;
using MTGcollector_app.Models;

namespace MTGcollector_app.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CollectionCard> CollectionCards => Set<CollectionCard>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Card>()
            .HasMany(c => c.Copies)
            .WithOne(c => c.Card)
            .HasForeignKey(c => c.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Deck>()
            .HasMany(d => d.Cards)
            .WithOne(dc => dc.Deck)
            .HasForeignKey(dc => dc.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollectionCard>()
            .HasMany<DeckCard>()
            .WithOne(dc => dc.CollectionCard)
            .HasForeignKey(dc => dc.CollectionCardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeckCard>()
            .HasIndex(dc => new
            {
                dc.DeckId,
                dc.CollectionCardId
            })
            .IsUnique();

        modelBuilder.Entity<Card>()
            .HasIndex(c => c.ScryfallId)
            .IsUnique();
    }
}