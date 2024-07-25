using BuildingBlocks.CQRS.Requests;
using MediatR;

namespace BuildingBlocks.CQRS.Handlers
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
        where TResponse : notnull
    {
    }
}
