namespace Catalog.API.Products.CreateProduct;

public record CreateProductRequest(string Name, ICollection<string> Category, string Description, string ImageFile, decimal Price) : ICommand<CreateProductResult>;
public record CreateProductResponse(Guid Id);
public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateProductCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateProductResponse>();

            return Results.Created($"/products/{response.Id}", response);
        })
        .WithName("CreateProduct") // metot ismi
        .Produces<CreateProductResponse>(StatusCodes.Status201Created) // başarı durumu
        .ProducesProblem(StatusCodes.Status400BadRequest) // hata durumu
        .WithSummary("Create Product") // metot özeti
        .WithDescription("Create Product"); // metot açıklaması
    }
}
