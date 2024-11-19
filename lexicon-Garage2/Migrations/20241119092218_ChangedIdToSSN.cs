using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class ChangedIdToSSN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SSN",
                table: "AspNetUsers");
        }
    }
}
