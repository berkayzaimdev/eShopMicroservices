
using System.Reflection;

namespace Catalog.API.Products.UpdateProduct;


public record UpdateProductCommand(Guid Id, string Name, ICollection<string> Category, string Description, string ImageFile, decimal Price) : ICommand<UpdateProductResult>;
public record UpdateProductResult(bool isSuccess);
internal class UpdateProductCommandHandler
    (IDocumentSession session, ILogger<UpdateProductCommandHandler> logger)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("UpdateProductCommandHandler.Handle called with {@Command}", command);

        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException();
        }

        UpdateProperties(product, command);

        session.Update(product);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(true);
    }

    private void UpdateProperties(object target, object source)
    {
        Type targetType = target.GetType();
        Type sourceType = source.GetType();

        PropertyInfo[] sourceProperties = sourceType.GetProperties();

        foreach (PropertyInfo sourceProperty in sourceProperties)
        {
            PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name);
            if (targetProperty != null && targetProperty.CanWrite)
            {
                object value = sourceProperty.GetValue(source);
                targetProperty.SetValue(target, value);
            }
        }
    }
}
