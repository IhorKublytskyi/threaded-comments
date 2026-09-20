namespace dZENcode.API.Validations;

public sealed record ConnectionStrings
{
	public string? DzenConnectionString { get; set; }
	
	public string? RedisConnectionString { get; set; }
}