using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianGroupIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TechnicianGroupId",
                table: "Notifications",
                column: "TechnicianGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TechnicianGroups_TechnicianGroupId",
                table: "Notifications",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TechnicianGroups_TechnicianGroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TechnicianGroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
