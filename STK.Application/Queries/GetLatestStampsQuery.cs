using MediatR;
using STK.Application.DTOs;


namespace STK.Application.Queries
{
    public class GetLatestStampsQuery : IRequest<IReadOnlyList<StampDto>>
    {
    }
}
