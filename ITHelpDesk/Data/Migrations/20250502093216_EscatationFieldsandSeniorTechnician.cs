using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class EscatationFieldsandSeniorTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClosedByTechnicianId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeniorTechnicianId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeniorTechnicianResponse",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SeniorTechnicians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeniorTechnicians", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClosedByTechnicianId",
                table: "Tickets",
                column: "ClosedByTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeniorTechnicianId",
                table: "Tickets",
                column: "SeniorTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_SeniorTechnicians_SeniorTechnicianId",
                table: "Tickets",
                column: "SeniorTechnicianId",
                principalTable: "SeniorTechnicians",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Technicians_ClosedByTechnicianId",
                table: "Tickets",
                column: "ClosedByTechnicianId",
                principalTable: "Technicians",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_SeniorTechnicians_SeniorTechnicianId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Technicians_ClosedByTechnicianId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "SeniorTechnicians");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ClosedByTechnicianId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SeniorTechnicianId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ClosedByTechnicianId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SeniorTechnicianId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SeniorTechnicianResponse",
                table: "Tickets");
        }
    }
}
