using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailToNotify",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManuallyAssignedToId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ManuallyAssignedToId",
                table: "Tickets",
                column: "ManuallyAssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Technicians_ManuallyAssignedToId",
                table: "Tickets",
                column: "ManuallyAssignedToId",
                principalTable: "Technicians",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Technicians_ManuallyAssignedToId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ManuallyAssignedToId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EmailToNotify",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ManuallyAssignedToId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Tickets");
        }
    }
}
