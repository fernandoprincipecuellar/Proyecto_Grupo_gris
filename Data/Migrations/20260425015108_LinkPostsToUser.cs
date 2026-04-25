using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Grupo_gris.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkPostsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ForumPosts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_UserId",
                table: "ForumPosts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumPosts_AspNetUsers_UserId",
                table: "ForumPosts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumPosts_AspNetUsers_UserId",
                table: "ForumPosts");

            migrationBuilder.DropIndex(
                name: "IX_ForumPosts_UserId",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ForumPosts");
        }
    }
}
