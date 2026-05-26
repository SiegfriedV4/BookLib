using BookLibrary.Endpoints;
using BookLibrary.Application;
using BookLibrary.Domain;
using BookLibrary.Infrastructure;
using System.Diagnostics;

const string connectionString = "Data Source=books.db";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IBookRepository>(_ => new DapperBookRepository(connectionString));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddBookCommand>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// Timing middleware — adds X-Response-Time-Ms header to every response
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    context.Response.OnStarting(() =>
    {
        sw.Stop();
        context.Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
        return Task.CompletedTask;
    });
    await next();
});

app.MapBookEndpoints();

// Seed large dataset on first run
await BookSeeder.SeedAsync(connectionString, count: 20_000);

app.Run();
