using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetAllPredictAisQueryHandler : IRequestHandler<GetAllPredictAisQuery, List<PredictAi>>
    {
        private readonly DataContext _dataContext;

        public GetAllPredictAisQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<PredictAi>> Handle(GetAllPredictAisQuery request, CancellationToken cancellationToken)
        {
            return await _dataContext.PredictAi.ToListAsync(cancellationToken);
        }
    }
}
