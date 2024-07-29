namespace Catalog.API.Products.GetProducts;

public record GetProductsQuery(int? PageNumber = 1, int? PageSize = 10) : IQuery<GetProductsResult>; // GetProductsResult döndürecek bir sınıf
public record GetProductsResult(IEnumerable<Product> Products); // IEnumerable türünden değer döndürecek result sınıfı

internal class GetProductsQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Product>()
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetProductsResult(products);
    }
}
