using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Interfaces;
using TechRadar.Core.Services;
using TechRadar.Data;
using TechRadar.Data.Repositories;
using TechRadar.Workers.Collectors;
using TechRadar.Workers.Services;
using TechRadar.Workers.Workers;

var builder = Host.CreateApplicationBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var pgHost = builder.Configuration["POSTGRES_HOST"] ?? "localhost";
var pgPort = builder.Configuration["POSTGRES_PORT"] ?? "5432";
var pgDb   = builder.Configuration["POSTGRES_DB"]   ?? "techradar";
var pgUser = builder.Configuration["POSTGRES_USER"]  ?? "techradar";
var pgPass = builder.Configuration["POSTGRES_PASSWORD"] ?? "techradar_dev";
var connStr = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";

builder.Services.AddDbContext<TechRadarDbContext>(opts => opts.UseNpgsql(connStr));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEntryRepository, EntryRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<ISnapshotRepository, SnapshotRepository>();
builder.Services.AddScoped<IDataSourceRepository, DataSourceRepository>();
builder.Services.AddScoped<IAgentRunRepository, AgentRunRepository>();

// ── Core Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<SnapshotService>();
builder.Services.AddScoped<EntryService>();

// ── LLM Classifier (optional) ─────────────────────────────────────────────────
var anthropicKey = builder.Configuration["ANTHROPIC_API_KEY"];
if (!string.IsNullOrWhiteSpace(anthropicKey))
{
    builder.Services.AddSingleton<ILlmClassifier>(
        new AnthropicLlmClassifier(
            anthropicKey,
            builder.Configuration["AGENTS_ANTHROPIC_MODEL"] ?? "claude-haiku-4-5-20251001"));
}
else
{
    builder.Services.AddSingleton<ILlmClassifier, PassthroughClassifier>();
}

// ── Agent infrastructure ──────────────────────────────────────────────────────
// Shared channel for manual trigger signals from the API
builder.Services.AddSingleton(Channel.CreateUnbounded<bool>(
    new UnboundedChannelOptions { SingleReader = true }));

builder.Services.AddScoped<RssFeedCollector>();
builder.Services.AddScoped<GitHubTopicsCollector>();
builder.Services.AddScoped<AgentScanService>();
builder.Services.AddHostedService<AgentScanWorker>();

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Logging.AddJsonConsole();

var host = builder.Build();
host.Run();
