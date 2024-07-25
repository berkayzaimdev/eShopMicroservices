using MediatR;

namespace BuildingBlocks.CQRS.Requests
{
    public interface ICommand : IRequest<Unit> // void tipi ifade eder. CQRS'te yeni öğreniyorum bunu
    {
    }

    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
