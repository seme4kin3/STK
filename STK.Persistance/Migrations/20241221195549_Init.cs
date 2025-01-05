using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STK.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    ParrentOrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameOrganization = table.Column<string>(type: "text", nullable: true),
                    Tittle = table.Column<string>(type: "text", nullable: true),
                    CertificationObject = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    DateOfCertificateExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfIssueCertificate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeclarationOfConformity = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EconomicActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OKVDNnumber = table.Column<string>(type: "text", nullable: true),
                    Discription = table.Column<string>(type: "text", nullable: true),
                    Adress = table.Column<string>(type: "text", nullable: true),
                    IndexAdress = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EconomicActivities_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Managements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    INN = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Managements_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requisites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    INN = table.Column<string>(type: "text", nullable: true),
                    KPP = table.Column<string>(type: "text", nullable: true),
                    OGRN = table.Column<string>(type: "text", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstablishmentCreateName = table.Column<string>(type: "text", nullable: true),
                    AuthorizedCapital = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requisites_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_OrganizationId",
                table: "Certificates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomicActivities_OrganizationId",
                table: "EconomicActivities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Managements_OrganizationId",
                table: "Managements",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisites_OrganizationId",
                table: "Requisites",
                column: "OrganizationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "EconomicActivities");

            migrationBuilder.DropTable(
                name: "Managements");

            migrationBuilder.DropTable(
                name: "Requisites");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
