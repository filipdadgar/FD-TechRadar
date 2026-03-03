using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class RingChangeHistoryConfiguration : IEntityTypeConfiguration<RingChangeHistory>
{
    public void Configure(EntityTypeBuilder<RingChangeHistory> builder)
    {
        builder.ToTable("ring_change_histories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        builder.Property(h => h.TechnologyEntryId).IsRequired();
        builder.Property(h => h.NewRing).HasConversion<int>().IsRequired();
        builder.Property(h => h.PreviousRing).HasConversion<int>();
        builder.Property(h => h.NewQuadrant).HasConversion<int>().IsRequired();
        builder.Property(h => h.PreviousQuadrant).HasConversion<int>();
        builder.Property(h => h.ChangedBy).HasMaxLength(200).IsRequired();
        builder.Property(h => h.ChangeReason).HasMaxLength(1000);
        builder.Property(h => h.ChangedAt).IsRequired();

        builder.HasIndex(h => new { h.TechnologyEntryId, h.ChangedAt });
    }
}
