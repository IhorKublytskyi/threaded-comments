namespace dZENcode.Application.Abstractions.Exceptions;

public class BadRequestException(
	string message) : Exception(message)
{
}