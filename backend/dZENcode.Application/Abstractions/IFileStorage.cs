using dZENcode.Application.Features.Comments;

namespace dZENcode.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(ReadOnlyMemory<byte> source, string fileName, string directory, CancellationToken cancellationToken = default);

    Task<FileWrapper> GetFileAsync(string path, CancellationToken cancellationToken = default); 
}
