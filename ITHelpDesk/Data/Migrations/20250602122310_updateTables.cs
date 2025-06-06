using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_TechnicianGroups_SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.DropColumn(
                name: "PortId",
                table: "TechnicianGroups");

            migrationBuilder.DropColumn(
                name: "SeniorTechnicianId",
                table: "TechnicianGroups");

            migrationBuilder.AddColumn<int>(
                name: "PortId",
                table: "SeniorTechnicians",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "SeniorTechnicians",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeniorTechnicians_PortId",
                table: "SeniorTechnicians",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_SeniorTechnicians_TechnicianGroupId",
                table: "SeniorTechnicians",
                column: "TechnicianGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SeniorTechnicians_Ports_PortId",
                table: "SeniorTechnicians",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeniorTechnicians_TechnicianGroups_TechnicianGroupId",
                table: "SeniorTechnicians",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeniorTechnicians_Ports_PortId",
                table: "SeniorTechnicians");

            migrationBuilder.DropForeignKey(
                name: "FK_SeniorTechnicians_TechnicianGroups_TechnicianGroupId",
                table: "SeniorTechnicians");

            migrationBuilder.DropIndex(
                name: "IX_SeniorTechnicians_PortId",
                table: "SeniorTechnicians");

            migrationBuilder.DropIndex(
                name: "IX_SeniorTechnicians_TechnicianGroupId",
                table: "SeniorTechnicians");

            migrationBuilder.DropColumn(
                name: "PortId",
                table: "SeniorTechnicians");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "SeniorTechnicians");

            migrationBuilder.AddColumn<int>(
                name: "PortId",
                table: "TechnicianGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeniorTechnicianId",
                table: "TechnicianGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianGroups_PortId",
                table: "TechnicianGroups",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianGroups_SeniorTechnicianId",
                table: "TechnicianGroups",
                column: "SeniorTechnicianId");

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
    }
}
