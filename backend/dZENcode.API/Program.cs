using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha;
using dZENcode.Application.Features.Captcha.DTOs;
using dZENcode.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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

builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
{
    return ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnectionString"));
});

builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ICaptchaChallengeGenerator, CaptchaChallengeGenerator>();
builder.Services.AddSingleton<ICaptchaStorage, DistributedCaptchaStorage>();

builder.Services.Configure<CaptchaOptions>(configuration.GetSection(nameof(CaptchaOptions)));

var app = builder.Build();

app.MapGet("/captcha", async(
    ICaptchaService captchaService,
    CancellationToken cancellationToken = default
) =>
{
    var response = await captchaService.GenerateAsync(cancellationToken);

    return Results.Ok(response);
});

await app.RunAsync();
