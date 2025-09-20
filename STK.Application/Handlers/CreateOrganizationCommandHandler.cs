using MediatR;
using Microsoft.Extensions.Logging;
using STK.Application.Commands;
using STK.Persistance;
using STK.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace STK.Application.Handlers
{
    public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Unit>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<CreateOrganizationCommandHandler> _logger;

        public CreateOrganizationCommandHandler(DataContext dataContext, ILogger<CreateOrganizationCommandHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Organization == null)
                    throw new ArgumentNullException(nameof(request.Organization));

                if (string.IsNullOrWhiteSpace(request.Organization.Name))
                    throw new ArgumentException("Organization name is required.", nameof(request.Organization.Name));

                if (request.Organization.RequisiteDto == null || string.IsNullOrWhiteSpace(request.Organization.RequisiteDto.INN))
                    throw new ArgumentException("Organization INN is required.", nameof(request.Organization.RequisiteDto));

                // локальная нормализация строк только для присвоений (поведение поиска не меняем)
                static string? N(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

                var existing = await _dataContext.OrganizationDownload
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Inn == request.Organization.RequisiteDto.INN, cancellationToken);

                var orgId = existing?.Id ?? Guid.NewGuid();

                if (existing == null)
                {
                    var orgDownload = new OrganizationDownload
                    {
                        Id = orgId,
                        Name = N(request.Organization.Name),
                        FullName = N(request.Organization.FullName),
                        Address = N(request.Organization.Address),
                        IndexAddress = N(request.Organization.IndexAddress),
                        PhoneNumber = N(request.Organization.PhoneNumber),
                        Email = N(request.Organization.Email),
                        Website = N(request.Organization.Website),
                        Inn = N(request.Organization.RequisiteDto?.INN),
                        Kpp = N(request.Organization.RequisiteDto?.KPP),
                        Ogrn = N(request.Organization.RequisiteDto?.OGRN),
                        IsLoad = false
                    };

                    _dataContext.Organizations.Add(new Organization { Id = orgId });
                    _dataContext.OrganizationDownload.Add(orgDownload);
                }

                _dataContext.UserCreatedOrganizations.Add(new UserCreatedOrganization
                {
                    OrganizationId = orgId,
                    UserId = request.UserId,
                    DateAdded = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create organization for user {UserId}", request.UserId);
                throw new ApplicationException("An error occurred while creating the organization.", ex);
            }
        }
    }
}
