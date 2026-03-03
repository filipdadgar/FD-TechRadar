using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechRadar.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataSources : Migration
    {
        // SourceType: RssFeed=0, GitHubTopics=1
        private static readonly DateTimeOffset SeedDate = new(2026, 3, 3, 0, 0, 0, TimeSpan.Zero);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sources = new[]
            {
                (Name: "IETF RFC Feed",
                 Type: 0,
                 Details: "{\"url\":\"https://www.ietf.org/rss/rfc.xml\"}"),
                (Name: "IETF Datatracker",
                 Type: 0,
                 Details: "{\"url\":\"https://datatracker.ietf.org/feed/atom/\"}"),
                (Name: "IEEE Spectrum",
                 Type: 0,
                 Details: "{\"url\":\"https://spectrum.ieee.org/feeds/feed.rss\"}"),
                (Name: "Eclipse IoT",
                 Type: 0,
                 Details: "{\"url\":\"https://iot.eclipse.org/feed.xml\"}"),
                (Name: "Hackster.io",
                 Type: 0,
                 Details: "{\"url\":\"https://www.hackster.io/rss\"}"),
                (Name: "Embedded.com",
                 Type: 0,
                 Details: "{\"url\":\"https://www.embedded.com/rss\"}"),
                (Name: "IoT For All",
                 Type: 0,
                 Details: "{\"url\":\"https://www.iotforall.com/feed\"}"),
            };

            foreach (var (name, type, details) in sources)
            {
                migrationBuilder.InsertData(
                    table: "data_sources",
                    columns: new[] { "Id", "Name", "SourceType", "ConnectionDetails", "Enabled", "CreatedAt" },
                    values: new object[] { Guid.NewGuid(), name, type, details, true, SeedDate });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("data_sources", "Name",
                new object[]
                {
                    "IETF RFC Feed", "IETF Datatracker", "IEEE Spectrum",
                    "Eclipse IoT", "Hackster.io", "Embedded.com", "IoT For All"
                });
        }
    }
}
