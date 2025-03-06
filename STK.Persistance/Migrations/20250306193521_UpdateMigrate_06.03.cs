using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STK.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigrate_0603 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EconomicActivityOrganization");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EconomicActivityOrganization",
                columns: table => new
                {
                    EconomicActivitiesId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicActivityOrganization", x => new { x.EconomicActivitiesId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_EconomicActivityOrganization_EconomicActivities_EconomicAct~",
                        column: x => x.EconomicActivitiesId,
                        principalTable: "EconomicActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EconomicActivityOrganization_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EconomicActivityOrganization_OrganizationId",
                table: "EconomicActivityOrganization",
                column: "OrganizationId");
        }
    }
}
