using BuildingBlocks.Concretes;

namespace Catalog.API.Products.CreateProduct;

public record CreateProductCommand(string Name, ICollection<string> Category, string Description, string ImageFile, decimal Price) : ICommand<CreateProductResult>;
public record CreateProductResult(Guid Id); // başarılı ise Id'sini döndür
internal class CreateProductCommandHandler(IDocumentSession session) : ICommandHandler<CreateProductCommand, CreateProductResult> // sadece o assembly'de geçerli olması için internal yaptık çünkü başka bir yerde çağırmayacağız
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var builder = new EntityBuilder<CreateProductCommand, Product>();

        var product = builder.Build(command);

        session.Store(product);

        await session.SaveChangesAsync(cancellationToken);

        return new CreateProductResult(product.Id);
    }
}
