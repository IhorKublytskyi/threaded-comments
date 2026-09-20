using dZENcode.API.DTOs.Comments;
using dZENcode.API.ExceptionHandlers;
using dZENcode.API.Validations;
using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha;
using dZENcode.Application.Features.Captcha.DTOs;
using dZENcode.Application.Features.Comments;
using dZENcode.Application.Features.Comments.Commands;
using dZENcode.Application.Features.Comments.DTOs;
using dZENcode.Application.Features.Comments.Queries;
using dZENcode.Application.Features.Comments.Validators.CommandsValidators;
using dZENcode.Application.Features.Comments.Validators.QueryValidators;
using dZENcode.Application.Features.Dispatcher;
using dZENcode.Application.Features.Dispatcher.Behaviors;
using dZENcode.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAntiforgery();

// CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("http://127.0.0.1:5500")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

// DbContext
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<DzenDbContext>());

builder.Services.AddDbContext<DzenDbContext>((serviceProvider, options) =>
{
	string connectionString = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value.DzenConnectionString!;
	options.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());
});

// Options
builder.Services.AddOptions<CaptchaOptions>()
	.Bind(configuration.GetSection(nameof(CaptchaOptions)))
	.ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<CaptchaOptions>, CaptchaOptionsValidation>();

builder.Services.AddOptions<StorageOptions>()
	.Bind(configuration.GetSection(nameof(StorageOptions)))
	.ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidation>();

builder.Services.AddOptions<ConnectionStrings>()
	.Bind(configuration.GetSection(nameof(ConnectionStrings)))
	.ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<ConnectionStrings>, ConnectingStringsValidation>();

// Dispatcher
builder.Services.AddDispatcher(typeof(IInstructionDispatcher).Assembly);

// Pipeline behaviors
builder.Services.AddPipelineBehavior(typeof(ValidationBehavior<,>));

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider => ConnectionMultiplexer.Connect(
	serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value.RedisConnectionString!));

builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ICaptchaChallengeGenerator, CaptchaChallengeGenerator>();
builder.Services.AddSingleton<ICaptchaStorage, DistributedCaptchaStorage>();

// CreateCommentCommand 
builder.Services.AddScoped<IValidator<CreateCommentCommand>, CreateCommentCommandValidator>();
builder.Services.AddScoped<IValidator<CommentAttachment>, CommentAttachmentValidator>();
builder.Services.AddScoped<IFileProcessor, ImageProcessor>();
builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
builder.Services.AddSingleton<IFileStorage, LocalAttachmentsFileStorage>();

// GetCommentsQuery
builder.Services.AddScoped<IValidator<GetCommentsQuery>, GetCommentsQueryValidator>();

// GetCommentRepliesQuery
builder.Services.AddScoped<IValidator<GetCommentRepliesQuery>, GetCommentRepliesQueryValidator>();

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.UseAntiforgery();

// CORS
app.UseCors();

app.MapGet("/antiforgery/token",
	(IAntiforgery antiforgery, HttpContext httpContext) => antiforgery.GetAndStoreTokens(httpContext));

app.MapGet("/comments", async (
	[FromQuery] GetCommentsRequest parameters,
	IInstructionDispatcher dispatcher,
	CancellationToken cancellationToken = default) =>
{
	CommentQueryParameters queryParameters = new(parameters.Pagination, parameters.Sorting);

	GetCommentsQuery query = new(queryParameters);

	return await dispatcher.SendAsync(query, cancellationToken);
});

app.MapGet("/comments/{id:int}/replies", async (
	int id,
	IInstructionDispatcher dispatcher,
	CancellationToken cancellationToken = default) =>
{
	GetCommentRepliesQuery query = new(id);

	return await dispatcher.SendAsync(query, cancellationToken);
});

// Workaround for the following issue(since IFormFile File is optional) - https://github.com/dotnet/aspnetcore/issues/56234 
app.MapPost("/comments", async (
	HttpRequest httpRequest,
	[FromServices] IInstructionDispatcher dispatcher,
	CancellationToken cancellationToken = default) =>
{
	// if (httpRequest.HasFormContentType is false)
	//     return "Expected multipart/form-data";

	IFormCollection form = await httpRequest.ReadFormAsync(cancellationToken);

	IFormFile? file = form.Files.GetFile("File");

	CommentAttachment? attachment = null;

	if (file is {Length: > 0})
	{
		int bytesToRead = 0;
		byte[] buffer = new byte[file.Length];
		await using Stream stream = file.OpenReadStream();

		while (bytesToRead < file.Length)
		{
			bytesToRead += await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
		}

		ReadOnlyMemory<byte> memory = buffer;
		attachment = new CommentAttachment(memory, file.FileName);
	}

	CreateCommentCommand command = new(
		form["Username"].ToString(),
		form["Email"].ToString(),
		string.IsNullOrWhiteSpace(form["HomePageUrl"]) ? null : form["HomePageUrl"].ToString(),
		form["Body"].ToString(),
		new CaptchaChallengeAnswer(
			form["CaptchaAnswer.Token"].ToString(),
			form["CaptchaAnswer.Input"].ToString()),
		int.TryParse(form["ParentCommentId"], out int pid) ? pid : null,
		attachment);

	return await dispatcher.SendAsync(command, cancellationToken);
});

app.MapGet("/captcha", async (
	ICaptchaService captchaService,
	CancellationToken cancellationToken = default
) =>
{
	CaptchaResponse response = await captchaService.GenerateAsync(cancellationToken);

	return Results.Ok(response);
});

app.MapGet("/attachments/{**path}", async (
	string path,
	[FromServices] IFileStorage fileStorage,
	CancellationToken cancellationToken = default) =>
{
	FileWrapper file = await fileStorage.GetFileAsync(path, cancellationToken);

	string contentType = ResolveContentType(file.Extension);
	bool isImage = contentType.StartsWith("image/");

	return Results.File(
		file.Content,
		contentType,
		isImage ? null : $"attachment{file.Extension}");
});

await app.RunAsync();

static string ResolveContentType(string extension)
{
	return extension.ToLowerInvariant() switch
	{
		".jpg" or ".jpeg" => "image/jpeg",
		".png" => "image/png",
		".gif" => "image/gif",
		".txt" => "text/plain; charset=utf-8",
		_ => "application/octet-stream"
	};
}