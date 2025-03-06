using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STK.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "EconomicActivities",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        OKVDNumber = table.Column<string>(type: "text", nullable: true),
            //        Description = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_EconomicActivities", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Organizations",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Name = table.Column<string>(type: "text", nullable: true),
            //        FullName = table.Column<string>(type: "text", nullable: true),
            //        Address = table.Column<string>(type: "text", nullable: true),
            //        IndexAddress = table.Column<string>(type: "text", nullable: true),
            //        ParentOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "text", nullable: true),
            //        Email = table.Column<string>(type: "text", nullable: true),
            //        Website = table.Column<string>(type: "text", nullable: true),
            //        StatusOrg = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Organizations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Roles",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Name = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Roles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TaxesModes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Name = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TaxesModes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Username = table.Column<string>(type: "text", nullable: true),
            //        Email = table.Column<string>(type: "text", nullable: true),
            //        PasswordHash = table.Column<string>(type: "text", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        IsActive = table.Column<bool>(type: "boolean", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BalanceSheets",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Year = table.Column<int>(type: "integer", nullable: false),
            //        AssetType = table.Column<string>(type: "text", nullable: true),
            //        NonCurrentActive = table.Column<decimal>(type: "numeric", nullable: true),
            //        CurrentActive = table.Column<decimal>(type: "numeric", nullable: true),
            //        CapitalReserves = table.Column<decimal>(type: "numeric", nullable: true),
            //        LongTermLiabilities = table.Column<decimal>(type: "numeric", nullable: true),
            //        ShortTermLiabilities = table.Column<decimal>(type: "numeric", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BalanceSheets", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_BalanceSheets_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Certificates",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Title = table.Column<string>(type: "text", nullable: true),
            //        Applicant = table.Column<string>(type: "text", nullable: true),
            //        Address = table.Column<string>(type: "text", nullable: true),
            //        Country = table.Column<string>(type: "text", nullable: true),
            //        CertificationObject = table.Column<string>(type: "text", nullable: true),
            //        DateOfIssueCertificate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        DateOfCertificateExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        CertificationType = table.Column<string>(type: "text", nullable: true),
            //        Status = table.Column<string>(type: "text", nullable: true),
            //        CertificateSuspensionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        CertificateRenewalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        PrescriptionReason = table.Column<string>(type: "text", nullable: true),
            //        SuspensionHistory = table.Column<string>(type: "text", nullable: true),
            //        Manufacturer = table.Column<string>(type: "text", nullable: true),
            //        ManufacturerCity = table.Column<string>(type: "text", nullable: true),
            //        ManufacturerAddress = table.Column<string>(type: "text", nullable: true),
            //        ManufacturerCountry = table.Column<string>(type: "text", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Certificates", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Certificates_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "EconomicActivityOrganization",
            //    columns: table => new
            //    {
            //        EconomicActivitiesId = table.Column<Guid>(type: "uuid", nullable: false),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_EconomicActivityOrganization", x => new { x.EconomicActivitiesId, x.OrganizationId });
            //        table.ForeignKey(
            //            name: "FK_EconomicActivityOrganization_EconomicActivities_EconomicAct~",
            //            column: x => x.EconomicActivitiesId,
            //            principalTable: "EconomicActivities",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_EconomicActivityOrganization_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FinancialResults",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Type = table.Column<string>(type: "text", nullable: true),
            //        Year = table.Column<int>(type: "integer", nullable: false),
            //        Revenue = table.Column<decimal>(type: "numeric", nullable: true),
            //        CostOfSales = table.Column<decimal>(type: "numeric", nullable: true),
            //        GrossProfitRevenue = table.Column<decimal>(type: "numeric", nullable: true),
            //        GrossProfitEarnings = table.Column<decimal>(type: "numeric", nullable: true),
            //        SalesProfit = table.Column<decimal>(type: "numeric", nullable: true),
            //        ProfitBeforeTax = table.Column<decimal>(type: "numeric", nullable: true),
            //        NetProfit = table.Column<decimal>(type: "numeric", nullable: true),
            //        IncomeTaxe = table.Column<decimal>(type: "numeric", nullable: true),
            //        TaxFee = table.Column<decimal>(type: "numeric", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FinancialResults", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_FinancialResults_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Licenses",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        NameTypeActivity = table.Column<string>(type: "text", nullable: true),
            //        SeriesNumber = table.Column<string>(type: "text", nullable: true),
            //        DateOfIssue = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        NameOrganizationIssued = table.Column<string>(type: "text", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Licenses", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Licenses_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Managements",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        FirstName = table.Column<string>(type: "text", nullable: true),
            //        LastName = table.Column<string>(type: "text", nullable: true),
            //        Position = table.Column<string>(type: "text", nullable: true),
            //        INN = table.Column<string>(type: "text", nullable: true),
            //        FullName = table.Column<string>(type: "text", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Managements", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Managements_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OrganizationsEconomicActivities",
            //    columns: table => new
            //    {
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
            //        EconomicActivityId = table.Column<Guid>(type: "uuid", nullable: false),
            //        IsMain = table.Column<bool>(type: "boolean", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OrganizationsEconomicActivities", x => new { x.OrganizationId, x.EconomicActivityId });
            //        table.ForeignKey(
            //            name: "FK_OrganizationsEconomicActivities_EconomicActivities_Economic~",
            //            column: x => x.EconomicActivityId,
            //            principalTable: "EconomicActivities",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_OrganizationsEconomicActivities_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Requisites",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        INN = table.Column<string>(type: "text", nullable: true),
            //        KPP = table.Column<string>(type: "text", nullable: true),
            //        OGRN = table.Column<string>(type: "text", nullable: true),
            //        DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        EstablishmentCreateName = table.Column<string>(type: "text", nullable: true),
            //        AuthorizedCapital = table.Column<int>(type: "integer", nullable: true),
            //        AvgCountEmployee = table.Column<int>(type: "integer", nullable: true),
            //        TypeOfCapital = table.Column<string>(type: "text", nullable: true),
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Requisites", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Requisites_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OrganizationTaxMode",
            //    columns: table => new
            //    {
            //        OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
            //        TaxesModesId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OrganizationTaxMode", x => new { x.OrganizationId, x.TaxesModesId });
            //        table.ForeignKey(
            //            name: "FK_OrganizationTaxMode_Organizations_OrganizationId",
            //            column: x => x.OrganizationId,
            //            principalTable: "Organizations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_OrganizationTaxMode_TaxesModes_TaxesModesId",
            //            column: x => x.TaxesModesId,
            //            principalTable: "TaxesModes",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RefreshTokens",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        Token = table.Column<string>(type: "text", nullable: true),
            //        Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //        Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //        UserId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RefreshTokens", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_RefreshTokens_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<Guid>(type: "uuid", nullable: false),
            //        RoleId = table.Column<Guid>(type: "uuid", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_UserRoles_Roles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "Roles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_UserRoles_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_BalanceSheets_OrganizationId",
            //    table: "BalanceSheets",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Certificates_OrganizationId",
            //    table: "Certificates",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_EconomicActivityOrganization_OrganizationId",
            //    table: "EconomicActivityOrganization",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FinancialResults_OrganizationId",
            //    table: "FinancialResults",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Licenses_OrganizationId",
            //    table: "Licenses",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Managements_OrganizationId",
            //    table: "Managements",
            //    column: "OrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_OrganizationsEconomicActivities_EconomicActivityId",
            //    table: "OrganizationsEconomicActivities",
            //    column: "EconomicActivityId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_OrganizationTaxMode_TaxesModesId",
            //    table: "OrganizationTaxMode",
            //    column: "TaxesModesId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_RefreshTokens_UserId",
            //    table: "RefreshTokens",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Requisites_OrganizationId",
            //    table: "Requisites",
            //    column: "OrganizationId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserRoles_RoleId",
            //    table: "UserRoles",
            //    column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "BalanceSheets");

            //migrationBuilder.DropTable(
            //    name: "Certificates");

            //migrationBuilder.DropTable(
            //    name: "EconomicActivityOrganization");

            //migrationBuilder.DropTable(
            //    name: "FinancialResults");

            //migrationBuilder.DropTable(
            //    name: "Licenses");

            //migrationBuilder.DropTable(
            //    name: "Managements");

            //migrationBuilder.DropTable(
            //    name: "OrganizationsEconomicActivities");

            //migrationBuilder.DropTable(
            //    name: "OrganizationTaxMode");

            //migrationBuilder.DropTable(
            //    name: "RefreshTokens");

            //migrationBuilder.DropTable(
            //    name: "Requisites");

            //migrationBuilder.DropTable(
            //    name: "UserRoles");

            //migrationBuilder.DropTable(
            //    name: "EconomicActivities");

            //migrationBuilder.DropTable(
            //    name: "TaxesModes");

            //migrationBuilder.DropTable(
            //    name: "Organizations");

            //migrationBuilder.DropTable(
            //    name: "Roles");

            //migrationBuilder.DropTable(
            //    name: "Users");
        }
    }
}
