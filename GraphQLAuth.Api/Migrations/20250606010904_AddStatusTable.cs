using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraphQLAuth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupportContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SystemVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "Id", "Health", "LastUpdated", "Notes", "SupportContact", "SystemVersion" },
                values: new object[] { 1, 0, new DateTimeOffset(new DateTime(2025, 6, 6, 1, 9, 3, 638, DateTimeKind.Unspecified).AddTicks(2190), new TimeSpan(0, 0, 0, 0, 0)), "System initialized", "support@demoapp.com", "1.0.0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Status");
        }
    }
}
