
namespace Catalog.API.Products.GetProducts
{
    public record GetProductsQuery() : IQuery<GetProductsResult>; // GetProductsResult döndürecek bir sınıf
    public record GetProductsResult(IEnumerable<Product> Products); // IEnumerable türünden değer döndürecek result sınıfı

    internal class GetProductsQueryHandler(
        IDocumentSession session,
        ILogger<GetProductsQueryHandler> logger) // session ve logger enjekte ettik
        : IQueryHandler<GetProductsQuery, GetProductsResult>
    {
        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductsQueryHandler.Handle called with {@Query}", query);
            var products = await session.Query<Product>().ToListAsync(cancellationToken);

            return new GetProductsResult(products);
        }
    }
}
