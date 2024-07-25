using MediatR;

namespace BuildingBlocks.CQRS
{
    public interface ICommand : IRequest<Unit> // void tipi ifade eder. CQRS'te yeni öğreniyorum bunu
    {
    }

    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
