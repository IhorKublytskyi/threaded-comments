namespace dZENcode.Application.Abstractions;

public interface IFileProcessor
{
	byte[] Process(ReadOnlyMemory<byte> source);
}