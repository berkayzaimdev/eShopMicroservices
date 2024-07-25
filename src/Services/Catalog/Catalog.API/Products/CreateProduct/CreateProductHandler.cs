using MediatR;

namespace Catalog.API.Products.CreateProduct
{
    public record CreateProductCommand(string Name, ICollection<string> Category, string Description, string ImageFile, decimal Price) : IRequest<CreateProductResult>;
    public record CreateProductResult(Guid Id); // başarılı ise Id'sini döndür
    internal class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResult> // sadece o assembly'de geçerli olması için internal yaptık çünkü başka bir yerde çağırmayacağız
    {
        public Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
