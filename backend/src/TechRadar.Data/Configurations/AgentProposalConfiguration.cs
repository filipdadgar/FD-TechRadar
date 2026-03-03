using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechRadar.Core.Domain;

namespace TechRadar.Data.Configurations;

public class AgentProposalConfiguration : IEntityTypeConfiguration<AgentProposal>
{
    public void Configure(EntityTypeBuilder<AgentProposal> builder)
    {
        builder.ToTable("agent_proposals");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.ProposedName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.RecommendedQuadrant).HasConversion<int?>();
        builder.Property(p => p.RecommendedRing).HasConversion<int?>();
        builder.Property(p => p.EvidenceSummary).HasMaxLength(4000);
        builder.Property(p => p.SourceReferences).HasColumnType("jsonb").IsRequired();
        builder.Property(p => p.IsLlmEnriched).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.ProposalType).HasConversion<int>().IsRequired();
        builder.Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Property(p => p.ReviewedBy).HasMaxLength(200);
        builder.Property(p => p.ReviewerNotes).HasMaxLength(2000);
        builder.Property(p => p.DetectedAt).IsRequired();

        builder.HasIndex(p => new { p.Status, p.DetectedAt });
        builder.HasIndex(p => p.AgentRunId);
        builder.HasIndex(p => p.ExistingEntryId);

        builder.HasOne(p => p.AgentRun)
            .WithMany(r => r.Proposals)
            .HasForeignKey(p => p.AgentRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ExistingEntry)
            .WithMany()
            .HasForeignKey(p => p.ExistingEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ResultingEntry)
            .WithMany()
            .HasForeignKey(p => p.ResultingEntryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
