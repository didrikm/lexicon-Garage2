using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class RelationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Vehicle",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ApplicationUserId",
                table: "Vehicle",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_AspNetUsers_ApplicationUserId",
                table: "Vehicle",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_AspNetUsers_ApplicationUserId",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_ApplicationUserId",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Vehicle");
        }
    }
}
