var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter(); // problemin çözümü için carter kütüphanesini buildingblocks yerine catalog.api projesine kurduk
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

app.MapCarter();

await app.RunAsync();
