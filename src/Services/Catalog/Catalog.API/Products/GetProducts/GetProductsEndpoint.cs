
using Catalog.API.Products.CreateProduct;

namespace Catalog.API.Products.GetProducts;

// public record GetProductsRequest(); // şuan request tarafında bir işimiz olmadığı için tanımlamadık
public record GetProductsResponse(IEnumerable<Product> Products);
public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery()); // handler'a query'i gönderdik

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
