using dZENcode.Application.Abstractions.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace dZENcode.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(
	IProblemDetailsService problemDetailsService,
	ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		httpContext.Response.StatusCode = exception switch
		{
			BadRequestException badRequestException => StatusCodes.Status400BadRequest,
			NotFoundException => StatusCodes.Status404NotFound,
			_ => StatusCodes.Status500InternalServerError
		};

		if (httpContext.Response.StatusCode is StatusCodes.Status500InternalServerError)
		{
			logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
		}

		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			Exception = exception,
			ProblemDetails = new ProblemDetails
			{
				Title = "An error occurred",
				Detail = exception is BadRequestException or NotFoundException
					? exception.Message
					: "An unexpected error occurred",
				Status = httpContext.Response.StatusCode
			}
		});
	}
}