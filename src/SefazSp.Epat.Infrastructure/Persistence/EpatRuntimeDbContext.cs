#nullable enable

using Microsoft.EntityFrameworkCore;

namespace SefazSp.Epat.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the ePAT external-state stores. Shares the SQLite file with Elsa's
/// management/runtime contexts but keeps its own migration history table
/// (<c>__EFMigrationsHistory_Epat</c>, configured at registration).
/// </summary>
public sealed class EpatRuntimeDbContext(DbContextOptions<EpatRuntimeDbContext> options) : DbContext(options)
{
    public DbSet<EpatSnapshotRow> Snapshots => Set<EpatSnapshotRow>();
    public DbSet<ServiceInteractionRow> ServiceInteractions => Set<ServiceInteractionRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var snapshot = modelBuilder.Entity<EpatSnapshotRow>();
        snapshot.ToTable("EpatSnapshots");
        snapshot.HasKey(x => new { x.StoreKind, x.ProcessId });
        snapshot.Property(x => x.StoreKind).HasMaxLength(64);
        snapshot.Property(x => x.ProcessId).HasMaxLength(256);
        snapshot.Property(x => x.DocumentJson).IsRequired();
        snapshot.Property(x => x.Version).IsConcurrencyToken();

        var interaction = modelBuilder.Entity<ServiceInteractionRow>();
        interaction.ToTable("ServiceInteractions");
        interaction.HasKey(x => x.Id);
        interaction.Property(x => x.Id).ValueGeneratedOnAdd();
        interaction.HasIndex(x => x.CorrelationId);
        interaction.Property(x => x.CorrelationId).HasMaxLength(256);
        interaction.Property(x => x.Port).HasMaxLength(64);
        interaction.Property(x => x.Operation).HasMaxLength(128);
        interaction.Property(x => x.RequestJson).IsRequired();
        interaction.Property(x => x.ResponseJson).IsRequired();
    }
}
