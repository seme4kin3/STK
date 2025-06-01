//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using STK.Application.DTOs.NotificationDto;
//using STK.Application.Queries;
//using STK.Persistance;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace STK.Application.Handlers
//{
//    public class GetUserChangesQueryHandler : IRequestHandler<GetUserChangesQuery, ChangeNotificationDto>
//    {
//        private readonly DataContext _dataContext;

//        public GetUserChangesQueryHandler(DataContext dataContext)
//        {
//            _dataContext = dataContext;
//        }

//        public async Task<ChangeNotificationDto> Handle(GetUserChangesQuery request, CancellationToken cancellationToken)
//        {
//            // Получаем избранные записи пользователя
//            var orgIds = await _dataContext.UsersFavoritesOrganizations
//                .Where(ufo => ufo.UserId == request.UserId)
//                .Select(ufo => ufo.OrganizationId)
//                .ToListAsync(cancellationToken);

//            var certIds = await _dataContext.UsersFavoritesCertificates
//                .Where(ufc => ufc.UserId == request.UserId)
//                .Select(ufc => ufc.CertificateId)
//                .ToListAsync(cancellationToken);

//            // Получаем изменения по избранным записям
//            var auditLogs = await _dataContext.AuditLog
//                .Where(a => a.ChangedAt >= request.DateSince &&
//                           ((a.TableName == "Organizations" && orgIds.Contains(a.RecordId)) ||
//                            (a.TableName == "Certificates" && certIds.Contains(a.RecordId))))
//                .OrderByDescending(a => a.ChangedAt)
//                .ToListAsync(cancellationToken);

//            // Обрабатываем изменения организаций
//            var orgChanges = auditLogs
//                .Where(a => a.TableName == "Organizations")
//                .GroupBy(a => a.RecordId) // Группируем по ID организации
//                .Select(g => g.First()) // Берем последнее изменение
//                .Select(a => new NotificationOrgDto
//                {
//                    Id = a.RecordId,
//                    FullName = ExtractNameFromJson(a.NewData ?? a.OldData),
//                    //ChangedAt = a.ChangedAt
//                })
//                .ToList();

//            // Обрабатываем изменения сертификатов
//            var certChanges = auditLogs
//                .Where(a => a.TableName == "Certificates")
//                .GroupBy(a => a.RecordId) // Группируем по ID сертификата
//                .Select(g => g.First()) // Берем последнее изменение
//                .Select(a => new NotificationCertDto
//                {
//                    Id = a.RecordId,
//                    Title = ExtractNameFromJson(a.NewData ?? a.OldData),
//                    //OrganizationId = a.RelatedOrganizationId ?? Guid.Empty,
//                    //OrganizationName = await GetOrganizationName(a.RelatedOrganizationId, cancellationToken),
//                    //ChangedAt = a.ChangedAt
//                })
//                .ToList();

//            return new ChangeNotificationDto
//            {
//                TotalChanges = orgChanges.Count + certChanges.Count,
//                ChangedOrganizations = orgChanges,
//                ChangedCertificates = certChanges
//            };
//        }

//        private string ExtractNameFromJson(string jsonData)
//        {
//            if (string.IsNullOrEmpty(jsonData))
//                return "Unknown";

//            try
//            {
//                using var doc = JsonDocument.Parse(jsonData);
//                if (doc.RootElement.TryGetProperty("FullName", out var nameProp) &&
//                    nameProp.ValueKind == JsonValueKind.String)
//                {
//                    return nameProp.GetString() ?? "Unknown";
//                }
//                return "Unknown";
//            }
//            catch
//            {
//                return "Unknown";
//            }
//        }

//        //private async Task<string> GetOrganizationName(Guid? organizationId, CancellationToken ct)
//        //{
//        //    if (!organizationId.HasValue)
//        //        return "Unknown";

//        //    var auditLog = await _context.AuditLogs
//        //        .Where(a => a.TableName == "Organizations" && a.RecordId == organizationId)
//        //        .OrderByDescending(a => a.ChangedAt)
//        //        .FirstOrDefaultAsync(ct);

//        //    return auditLog != null ? ExtractNameFromJson(auditLog.NewData ?? auditLog.OldData) : "Unknown";
//        //}
//    }
//}
