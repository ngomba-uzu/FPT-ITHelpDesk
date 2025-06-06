using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class PortIdToTechnicianGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.AddColumn<int>(
                name: "PortId",
                table: "TechnicianGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianGroups_PortId",
                table: "TechnicianGroups",
                column: "PortId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicianGroups_Ports_PortId",
                table: "TechnicianGroups",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups",
                column: "SeniorTechnicianId",
                principalTable: "SeniorTechnicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicianGroups_Ports_PortId",
                table: "TechnicianGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.DropIndex(
                name: "IX_TechnicianGroups_PortId",
                table: "TechnicianGroups");

            migrationBuilder.DropColumn(
                name: "PortId",
                table: "TechnicianGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicianGroups_SeniorTechnicians_SeniorTechnicianId",
                table: "TechnicianGroups",
                column: "SeniorTechnicianId",
                principalTable: "SeniorTechnicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           
        }
    }
}
