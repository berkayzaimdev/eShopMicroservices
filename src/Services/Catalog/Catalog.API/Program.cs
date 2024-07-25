var builder = WebApplication.CreateBuilder(args);


var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});

builder.Services.AddCarter(); // problemin çözümü için carter kütüphanesini buildingblocks yerine catalog.api projesine kurduk

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions(); // performans artışı sağlar

var app = builder.Build();

app.MapCarter();

await app.RunAsync();
