using MediatR;
using STK.Domain.Entities;


namespace STK.Application.Queries
{
    public class GetAllPredictAisQuery : IRequest<List<PredictAi>> { }
}
