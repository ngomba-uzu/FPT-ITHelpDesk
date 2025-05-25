using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeniorTechnicianIdToTechnicianGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeniorTechnicianId",
                table: "TechnicianGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianGroups_SeniorTechnicianId",
                table: "TechnicianGroups",
                column: "SeniorTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups",
                column: "SeniorTechnicianId",
                principalTable: "SeniorTechnicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.DropIndex(
                name: "IX_TechnicianGroups_SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.DropColumn(
                name: "SeniorTechnicianId",
                table: "TechnicianGroups");
        }
    }
}
