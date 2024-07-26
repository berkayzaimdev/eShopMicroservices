using BuildingBlocks.CQRS.Requests;
using FluentValidation;
using MediatR;
using System.Windows.Input;

namespace BuildingBlocks.Behaviours;

public class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> Validators) // Valide edeceği nesne TRequest olan IValidator'ları tutan bir enumerable
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse> // TRequest'in ICommand'e eşit olmasını garantiledik, Query'ler için gereksiz bir validation yapmayacak
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request); // TRequest'in validasyonlarının bulunduğu bir validasyon içeriği tanımladık

        var validationResults = await Task.WhenAll(Validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any()) // meydana gelen hataları varsa where ile yakalama. her validasyon constrainti için birden çok hata olabilir o yüzden liste!
            .SelectMany(r => r.Errors) // ienumerable'dan tüm elemanları tek tek alır
            .ToList();

        if(failures.Any()) // hata varsa hata fırlat
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
