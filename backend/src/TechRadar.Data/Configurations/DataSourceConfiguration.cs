using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("data_sources");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.SourceType).HasConversion<int>().IsRequired();
        builder.Property(s => s.ConnectionDetails).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.Enabled).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.Name).IsUnique();
    }
}
