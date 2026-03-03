using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechRadar.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_run_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TriggerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourcesScanned = table.Column<int>(type: "integer", nullable: false),
                    SignalsFound = table.Column<int>(type: "integer", nullable: false),
                    ProposalsGenerated = table.Column<int>(type: "integer", nullable: false),
                    ProposalsDropped = table.Column<int>(type: "integer", nullable: false),
                    Errors = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_run_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "data_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ConnectionDetails = table.Column<string>(type: "jsonb", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSuccessfulScanAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "radar_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TriggerEvent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TriggerEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Entries = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_radar_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technology_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Quadrant = table.Column<int>(type: "integer", nullable: false),
                    Ring = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "ARRAY[]::text[]"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedFromProposalId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technology_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agent_proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecommendedQuadrant = table.Column<int>(type: "integer", nullable: true),
                    RecommendedRing = table.Column<int>(type: "integer", nullable: true),
                    EvidenceSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SourceReferences = table.Column<string>(type: "jsonb", nullable: false),
                    ConfidenceScore = table.Column<float>(type: "real", nullable: true),
                    IsLlmEnriched = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProposalType = table.Column<int>(type: "integer", nullable: false),
                    ExistingEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AgentRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewerNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResultingEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_proposals_agent_run_logs_AgentRunId",
                        column: x => x.AgentRunId,
                        principalTable: "agent_run_logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_proposals_technology_entries_ExistingEntryId",
                        column: x => x.ExistingEntryId,
                        principalTable: "technology_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_proposals_technology_entries_ResultingEntryId",
                        column: x => x.ResultingEntryId,
                        principalTable: "technology_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ring_change_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousRing = table.Column<int>(type: "integer", nullable: true),
                    NewRing = table.Column<int>(type: "integer", nullable: false),
                    PreviousQuadrant = table.Column<int>(type: "integer", nullable: true),
                    NewQuadrant = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ring_change_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ring_change_histories_technology_entries_TechnologyEntryId",
                        column: x => x.TechnologyEntryId,
                        principalTable: "technology_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_proposals_AgentRunId",
                table: "agent_proposals",
                column: "AgentRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_proposals_ExistingEntryId",
                table: "agent_proposals",
                column: "ExistingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_proposals_ResultingEntryId",
                table: "agent_proposals",
                column: "ResultingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_proposals_Status_DetectedAt",
                table: "agent_proposals",
                columns: new[] { "Status", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_agent_run_logs_StartedAt",
                table: "agent_run_logs",
                column: "StartedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_agent_run_logs_Status",
                table: "agent_run_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_Name",
                table: "data_sources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_radar_snapshots_CapturedAt",
                table: "radar_snapshots",
                column: "CapturedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ring_change_histories_TechnologyEntryId_ChangedAt",
                table: "ring_change_histories",
                columns: new[] { "TechnologyEntryId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_technology_entries_Name",
                table: "technology_entries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technology_entries_Quadrant_Status",
                table: "technology_entries",
                columns: new[] { "Quadrant", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_technology_entries_Ring_Status",
                table: "technology_entries",
                columns: new[] { "Ring", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_proposals");

            migrationBuilder.DropTable(
                name: "data_sources");

            migrationBuilder.DropTable(
                name: "radar_snapshots");

            migrationBuilder.DropTable(
                name: "ring_change_histories");

            migrationBuilder.DropTable(
                name: "agent_run_logs");

            migrationBuilder.DropTable(
                name: "technology_entries");
        }
    }
}
