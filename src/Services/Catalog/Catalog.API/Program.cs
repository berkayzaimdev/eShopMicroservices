var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter(); // problemin ��z�m� i�in carter k�t�phanesini buildingblocks yerine catalog.api projesine kurduk
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

app.MapCarter();

await app.RunAsync();
