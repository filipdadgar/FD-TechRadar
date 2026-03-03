using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechRadar.Api.Middleware;
using TechRadar.Core.Interfaces;
using TechRadar.Core.Services;
using TechRadar.Data;
using TechRadar.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var pgHost = builder.Configuration["POSTGRES_HOST"] ?? "localhost";
var pgPort = builder.Configuration["POSTGRES_PORT"] ?? "5432";
var pgDb   = builder.Configuration["POSTGRES_DB"]   ?? "techradar";
var pgUser = builder.Configuration["POSTGRES_USER"]  ?? "techradar";
var pgPass = builder.Configuration["POSTGRES_PASSWORD"] ?? "techradar_dev";
var connStr = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";

builder.Services.AddDbContext<TechRadarDbContext>(opts =>
    opts.UseNpgsql(connStr));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEntryRepository, EntryRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<ISnapshotRepository, SnapshotRepository>();
builder.Services.AddScoped<IDataSourceRepository, DataSourceRepository>();
builder.Services.AddScoped<IAgentRunRepository, AgentRunRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<SnapshotService>();
builder.Services.AddScoped<EntryService>();
builder.Services.AddScoped<RadarService>();
builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<SnapshotComparisonService>();

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

// ── Agent trigger channel (shared singleton for manual trigger endpoint) ──────
builder.Services.AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<bool>(
    new System.Threading.Channels.UnboundedChannelOptions { SingleReader = true }));

// ── Auth ──────────────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET environment variable is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

// ── Controllers & CORS ────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Logging.AddJsonConsole();

var app = builder.Build();

// ── Migrate on startup ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbCtx = scope.ServiceProvider.GetRequiredService<TechRadarDbContext>();
    await dbCtx.Database.MigrateAsync();
}

app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
