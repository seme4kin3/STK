using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetOrganizationByIdQueryHandler: IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationByIdQueryHandler> _logger;
        private static readonly Dictionary<string, string> statusOrg = new Dictionary<string, string>
        {
            { "Active", "Действующая" },
            { "active", "Действующая" },
            { "Действующая организация", "Действующая" },
            { "LIQUIDATING", "Ликвидируется" },
            { "LIQUIDATED", "Ликвидирована" },
            { "BANKRUPT", "Банкротство" },
            { "REORGANIZING", "В процессе присоединения к другому юр.лицу, с последующей ликвидацией" }
        };

        private static readonly Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
            { "Paused", "Действие приостановалено" },
        };
        public GetOrganizationByIdQueryHandler(DataContext dataContext, ILogger<GetOrganizationByIdQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var organization = await _dataContext.Organizations
                    .AsNoTracking()
                    .Where(o => o.Id == query.Id)
                    .AsSplitQuery()
                    .Select(o => new OrganizationDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Address = $"{o.Address} {o.IndexAddress}",
                        StatusOrg = o.StatusOrg,
                        IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                        Requisites = new RequisiteDto
                        {
                            INN = o.Requisites.INN,
                            KPP = o.Requisites.KPP,
                            OGRN = o.Requisites.OGRN,
                            DateCreation = o.Requisites.DateCreation,
                            EstablishmentCreateName = o.Requisites.EstablishmentCreateName,
                            AuthorizedCapital = o.Requisites.AuthorizedCapital,
                        },
                        Managements = o.Managements.Select(m => new ManagementDto
                        {
                            FullName = m.FullName,
                            Position = m.Position,
                            INN = m.INN

                        }).ToList(),
                        EconomicActivities = o.OrganizationsEconomicActivities.Select(e => new SearchEconomicActivityDto
                        {
                            OKVDNumber = e.EconomicActivities.OKVDNumber,
                            Description = e.EconomicActivities.Description,
                        }).ToList(),
                        Certificates = o.Certificates.Select(c => new CertificateDto
                        {
                            Id = c.Id,
                            Applicant = c.Applicant,
                            Title = c.Title,
                            CertificationObject = c.CertificationObject,
                            Address = c.Address,
                            Country = c.Country,
                            DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                            DateOfIssueCertificate = c.DateOfIssueCertificate,
                            Status = statusCertificate.GetValueOrDefault(c.Status, c.Status),
                            Manufacturer = c.Manufacturer,
                        }).ToList(),
                        BalanceSheets = o.BalanceSheets.Select(bs => new BalanceSheetDto
                        {
                            Year = bs.Year,
                            AssetType = bs.AssetType,
                            NonCurrentActive = bs.NonCurrentActive,
                            CurrentActive = bs.CurrentActive,
                            CapitalReserves = bs.CapitalReserves,
                            LongTermLiabilities = bs.LongTermLiabilities,
                            ShortTermLiabilities = bs.ShortTermLiabilities
                        }).ToList(),
                        FinancialResults = o.FinancialResults
                        .Select(fr => new FinancialResultDto
                        {
                            Year = fr.Year,
                            Profit = new
                            {                         
                                fr.GrossProfitEarnings,
                                fr.SalesProfit,
                                fr.NetProfit,
                                fr.IncomeTaxe,
                                fr.TaxFee,
                            },
                            Revenue = new 
                            {
                                fr.Revenue,
                                fr.CostOfSales,
                                fr.GrossProfitRevenue
                            }
                        }).OrderBy(fr => fr.Year).ToList(),
                        Licenses = o.Licenses.Select(fr => new LicenseDto
                        {
                            NameTypeActivity = fr.NameTypeActivity,
                            NameOrganizationIssued = fr.NameOrganizationIssued,
                            SeriesNumber = fr.SeriesNumber,
                            DateOfIssue = fr.DateOfIssue,
                        }).ToList(),
                        TaxMode = o.TaxesModes.FirstOrDefault().Name
                    }).FirstOrDefaultAsync(cancellationToken);
 
                if(statusOrg.TryGetValue(organization.StatusOrg, out string status))
                {
                    organization.StatusOrg = status;
                }

                if (organization == null) { return null; }

                return organization;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
