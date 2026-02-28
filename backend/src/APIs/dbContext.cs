using Microsoft.EntityFrameworkCore;
using Flashcards.APIs.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<CardType> Types { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Pair> Pairs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map to the exact table names in PostgreSQL (quoted/case-sensitive)
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<CardType>().ToTable("Type");
        modelBuilder.Entity<Deck>().ToTable("Deck");
        modelBuilder.Entity<Item>().ToTable("Item");
        modelBuilder.Entity<Pair>().ToTable("Pair");

        // ---- User ----
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ---- CardType ----
        modelBuilder.Entity<CardType>(entity =>
        {
            entity.HasKey(t => t.TypeId);
            entity.Property(t => t.TypeId).HasColumnName("type_id");
            entity.Property(t => t.TypeName).HasColumnName("type_name").HasMaxLength(10).IsRequired();

            entity.HasIndex(t => t.TypeName).IsUnique();

            entity.HasData(new CardType
            {
                TypeId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001"),
                TypeName = "text"
            });
        });

        // ---- Deck ----
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(d => d.DeckId);
            entity.Property(d => d.DeckId).HasColumnName("deck_id");
            entity.Property(d => d.UserId).HasColumnName("user_id");
            entity.Property(d => d.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(d => d.Description).HasColumnName("description");
            entity.Property(d => d.IsPublic).HasColumnName("is_public").HasDefaultValue(false);
            entity.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(d => d.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(d => d.User)
                  .WithMany(u => u.Decks)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Item ----
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(i => i.ItemId);
            entity.Property(i => i.ItemId).HasColumnName("item_id");
            entity.Property(i => i.DeckId).HasColumnName("deck_id");
            entity.Property(i => i.TypeId).HasColumnName("type_id");
            entity.Property(i => i.Value).HasColumnName("value").IsRequired();
            entity.Property(i => i.Position).HasColumnName("position").HasDefaultValue(0);

            entity.HasOne(i => i.Deck)
                  .WithMany(d => d.Items)
                  .HasForeignKey(i => i.DeckId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Type)
                  .WithMany()
                  .HasForeignKey(i => i.TypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Pair ----
        modelBuilder.Entity<Pair>(entity =>
        {
            entity.HasKey(p => p.PairId);
            entity.Property(p => p.PairId).HasColumnName("pair_id");
            entity.Property(p => p.DeckId).HasColumnName("deck_id");
            entity.Property(p => p.Item1Id).HasColumnName("item1_id");
            entity.Property(p => p.Item2Id).HasColumnName("item2_id");
            entity.Property(p => p.Position).HasColumnName("position").HasDefaultValue(0);

            entity.HasOne(p => p.Deck)
                  .WithMany(d => d.Pairs)
                  .HasForeignKey(p => p.DeckId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Item1)
                  .WithMany()
                  .HasForeignKey(p => p.Item1Id)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Item2)
                  .WithMany()
                  .HasForeignKey(p => p.Item2Id)
                  .OnDelete(DeleteBehavior.Cascade);

            // CHECK constraint: item1_id <> item2_id
            entity.HasCheckConstraint("different_items", "item1_id <> item2_id");
        });
    }
}