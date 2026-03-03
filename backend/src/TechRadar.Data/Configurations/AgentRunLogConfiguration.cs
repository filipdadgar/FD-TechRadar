using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class AgentRunLogConfiguration : IEntityTypeConfiguration<AgentRunLog>
{
    public void Configure(EntityTypeBuilder<AgentRunLog> builder)
    {
        builder.ToTable("agent_run_logs");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.TriggerType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Errors).HasColumnType("jsonb").IsRequired().HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Status).HasConversion<int>().IsRequired();
        builder.Property(r => r.StartedAt).IsRequired();

        builder.HasIndex(r => r.StartedAt).IsDescending();
        builder.HasIndex(r => r.Status);
    }
}
