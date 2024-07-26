using BuildingBlocks.Behaviours;

var builder = WebApplication.CreateBuilder(args);


var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddCarter(); // problemin çözümü için carter kütüphanesini buildingblocks yerine catalog.api projesine kurduk

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions(); // performans artışı sağlar

var app = builder.Build();

app.MapCarter();

await app.RunAsync();
