using BookLibrary.Endpoints;
using BookLibrary.Application;
using BookLibrary.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookContext>(o => o.UseSqlite("Data Source=books.db"));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddBookCommand>()); // this is where the handlers regisrter using di
// it is useful to register the handlers in the same assembly as the commands and queries
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookContext>();
    await BookSeeder.SeedAsync(db, count: 20_000);
}

app.Run();
