using System.Text.Json;
using GitInsight.Core;

namespace GitInsight.Infrastructure;

public class GitInsightContext : DbContext
{
    public DbSet<RepositoryEntry> Repositories { get; set; }
    
    public GitInsightContext(DbContextOptions<GitInsightContext> options)
        : base(options)
    {
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepositoryEntry>()
            .HasKey(r => new {r.URI, r.Mode})
            .HasName("PK_URI");

        modelBuilder.Entity<RepositoryEntry>()
            .Property(e => e.Mode)
            .HasConversion(new EnumToStringConverter<Mode>(new ConverterMappingHints(size: 50)));

        modelBuilder.Entity<RepositoryEntry>()
            .Property(e => e.Results)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Dictionary<string, object>>>(v, (JsonSerializerOptions)null)
            );
    }
}