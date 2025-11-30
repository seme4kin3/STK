using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetLatestStampsQueryHandler : IRequestHandler<GetLatestStampsQuery, IReadOnlyList<StampDto>>
    {
        private readonly DataContext _dataContext;

        public GetLatestStampsQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IReadOnlyList<StampDto>> Handle(GetLatestStampsQuery query, CancellationToken cancellationToken)
        {
            var stamps = await _dataContext.Stamps
                .OrderByDescending(s => s.Registration)
                .Take(50)
                .Select(s => new StampDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    StampNum = s.StampNum,
                    StampStatus = s.StampStatus,
                    Contragent = s.Contragent,
                    Place = s.Place,
                    Status = s.Status,
                    Registration = s.Registration,
                    Validity = s.Validity,
                    Usage = s.Usage,
                    OrganizationId = s.OrganizationId
                })
                .ToListAsync(cancellationToken);

            return stamps;
        }
    }
}
