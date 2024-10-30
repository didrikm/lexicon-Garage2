using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRegistrationConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_RegistrationNumber",
                table: "Vehicle",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicle_RegistrationNumber",
                table: "Vehicle");
        }
    }
}
