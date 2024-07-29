
using Catalog.API.Products.CreateProduct;

namespace Catalog.API.Products.GetProducts;

public record GetProductsRequest(int? PageNumber = 1, int? PageSize = 10);
public record GetProductsResponse(IEnumerable<Product> Products);
public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender, [AsParameters] GetProductsRequest request) =>
        {
            var query = request.Adapt<GetProductsQuery>();

            var result = await sender.Send(query); 

            var response = result.Adapt<GetProductsResponse>();

            return Results.Ok(response); // OK mesajı ile response'u döndür
        })
        .WithName("GetProduct") 
        .Produces<CreateProductResponse>(StatusCodes.Status200OK) 
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Product") 
        .WithDescription("Get Product");
    }
}
