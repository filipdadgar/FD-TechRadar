using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class TechnologyEntryConfiguration : IEntityTypeConfiguration<TechnologyEntry>
{
    public void Configure(EntityTypeBuilder<TechnologyEntry> builder)
    {
        builder.ToTable("technology_entries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Rationale).HasMaxLength(4000).IsRequired();
        builder.Property(e => e.Quadrant).HasConversion<int>().IsRequired();
        builder.Property(e => e.Ring).HasConversion<int>().IsRequired();
        builder.Property(e => e.Status).HasConversion<int>().IsRequired();
        builder.Property(e => e.Tags)
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]");
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.LastReviewedAt).IsRequired();

        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => new { e.Quadrant, e.Status });
        builder.HasIndex(e => new { e.Ring, e.Status });

        builder.HasMany(e => e.RingHistory)
            .WithOne(h => h.Entry)
            .HasForeignKey(h => h.TechnologyEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
