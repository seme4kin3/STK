using MediatR;
using Microsoft.Extensions.Logging;
using STK.Application.Commands;
using STK.Persistance;
using STK.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using STK.Application.Middleware;

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
            if (request.Organization == null)
                throw new ArgumentNullException(nameof(request.Organization));

            if (string.IsNullOrWhiteSpace(request.Organization.Name))
                throw new ArgumentException("Organization name is required.", nameof(request.Organization.Name));

            if (request.Organization.RequisiteDto == null || string.IsNullOrWhiteSpace(request.Organization.RequisiteDto.INN))
                throw new ArgumentException("Organization INN is required.", nameof(request.Organization.RequisiteDto));

            static string? N(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

            try
            {
                var inn = request.Organization?.RequisiteDto?.INN?.Trim();

                if (!string.IsNullOrWhiteSpace(inn))
                {
                    var publicOrgWithSameInn = await _dataContext.Organizations
                        .AsNoTracking()
                        .Where(o => o.Requisites != null && o.Requisites.INN == inn)
                        .Where(o => !_dataContext.UserCreatedOrganizations
                            .Any(uc => uc.OrganizationId == o.Id))
                        .Select(o => new { o.Id, o.Name })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (publicOrgWithSameInn != null)
                    {
                        _logger.LogWarning(
                            "Орг с ИНН {Inn} уже существует как общедоступная (OrgId={OrgId})",
                            inn, publicOrgWithSameInn.Id);

                        throw DomainException.Conflict(
                            $"Организация с ИНН {inn} уже существует. Найдите ее в поиске");
                    }
                }

                var existing = await _dataContext.OrganizationDownload
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Inn == inn, cancellationToken);

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

                var linkExists = await _dataContext.UserCreatedOrganizations
                    .AsNoTracking()
                    .AnyAsync(x => x.OrganizationId == orgId && x.UserId == request.UserId, cancellationToken);

                if (linkExists)
                    throw DomainException.Conflict("Для указанного пользователя данная организация уже существует");

                _dataContext.UserCreatedOrganizations.Add(new UserCreatedOrganization
                {
                    OrganizationId = orgId,
                    UserId = request.UserId,
                    DateAdded = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
            catch (DomainException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                // защита от гонок: если сработал уникальный индекс
                if (IsUniqueLinkViolation(ex))
                    throw DomainException.Conflict("Для указанного пользователя данная организация уже существует");

                _logger.LogError(ex, "DB error while creating organization for user {UserId}", request.UserId);
                throw new DomainException("Ошибка при сохранении организации.", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create organization for user {UserId}", request.UserId);
                throw new DomainException("Произошла ошибка при создании организации.", StatusCodes.Status500InternalServerError);
            }
        }


        private static bool IsUniqueLinkViolation(DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return msg.Contains("IX_UserCreatedOrganizations_UserId_OrganizationId", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
        }

    }
}
