using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class TechnicianGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Technicians",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TechnicianGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_TechnicianGroupId",
                table: "Technicians",
                column: "TechnicianGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_TechnicianGroups_TechnicianGroupId",
                table: "Technicians",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_TechnicianGroups_TechnicianGroupId",
                table: "Technicians");

            migrationBuilder.DropTable(
                name: "TechnicianGroups");

            migrationBuilder.DropIndex(
                name: "IX_Technicians_TechnicianGroupId",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Technicians");
        }
    }
}
