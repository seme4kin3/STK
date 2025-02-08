using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STK.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetProfit",
                table: "Requisites");

            migrationBuilder.CreateTable(
                name: "BalanceSheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    AssetType = table.Column<string>(type: "text", nullable: true),
                    NonCurrentActive = table.Column<decimal>(type: "numeric", nullable: true),
                    CurrentActive = table.Column<decimal>(type: "numeric", nullable: true),
                    CapitalReserves = table.Column<decimal>(type: "numeric", nullable: true),
                    LongTermLiabilities = table.Column<decimal>(type: "numeric", nullable: true),
                    ShortTermLiabilities = table.Column<decimal>(type: "numeric", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceSheets_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: true),
                    CostOfSales = table.Column<decimal>(type: "numeric", nullable: true),
                    GrossProfitRevenue = table.Column<decimal>(type: "numeric", nullable: true),
                    GrossProfitEarnings = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesProfit = table.Column<decimal>(type: "numeric", nullable: true),
                    ProfitBeforeTax = table.Column<decimal>(type: "numeric", nullable: true),
                    NetProfit = table.Column<decimal>(type: "numeric", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialResults_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheets_OrganizationId",
                table: "BalanceSheets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialResults_OrganizationId",
                table: "FinancialResults",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceSheets");

            migrationBuilder.DropTable(
                name: "FinancialResults");

            migrationBuilder.AddColumn<int>(
                name: "NetProfit",
                table: "Requisites",
                type: "integer",
                nullable: true);
        }
    }
}
