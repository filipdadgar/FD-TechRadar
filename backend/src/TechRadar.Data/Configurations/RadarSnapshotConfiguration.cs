using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class RadarSnapshotConfiguration : IEntityTypeConfiguration<RadarSnapshot>
{
    public void Configure(EntityTypeBuilder<RadarSnapshot> builder)
    {
        builder.ToTable("radar_snapshots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.CapturedAt).IsRequired();
        builder.Property(s => s.TriggerEvent).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Entries).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(s => s.CapturedAt).IsDescending();
    }
}
