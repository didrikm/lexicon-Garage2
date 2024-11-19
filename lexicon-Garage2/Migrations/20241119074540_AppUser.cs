using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class AppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SSN",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Id",
                table: "AspNetUsers",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Id",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SSN",
                table: "AspNetUsers",
                column: "SSN",
                unique: true);
        }
    }
}
