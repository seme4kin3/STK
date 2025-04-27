using MediatR;
using STK.Application.DTOs;


namespace STK.Application.Queries
{
    public class GetListTendersQuery : IRequest<IReadOnlyList<TenderDto>>
    {
    }
}
