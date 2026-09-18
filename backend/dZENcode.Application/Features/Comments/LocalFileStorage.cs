using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Comments.DTOs;

namespace dZENcode.Application.Features.Comments;

public class LocalAttachmentsFileStorage : IFileStorage
{
    private const string FilesDirectoryName = "Attachments"; 

    public Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> SaveAsync(ReadOnlyMemory<byte> source, string fileName, string uniqueDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uniqueDirectory))
        {
            throw new ArgumentNullException(nameof(uniqueDirectory));
        }

        string directory = Path.Combine(Environment.CurrentDirectory, FilesDirectoryName, uniqueDirectory);

        if (Directory.Exists(directory) is false)
        {
            Directory.CreateDirectory(directory);
        }

        string filename = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        int count = Directory
            .EnumerateFiles(directory)
            .Count(f => Path
                        .GetFileNameWithoutExtension(fileName)
                        .StartsWith(filename));
        // TODO: replace filename to Guid.NewGuid().ToString()
        string uniqueFileName = count > 0
        ? $"{filename}_{count}{extension}"
        : $"{filename}{extension}";

        string destinationPath = Path.Combine(directory, uniqueFileName);

        await using FileStream fileStream = File.Create(destinationPath);
        
        await fileStream.WriteAsync(source, cancellationToken);

        return destinationPath;
    }
}
