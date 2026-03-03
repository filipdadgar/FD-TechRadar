using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;

namespace TechRadar.Data;

public class TechRadarDbContext(DbContextOptions<TechRadarDbContext> options) : DbContext(options)
{
    public DbSet<TechnologyEntry> TechnologyEntries => Set<TechnologyEntry>();
    public DbSet<RingChangeHistory> RingChangeHistories => Set<RingChangeHistory>();
    public DbSet<RadarSnapshot> RadarSnapshots => Set<RadarSnapshot>();
    public DbSet<AgentProposal> AgentProposals => Set<AgentProposal>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<AgentRunLog> AgentRunLogs => Set<AgentRunLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechRadarDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
