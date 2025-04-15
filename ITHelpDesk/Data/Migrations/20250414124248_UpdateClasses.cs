using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcategoryTechnicians_Technicians_TechnicianId",
                table: "SubcategoryTechnicians");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TechnicianId",
                table: "SubcategoryTechnicians",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "SubcategoryTechnicians",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TechnicianGroupId",
                table: "Tickets",
                column: "TechnicianGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoryTechnicians_TechnicianGroupId",
                table: "SubcategoryTechnicians",
                column: "TechnicianGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcategoryTechnicians_TechnicianGroups_TechnicianGroupId",
                table: "SubcategoryTechnicians",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubcategoryTechnicians_Technicians_TechnicianId",
                table: "SubcategoryTechnicians",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TechnicianGroups_TechnicianGroupId",
                table: "Tickets",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcategoryTechnicians_TechnicianGroups_TechnicianGroupId",
                table: "SubcategoryTechnicians");

            migrationBuilder.DropForeignKey(
                name: "FK_SubcategoryTechnicians_Technicians_TechnicianId",
                table: "SubcategoryTechnicians");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TechnicianGroups_TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_SubcategoryTechnicians_TechnicianGroupId",
                table: "SubcategoryTechnicians");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "SubcategoryTechnicians");

            migrationBuilder.AlterColumn<int>(
                name: "TechnicianId",
                table: "SubcategoryTechnicians",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubcategoryTechnicians_Technicians_TechnicianId",
                table: "SubcategoryTechnicians",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
