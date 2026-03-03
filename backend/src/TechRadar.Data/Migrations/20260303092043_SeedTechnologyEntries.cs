using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace TechRadar.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTechnologyEntries : Migration
    {
        // Quadrant: ConnectivityProtocols=0, EdgePlatforms=1, ToolsAndFrameworks=2, StandardsAndTechniques=3
        // Ring: Adopt=0, Trial=1, Assess=2, Hold=3
        // Status: Active=0

        private static readonly DateTimeOffset SeedDate = new(2026, 3, 3, 0, 0, 0, TimeSpan.Zero);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entries = new[]
            {
                (Id: "a1b2c3d4-0001-0001-0001-000000000001", Name: "MQTT", Quadrant: 0, Ring: 0,
                 Desc: "Lightweight publish-subscribe messaging protocol for IoT and constrained devices.",
                 Rationale: "Industry standard for IoT messaging; supported by all major cloud providers and brokers."),
                (Id: "a1b2c3d4-0002-0002-0002-000000000002", Name: "LoRaWAN", Quadrant: 0, Ring: 0,
                 Desc: "Long Range Wide Area Network protocol for low-power IoT devices over unlicensed spectrum.",
                 Rationale: "Mature ecosystem with global network operators; ideal for battery-powered rural deployments."),
                (Id: "a1b2c3d4-0003-0003-0003-000000000003", Name: "Matter", Quadrant: 0, Ring: 1,
                 Desc: "IP-based smart home connectivity standard backed by Apple, Google, Amazon and others.",
                 Rationale: "Strong industry backing driving interoperability; ecosystem still maturing across devices."),
                (Id: "a1b2c3d4-0004-0004-0004-000000000004", Name: "Thread", Quadrant: 0, Ring: 1,
                 Desc: "IPv6 mesh networking protocol for low-power smart home devices.",
                 Rationale: "Foundation of Matter networking; growing adoption in premium smart home devices."),
                (Id: "a1b2c3d4-0005-0005-0005-000000000005", Name: "NB-IoT", Quadrant: 0, Ring: 2,
                 Desc: "Narrowband IoT cellular standard for low-power wide-area device connectivity.",
                 Rationale: "Viable for cellular-connected sensors but deployment varies by region and operator."),
                (Id: "a1b2c3d4-0006-0006-0006-000000000006", Name: "AWS IoT Core", Quadrant: 1, Ring: 0,
                 Desc: "Managed cloud IoT platform for device connectivity, messaging and fleet management.",
                 Rationale: "Proven at scale; deep AWS ecosystem integration for production IoT workloads."),
                (Id: "a1b2c3d4-0007-0007-0007-000000000007", Name: "Zephyr RTOS", Quadrant: 2, Ring: 1,
                 Desc: "Open-source real-time operating system for resource-constrained embedded devices.",
                 Rationale: "Linux Foundation project with growing hardware support; strong IoT and BLE stack."),
                (Id: "a1b2c3d4-0008-0008-0008-000000000008", Name: "DTDL", Quadrant: 3, Ring: 2,
                 Desc: "Digital Twin Definition Language for describing IoT device models and capabilities.",
                 Rationale: "Used by Azure Digital Twins; assess for interoperable device modelling across vendors."),
            };

            foreach (var (id, name, quadrant, ring, desc, rationale) in entries)
            {
                migrationBuilder.InsertData(
                    table: "technology_entries",
                    columns: new[] { "Id", "Name", "Description", "Quadrant", "Ring", "Rationale", "Tags", "Status", "CreatedAt", "LastReviewedAt" },
                    values: new object[] { Guid.Parse(id), name, desc, quadrant, ring, rationale, Array.Empty<string>(), 0, SeedDate, SeedDate });

                migrationBuilder.InsertData(
                    table: "ring_change_histories",
                    columns: new[] { "Id", "TechnologyEntryId", "PreviousRing", "NewRing", "NewQuadrant", "ChangedAt", "ChangedBy" },
                    values: new object[] { Guid.NewGuid(), Guid.Parse(id), null, ring, quadrant, SeedDate, "seed" });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("technology_entries", "Name",
                new object[] { "MQTT", "LoRaWAN", "Matter", "Thread", "NB-IoT", "AWS IoT Core", "Zephyr RTOS", "DTDL" });
        }
    }
}
