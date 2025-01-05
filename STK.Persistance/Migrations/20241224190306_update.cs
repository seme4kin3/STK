using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STK.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adress",
                table: "EconomicActivities");

            migrationBuilder.DropColumn(
                name: "IndexAdress",
                table: "EconomicActivities");

            migrationBuilder.AddColumn<string>(
                name: "Adress",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndexAdress",
                table: "Organizations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adress",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IndexAdress",
                table: "Organizations");

            migrationBuilder.AddColumn<string>(
                name: "Adress",
                table: "EconomicActivities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndexAdress",
                table: "EconomicActivities",
                type: "text",
                nullable: true);
        }
    }
}
