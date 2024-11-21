using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexicon_Garage2.Migrations
{
    /// <inheritdoc />
    public partial class AddedParkignSpotAndVehicleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleType",
                table: "Vehicle",
                newName: "VehicleTypeId");

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkedVehicleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSpots_Vehicle_ParkedVehicleId",
                        column: x => x.ParkedVehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VehicleTypeId",
                table: "Vehicle",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_ParkedVehicleId",
                table: "ParkingSpots",
                column: "ParkedVehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_VehicleTypes_VehicleTypeId",
                table: "Vehicle",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_VehicleTypes_VehicleTypeId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_VehicleTypeId",
                table: "Vehicle");

            migrationBuilder.RenameColumn(
                name: "VehicleTypeId",
                table: "Vehicle",
                newName: "VehicleType");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSpot",
                table: "Vehicle",
                type: "int",
                nullable: true);
        }
    }
}
