using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateTechinicianGroupToManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subcategories_TechnicianGroups_TechnicianGroupId",
                table: "Subcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TechnicianGroups_TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Subcategories_TechnicianGroupId",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TechnicianGroupId",
                table: "Subcategories");

          

            migrationBuilder.CreateTable(
                name: "SubcategoryTechnicianGroup",
                columns: table => new
                {
                    SubcategoriesId = table.Column<int>(type: "int", nullable: false),
                    TechnicianGroupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubcategoryTechnicianGroup", x => new { x.SubcategoriesId, x.TechnicianGroupsId });
                    table.ForeignKey(
                        name: "FK_SubcategoryTechnicianGroup_Subcategories_SubcategoriesId",
                        column: x => x.SubcategoriesId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcategoryTechnicianGroup_TechnicianGroups_TechnicianGroupsId",
                        column: x => x.TechnicianGroupsId,
                        principalTable: "TechnicianGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianGroupTicket",
                columns: table => new
                {
                    TechnicianGroupsId = table.Column<int>(type: "int", nullable: false),
                    TicketsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianGroupTicket", x => new { x.TechnicianGroupsId, x.TicketsId });
                    table.ForeignKey(
                        name: "FK_TechnicianGroupTicket_TechnicianGroups_TechnicianGroupsId",
                        column: x => x.TechnicianGroupsId,
                        principalTable: "TechnicianGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnicianGroupTicket_Tickets_TicketsId",
                        column: x => x.TicketsId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoryTechnicianGroup_TechnicianGroupsId",
                table: "SubcategoryTechnicianGroup",
                column: "TechnicianGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianGroupTicket_TicketsId",
                table: "TechnicianGroupTicket",
                column: "TicketsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubcategoryTechnicianGroup");

            migrationBuilder.DropTable(
                name: "TechnicianGroupTicket");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianGroupId",
                table: "Subcategories",
                type: "int",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TechnicianGroupId",
                table: "Tickets",
                column: "TechnicianGroupId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TechnicianGroups_TechnicianGroupId",
                table: "Tickets",
                column: "TechnicianGroupId",
                principalTable: "TechnicianGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
