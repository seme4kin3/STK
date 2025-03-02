using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetOrganizationByIdQueryHandler: IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
    {
        private readonly DataContext _dataContext;

        public GetOrganizationByIdQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken)
        {
            var organization = await _dataContext.Organizations
                .AsNoTracking()
                .Include(o => o.Requisites)
                .Include(o => o.EconomicActivities)
                .Include(o => o.Managements)
                .Include(o => o.Certificates)
                .Include(o => o.Licenses)
                .Include(o => o.FinancialResults)
                .Include(o => o.BalanceSheets)
                .Include(o => o.TaxesModes)
                .FirstOrDefaultAsync(o => o.Id == query.Id);

            if (organization == null) { return null; }

            var response = new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                FullName = organization.FullName,
                Address = $"{organization.Address} {organization.IndexAddress}",
                Requisites = new RequisiteDto
                {
                    INN = organization.Requisites.INN,
                    KPP = organization.Requisites.KPP,
                    OGRN = organization.Requisites.OGRN,
                    DateCreation = organization.Requisites.DateCreation,
                    EstablishmentCreateName = organization.Requisites.EstablishmentCreateName,
                    AuthorizedCapital = organization.Requisites.AuthorizedCapital,
                },
                Managements = organization.Managements.Select(m => new ManagementDto
                {
                    FullName = $"{m.FirstName} {m.LastName}",
                    Position = m.Position,
                    INN = m.INN

                }).ToList(),
                EconomicActivities = organization.EconomicActivities.Select(e => new SearchEconomicActivityDto
                {
                    OKVDNumber = e.OKVDNumber,
                    Description = e.Description,
                }).ToList(),
                Certificate = organization.Certificates.Select(c => new CertificateDto
                {
                    Applicant = c.Applicant,
                    Title = c.Title,
                    CertificationObject = c.CertificationObject,
                    Address = c.Address,
                    Country = c.Country,
                    DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                    DateOfIssueCertificate = c.DateOfIssueCertificate,
                    Status = c.Status,
                    Manufacturer = c.Manufacturer,
                }).ToList(),
                BalanceSheets = organization.BalanceSheets.Select(bs => new BalanceSheetDto
                {
                    Year = bs.Year,
                    AssetType = bs.AssetType,
                    NonCurrentActive = bs.NonCurrentActive,
                    CurrentActive = bs.CurrentActive,
                    CapitalReserves = bs.CapitalReserves,
                    LongTermLiabilities = bs.LongTermLiabilities,
                    ShortTermLiabilities = bs.ShortTermLiabilities
                }).ToList(),
                FinancialResults = organization.FinancialResults.Select(fr => new FinancialResultDto
                {
                    Type = fr.Type,
                    Year = fr.Year,
                    Revenue = fr.Revenue,
                    CostOfSales = fr.CostOfSales,
                    GrossProfitEarnings = fr.GrossProfitEarnings,
                    GrossProfitRevenue = fr.GrossProfitRevenue,
                    SalesProfit = fr.SalesProfit,
                    ProfitBeforeTax = fr.ProfitBeforeTax,
                    NetProfit = fr.NetProfit,
                    IncomeTaxe = fr.IncomeTaxe,
                    TaxFee = fr.TaxFee
                }).ToList(),
                Licenses = organization.Licenses.Select(fr => new LicenseDto
                {
                    NameTypeActivity = fr.NameTypeActivity,
                    NameOrganizationIssued = fr.NameOrganizationIssued,
                    SeriesNumber = fr.SeriesNumber,
                    DateOfIssue = fr.DateOfIssue,
                }).ToList(),
                TaxModes = organization.TaxesModes.Select(tm => new TaxModeDto
                {
                    Name = tm.Name
                }).ToList()
            };

            return response;
        }
    }
}
