using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxCapacityToParkingSpot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingSpots_VehicleId",
                table: "ParkingSpots");

            migrationBuilder.DropColumn(
                name: "ParkingSpot",
                table: "Vehicle");

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "ParkingSpots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_VehicleId",
                table: "ParkingSpots",
                column: "VehicleId",
                unique: true,
                filter: "[VehicleId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingSpots_VehicleId",
                table: "ParkingSpots");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "ParkingSpots");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSpot",
                table: "Vehicle",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_VehicleId",
                table: "ParkingSpots",
                column: "VehicleId");
        }
    }
}
