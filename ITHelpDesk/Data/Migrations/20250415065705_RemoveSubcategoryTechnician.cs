using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubcategoryTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubcategoryTechnicians");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Subcategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_TechnicianGroupId",
                table: "Subcategories",
                column: "TechnicianGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subcategories_TechnicianGroups_TechnicianGroupId",
                table: "Subcategories",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subcategories_TechnicianGroups_TechnicianGroupId",
                table: "Subcategories");

            migrationBuilder.DropIndex(
                name: "IX_Subcategories_TechnicianGroupId",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Subcategories");

            migrationBuilder.CreateTable(
                name: "SubcategoryTechnicians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubcategoryId = table.Column<int>(type: "int", nullable: false),
                    TechnicianGroupId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubcategoryTechnicians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcategoryTechnicians_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcategoryTechnicians_TechnicianGroups_TechnicianGroupId",
                        column: x => x.TechnicianGroupId,
                        principalTable: "TechnicianGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcategoryTechnicians_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoryTechnicians_SubcategoryId",
                table: "SubcategoryTechnicians",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoryTechnicians_TechnicianGroupId",
                table: "SubcategoryTechnicians",
                column: "TechnicianGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoryTechnicians_TechnicianId",
                table: "SubcategoryTechnicians",
                column: "TechnicianId");
        }
    }
}
