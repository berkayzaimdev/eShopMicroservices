using MediatR;

namespace BuildingBlocks.CQRS.Requests
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
        where TResponse : notnull // null dönemez
    {
    }
}
