using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_DEPI_.Migrations
{
    /// <inheritdoc />
    public partial class ApplySecurityAndBusinessLogicChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationCarts_Rooms_RoomId1",
                table: "ReservationCarts");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCarts_RoomId1",
                table: "ReservationCarts");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "ReservationCarts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId1",
                table: "ReservationCarts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCarts_RoomId1",
                table: "ReservationCarts",
                column: "RoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationCarts_Rooms_RoomId1",
                table: "ReservationCarts",
                column: "RoomId1",
                principalTable: "Rooms",
                principalColumn: "RoomId");
        }
    }
}
