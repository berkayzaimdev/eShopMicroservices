using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddCarter(); // problemin çözümü için carter kütüphanesini buildingblocks yerine catalog.api projesine kurduk

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions(); // performans artışı sağlar

builder.Services.AddExceptionHandler<CustomExceptionHandler>(); // container-side

var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler(options =>{ }); // app-side

await app.RunAsync();
