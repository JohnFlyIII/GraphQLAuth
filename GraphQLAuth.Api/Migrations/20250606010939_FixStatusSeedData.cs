using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphQLAuth.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixStatusSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Status",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTimeOffset(new DateTime(2025, 6, 5, 20, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Status",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTimeOffset(new DateTime(2025, 6, 6, 1, 9, 3, 638, DateTimeKind.Unspecified).AddTicks(2190), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
