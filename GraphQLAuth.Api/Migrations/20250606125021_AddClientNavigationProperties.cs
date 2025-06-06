using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphQLAuth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClientNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Clients_ClientId",
                table: "Assets",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Clients_ClientId",
                table: "Blogs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Clients_ClientId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Clients_ClientId",
                table: "Blogs");
        }
    }
}
