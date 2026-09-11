using dZENcode.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddDbContext<DzenDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("DzenConnectionString"),
    options =>
    {
        options.EnableRetryOnFailure();
    });
});

var app = builder.Build();

await app.RunAsync();
