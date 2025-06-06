using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PortId",
                table: "Notifications",
                column: "PortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Ports_PortId",
                table: "Notifications",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Ports_PortId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_PortId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PortId",
                table: "Notifications");
        }
    }
}
