using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphQLAuth.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToOneToManyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogAssets");

            migrationBuilder.AddColumn<Guid>(
                name: "BlogId",
                table: "Assets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_BlogId",
                table: "Assets",
                column: "BlogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Blogs_BlogId",
                table: "Assets",
                column: "BlogId",
                principalTable: "Blogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Blogs_BlogId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_BlogId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BlogId",
                table: "Assets");

            migrationBuilder.CreateTable(
                name: "BlogAssets",
                columns: table => new
                {
                    BlogId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogAssets", x => new { x.BlogId, x.AssetId });
                    table.ForeignKey(
                        name: "FK_BlogAsset_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogAsset_Blog_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogAssets_AssetId",
                table: "BlogAssets",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogAssets_BlogId",
                table: "BlogAssets",
                column: "BlogId");
        }
    }
}
